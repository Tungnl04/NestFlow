using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NestFlow.Application.Services.Interfaces;
using NestFlow.Models;

namespace NestFlow.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly NestFlowSystemContext _context;
        private readonly IWalletService _walletService;
        private readonly IEmailService _emailService;
        private readonly INotificationService? _notificationService;
        private readonly ILogger<BookingController> _logger;

        public BookingController(
            NestFlowSystemContext context,
            IWalletService walletService,
            IEmailService emailService,
            ILogger<BookingController> logger,
            INotificationService? notificationService = null)
        {
            _context = context;
            _walletService = walletService;
            _emailService = emailService;
            _logger = logger;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Landlord chấp nhận booking
        /// </summary>
        [HttpPost("approve/{bookingId}")]
        public async Task<IActionResult> ApproveBooking(long bookingId)
        {
            try
            {
                _logger.LogInformation($"=== START ApproveBooking {bookingId} ===");
                
                var landlordId = HttpContext.Session.GetInt32("UserId");
                _logger.LogInformation($"LandlordId from session: {landlordId}");
                
                if (landlordId == null)
                {
                    return Unauthorized(new { success = false, message = "Vui lòng đăng nhập" });
                }

                _logger.LogInformation("Loading booking from database...");
                var booking = await _context.Bookings
                    .Include(b => b.Property)
                    .Include(b => b.Renter)
                    .FirstOrDefaultAsync(b => b.BookingId == bookingId);

                if (booking == null)
                {
                    _logger.LogWarning($"Booking {bookingId} not found");
                    return NotFound(new { success = false, message = "Không tìm thấy booking" });
                }

                _logger.LogInformation($"Booking found. PropertyId: {booking.PropertyId}, RenterId: {booking.RenterId}, Status: {booking.Status}");

                // Kiểm tra quyền
                if (booking.Property.LandlordId != landlordId)
                {
                    _logger.LogWarning($"Landlord {landlordId} không có quyền với booking {bookingId}");
                    return Forbid();
                }

                // Kiểm tra trạng thái
                if (booking.Status != "Pending")
                {
                    _logger.LogWarning($"Booking {bookingId} đã được xử lý. Status: {booking.Status}");
                    return BadRequest(new { success = false, message = "Booking đã được xử lý" });
                }

                _logger.LogInformation("Finding payment...");
                // Tìm payment tương ứng
                var payment = await _context.Payments
                    .Where(p => p.PaymentType == "Deposit" && 
                                p.Status == "Completed" &&
                                p.PayerUserId == booking.RenterId &&
                                p.LandlordId == booking.Property.LandlordId)
                    .OrderByDescending(p => p.CreatedAt)
                    .FirstOrDefaultAsync();

                if (payment == null)
                {
                    _logger.LogError("Payment not found!");
                    return BadRequest(new { success = false, message = "Không tìm thấy thanh toán" });
                }

                _logger.LogInformation($"Payment found. PaymentId: {payment.PaymentId}, Amount: {payment.Amount}");

                _logger.LogInformation("Getting or creating wallet...");
                // Lấy hoặc tạo ví cho landlord
                var wallet = await _walletService.GetOrCreateWalletAsync(booking.Property.LandlordId);
                _logger.LogInformation($"Wallet: WalletId={wallet.WalletId}, Available={wallet.AvailableBalance}, Locked={wallet.LockedBalance}");

                _logger.LogInformation("Transferring locked to available...");
                // Chuyển tiền từ locked sang available
                var transferSuccess = await _walletService.TransferLockedToAvailableAsync(
                    wallet.WalletId,
                    payment.Amount,
                    "booking",
                    bookingId,
                    $"Chấp nhận booking #{bookingId} - {booking.Property.Title}"
                );

                if (!transferSuccess)
                {
                    _logger.LogError("Transfer failed!");
                    return StatusCode(500, new { success = false, message = "Lỗi xử lý ví" });
                }

                _logger.LogInformation("Transfer successful. Updating booking status...");
                // Cập nhật trạng thái booking
                booking.Status = "Confirmed";
                booking.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Booking status updated to Confirmed");

                // Gửi thông báo cho renter (không throw nếu lỗi)
                try
                {
                    _logger.LogInformation($"Sending email to {booking.Renter.Email}...");
                    await _emailService.SendEmailAsync(
                        booking.Renter.Email,
                        "Booking được chấp nhận",
                        $"Chào {booking.Renter.FullName},<br><br>" +
                        $"Chủ nhà đã chấp nhận booking của bạn cho phòng: {booking.Property.Title}<br>" +
                        $"Ngày xem phòng: {booking.BookingDate:dd/MM/yyyy} lúc {booking.BookingTime}<br>" +
                        $"Vui lòng liên hệ chủ nhà để sắp xếp chi tiết.<br><br>" +
                        $"Trân trọng,<br>NestFlow Team"
                    );
                    _logger.LogInformation("Email sent successfully");

                    if (_notificationService != null)
                    {
                        _logger.LogInformation("Sending notification...");
                        await _notificationService.CreateAndSendNotificationAsync(
                            booking.RenterId,
                            "Booking được chấp nhận",
                            $"Chủ nhà đã chấp nhận booking của bạn cho phòng: {booking.Property.Title}",
                            "success",
                            $"/Room/Detail?id={booking.PropertyId}"
                        );
                        _logger.LogInformation("Notification sent successfully");
                    }
                }
                catch (Exception emailEx)
                {
                    _logger.LogError($"Error sending email/notification: {emailEx.Message}");
                    // Không throw - vì booking đã approve thành công
                }

                _logger.LogInformation("=== END ApproveBooking SUCCESS ===");
                
                return Ok(new
                {
                    success = true,
                    message = "Đã chấp nhận booking",
                    bookingId = booking.BookingId,
                    amount = payment.Amount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"=== ERROR ApproveBooking ===");
                _logger.LogError($"Exception: {ex.GetType().Name}");
                _logger.LogError($"Message: {ex.Message}");
                _logger.LogError($"StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    _logger.LogError($"InnerException: {ex.InnerException.Message}");
                }
                return StatusCode(500, new { 
                    success = false, 
                    message = "Có lỗi xảy ra khi chấp nhận booking",
                    error = ex.Message,
                    details = ex.InnerException?.Message
                });
            }
        }

        /// <summary>
        /// Landlord từ chối booking
        /// </summary>
        [HttpPost("reject/{bookingId}")]
        public async Task<IActionResult> RejectBooking(long bookingId, [FromBody] RejectBookingRequest request)
        {
            try
            {
                _logger.LogInformation($"=== START RejectBooking {bookingId} ===");
                
                var landlordId = HttpContext.Session.GetInt32("UserId");
                _logger.LogInformation($"LandlordId from session: {landlordId}");
                
                if (landlordId == null)
                {
                    return Unauthorized(new { success = false, message = "Vui lòng đăng nhập" });
                }

                _logger.LogInformation("Loading booking from database...");
                var booking = await _context.Bookings
                    .Include(b => b.Property)
                    .Include(b => b.Renter)
                    .FirstOrDefaultAsync(b => b.BookingId == bookingId);

                if (booking == null)
                {
                    _logger.LogWarning($"Booking {bookingId} not found");
                    return NotFound(new { success = false, message = "Không tìm thấy booking" });
                }

                _logger.LogInformation($"Booking found. Status: {booking.Status}");

                // Kiểm tra quyền
                if (booking.Property.LandlordId != landlordId)
                {
                    _logger.LogWarning($"Landlord {landlordId} không có quyền với booking {bookingId}");
                    return Forbid();
                }

                // Kiểm tra trạng thái
                if (booking.Status != "Pending")
                {
                    _logger.LogWarning($"Booking {bookingId} đã được xử lý. Status: {booking.Status}");
                    return BadRequest(new { success = false, message = "Booking đã được xử lý" });
                }

                _logger.LogInformation("Finding payment...");
                // Tìm payment tương ứng
                var payment = await _context.Payments
                    .Where(p => p.PaymentType == "Deposit" && 
                                p.Status == "Completed" &&
                                p.PayerUserId == booking.RenterId &&
                                p.LandlordId == booking.Property.LandlordId)
                    .OrderByDescending(p => p.CreatedAt)
                    .FirstOrDefaultAsync();

                if (payment == null)
                {
                    _logger.LogError("Payment not found!");
                    return BadRequest(new { success = false, message = "Không tìm thấy thanh toán" });
                }

                _logger.LogInformation($"Payment found. Amount: {payment.Amount}");

                _logger.LogInformation("Getting landlord wallet...");
                // Lấy ví landlord
                var wallet = await _walletService.GetOrCreateWalletAsync(booking.Property.LandlordId);
                _logger.LogInformation($"Wallet: WalletId={wallet.WalletId}, Locked={wallet.LockedBalance}");

                _logger.LogInformation("Releasing locked balance...");
                // Giải phóng locked balance
                var releaseSuccess = await _walletService.ReleaseLockedBalanceAsync(
                    wallet.WalletId,
                    payment.Amount,
                    "booking",
                    bookingId,
                    $"Từ chối booking #{bookingId} - {booking.Property.Title}"
                );

                if (!releaseSuccess)
                {
                    _logger.LogError("Release locked balance failed!");
                    return StatusCode(500, new { success = false, message = "Lỗi xử lý ví landlord" });
                }

                _logger.LogInformation("Refunding to renter...");
                // Hoàn tiền cho renter
                var refundSuccess = await _walletService.RefundToRenterAsync(
                    booking.RenterId,
                    payment.Amount,
                    "booking_refund",
                    bookingId,
                    $"Hoàn tiền booking #{bookingId} - {booking.Property.Title}"
                );

                if (!refundSuccess)
                {
                    _logger.LogError("Refund to renter failed!");
                    return StatusCode(500, new { success = false, message = "Lỗi hoàn tiền" });
                }

                _logger.LogInformation("Updating booking and payment status...");
                // Cập nhật trạng thái booking
                booking.Status = "Rejected";
                booking.Notes = $"{booking.Notes}\n[Lý do từ chối: {request.Reason}]";
                booking.UpdatedAt = DateTime.Now;

                // Cập nhật payment status
                payment.Status = "Refunded";

                await _context.SaveChangesAsync();
                _logger.LogInformation("Booking rejected and payment refunded");

                // Gửi thông báo cho renter (không throw nếu lỗi)
                try
                {
                    _logger.LogInformation($"Sending email to {booking.Renter.Email}...");
                    await _emailService.SendEmailAsync(
                        booking.Renter.Email,
                        "Booking bị từ chối - Đã hoàn tiền",
                        $"Chào {booking.Renter.FullName},<br><br>" +
                        $"Rất tiếc, chủ nhà đã từ chối booking của bạn cho phòng: {booking.Property.Title}<br>" +
                        $"Lý do: {request.Reason}<br><br>" +
                        $"Số tiền {payment.Amount:N0} VNĐ đã được hoàn vào ví của bạn.<br>" +
                        $"Bạn có thể sử dụng số tiền này để đặt phòng khác.<br><br>" +
                        $"Trân trọng,<br>NestFlow Team"
                    );
                    _logger.LogInformation("Email sent successfully");

                    if (_notificationService != null)
                    {
                        _logger.LogInformation("Sending notification...");
                        await _notificationService.CreateAndSendNotificationAsync(
                            booking.RenterId,
                            "Booking bị từ chối - Đã hoàn tiền",
                            $"Chủ nhà đã từ chối booking. Số tiền {payment.Amount:N0} VNĐ đã được hoàn vào ví của bạn.",
                            "warning",
                            "/Home/Profile"
                        );
                        _logger.LogInformation("Notification sent successfully");
                    }
                }
                catch (Exception emailEx)
                {
                    _logger.LogError($"Error sending email/notification: {emailEx.Message}");
                    // Không throw - vì booking đã reject thành công
                }

                _logger.LogInformation("=== END RejectBooking SUCCESS ===");
                
                return Ok(new
                {
                    success = true,
                    message = "Đã từ chối booking và hoàn tiền",
                    bookingId = booking.BookingId,
                    refundAmount = payment.Amount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"=== ERROR RejectBooking ===");
                _logger.LogError($"Exception: {ex.GetType().Name}");
                _logger.LogError($"Message: {ex.Message}");
                _logger.LogError($"StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    _logger.LogError($"InnerException: {ex.InnerException.Message}");
                }
                return StatusCode(500, new { 
                    success = false, 
                    message = "Có lỗi xảy ra khi từ chối booking",
                    error = ex.Message,
                    details = ex.InnerException?.Message
                });
            }
        }

        /// <summary>
        /// Lấy danh sách booking của landlord
        /// </summary>
        [HttpGet("landlord/bookings")]
        public async Task<IActionResult> GetLandlordBookings([FromQuery] string? status = null)
        {
            try
            {
                var landlordId = HttpContext.Session.GetInt32("UserId");
                if (landlordId == null)
                {
                    return Unauthorized(new { success = false, message = "Vui lòng đăng nhập" });
                }

                var query = _context.Bookings
                    .Include(b => b.Property)
                    .Include(b => b.Renter)
                    .Where(b => b.Property.LandlordId == landlordId);

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(b => b.Status == status);
                }

                var bookings = await query
                    .OrderByDescending(b => b.CreatedAt)
                    .Select(b => new
                    {
                        b.BookingId,
                        b.PropertyId,
                        PropertyTitle = b.Property.Title,
                        PropertyImage = b.Property.PropertyImages.FirstOrDefault()!.ImageUrl,
                        RenterName = b.Renter.FullName,
                        RenterPhone = b.Renter.Phone,
                        RenterEmail = b.Renter.Email,
                        b.BookingDate,
                        b.BookingTime,
                        b.Status,
                        b.Notes,
                        b.CreatedAt,
                        DepositAmount = b.Property.Deposit
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = bookings });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting landlord bookings: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra" });
            }
        }
    }

    public class RejectBookingRequest
    {
        public string Reason { get; set; } = "Không phù hợp";
    }
}
