using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NestFlow.Models;
using NestFlow.Application.Services.Interfaces;
using NestFlow.Application.Services;
using Net.payOS;
using Net.payOS.Types;

namespace NestFlow.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly NestFlowSystemContext _context;
        private readonly PayOS _payOS;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _emailService;
        private readonly INotificationService? _notificationService;

        public PaymentController(
            NestFlowSystemContext context,
            PayOS payOS,
            IHttpContextAccessor httpContextAccessor,
            IEmailService emailService,
            INotificationService? notificationService = null)
        {
            _context = context;
            _payOS = payOS;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Tạo link thanh toán PayOS cho booking
        /// </summary>
        [HttpPost("create-booking-payment")]
        public async Task<IActionResult> CreateBookingPayment([FromBody] CreateBookingPaymentRequest request)
        {
            try
            {
                // Get or create user
                long userId;
                var sessionUserId = HttpContext.Session.GetInt32("UserId");

                if (sessionUserId != null)
                {
                    // User đã login
                    userId = sessionUserId.Value;
                }
                else
                {
                    // User chưa login - validate thông tin
                    if (string.IsNullOrEmpty(request.FullName) || 
                        string.IsNullOrEmpty(request.Email) || 
                        string.IsNullOrEmpty(request.Phone))
                    {
                        return BadRequest(new { success = false, message = "Vui lòng điền đầy đủ thông tin liên hệ" });
                    }

                    // Tìm hoặc tạo user guest
                    var existingUser = await _context.Users
                        .FirstOrDefaultAsync(u => u.Email == request.Email || u.Phone == request.Phone);

                    if (existingUser != null)
                    {
                        userId = existingUser.UserId;
                    }
                    else
                    {
                        // Tạo user mới
                        var newUser = new User
                        {
                            Email = request.Email,
                            FullName = request.FullName,
                            Phone = request.Phone,
                            PasswordHash = Guid.NewGuid().ToString(), // Random password
                            UserType = "Renter",
                            IsVerified = false,
                            Status = "Active",
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        };

                        _context.Users.Add(newUser);
                        await _context.SaveChangesAsync();
                        userId = newUser.UserId;
                    }
                }

                // Get property info
                var property = await _context.Properties
                    .Include(p => p.Landlord)
                    .FirstOrDefaultAsync(p => p.PropertyId == request.PropertyId);

                if (property == null)
                {
                    Console.WriteLine($"Property not found: PropertyId={request.PropertyId}");
                    return NotFound(new { success = false, message = "Không tìm thấy bất động sản" });
                }

                Console.WriteLine($"Property found: {property.Title}, LandlordId={property.LandlordId}");

                // Verify user exists
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    Console.WriteLine($"User not found: UserId={userId}");
                    return NotFound(new { success = false, message = "Không tìm thấy thông tin người dùng" });
                }

                Console.WriteLine($"User found: {user.FullName}, UserId={userId}");

                // Calculate amount (deposit) - TEST TRƯỚC
                var amount = (int)(property.Deposit ?? 0);
                if (amount <= 0)
                {
                    Console.WriteLine($"Invalid deposit amount: {amount}");
                    return BadRequest(new { success = false, message = "Số tiền đặt cọc không hợp lệ" });
                }

                Console.WriteLine($"Deposit amount: {amount}");

                // TẠM THỜI COMMENT BOOKING ĐỂ TEST PAYOS
                // Create booking
                /*
                var booking = new Booking
                {
                    PropertyId = request.PropertyId,
                    RenterId = userId,
                    BookingDate = DateOnly.FromDateTime(request.BookingDate),
                    BookingTime = TimeOnly.FromDateTime(request.BookingDate),
                    Status = "Pending",
                    Notes = request.Notes,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                Console.WriteLine($"Creating booking: PropertyId={booking.PropertyId}, RenterId={booking.RenterId}, Date={booking.BookingDate}");

                _context.Bookings.Add(booking);
                
                try
                {
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"Booking created successfully: BookingId={booking.BookingId}");
                }
                catch (Exception saveEx)
                {
                    Console.WriteLine($"Error saving booking: {saveEx.Message}");
                    if (saveEx.InnerException != null)
                    {
                        Console.WriteLine($"Inner exception: {saveEx.InnerException.Message}");
                    }
                    throw;
                }
                */

                // FAKE BOOKING ID ĐỂ TEST
                long fakeBookingId = 999;
                Console.WriteLine($"Using fake booking ID: {fakeBookingId}");

                // Calculate amount (deposit)
                // var amount = (int)(property.Deposit ?? 0);
                // if (amount <= 0)
                // {
                //     return BadRequest(new { success = false, message = "Số tiền đặt cọc không hợp lệ" });
                // }

                // Create PayOS payment link
                long orderCode = long.Parse(DateTimeOffset.Now.ToString("ffffff"));

                ItemData item = new ItemData(
                    $"Dat coc {property.Title}",
                    1,
                    amount
                );
                List<ItemData> items = new List<ItemData> { item };

                var httpRequest = _httpContextAccessor.HttpContext?.Request ?? HttpContext.Request;
                var baseUrl = $"{httpRequest.Scheme}://{httpRequest.Host}";

                PaymentData paymentData = new PaymentData(
                    orderCode,
                    amount,
                    "Dat coc phong tro",
                    items,
                    $"{baseUrl}/api/Payment/payment-cancel?bookingId={fakeBookingId}",
                    $"{baseUrl}/api/Payment/payment-success?bookingId={fakeBookingId}"
                );

                CreatePaymentResult createPayment = await _payOS.createPaymentLink(paymentData);

                Console.WriteLine($"PayOS payment link created: {createPayment.checkoutUrl}");

                // Save payment record
                var payment = new Models.Payment
                {
                    PayerUserId = userId,
                    LandlordId = property.LandlordId,
                    PaymentType = "Deposit",
                    Amount = amount,
                    Provider = "PayOS",
                    ProviderOrderCode = orderCode.ToString(),
                    PayUrl = createPayment.checkoutUrl,
                    Status = "Pending",
                    CreatedAt = DateTime.Now
                };

                Console.WriteLine($"Creating payment record: PayerUserId={payment.PayerUserId}, LandlordId={payment.LandlordId}, Amount={payment.Amount}");

                _context.Payments.Add(payment);
                
                try
                {
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"Payment saved successfully: PaymentId={payment.PaymentId}");
                }
                catch (Exception paymentEx)
                {
                    Console.WriteLine($"Error saving payment: {paymentEx.Message}");
                    if (paymentEx.InnerException != null)
                    {
                        Console.WriteLine($"Payment inner exception: {paymentEx.InnerException.Message}");
                    }
                    throw;
                }

                return Ok(new
                {
                    success = true,
                    checkoutUrl = createPayment.checkoutUrl,
                    bookingId = fakeBookingId,
                    paymentId = payment.PaymentId
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating booking payment: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    Console.WriteLine($"Inner stack trace: {ex.InnerException.StackTrace}");
                }
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        /// <summary>
        /// Xử lý khi thanh toán thành công
        /// </summary>
        [HttpGet("payment-success")]
        public async Task<IActionResult> PaymentSuccess([FromQuery] long bookingId)
        {
            try
            {
                var booking = await _context.Bookings
                    .Include(b => b.Property)
                    .Include(b => b.Renter)
                    .FirstOrDefaultAsync(b => b.BookingId == bookingId);

                if (booking == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy booking" });
                }

                // Update booking status
                booking.Status = "Confirmed";
                booking.UpdatedAt = DateTime.Now;

                // Update payment status
                var payment = await _context.Payments
                    .Where(p => p.PaymentType == "Deposit" && p.Status == "Pending")
                    .OrderByDescending(p => p.CreatedAt)
                    .FirstOrDefaultAsync();

                if (payment != null)
                {
                    payment.Status = "Completed";
                    payment.PaidAt = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                // Send notification
                if (booking.Renter != null && !string.IsNullOrEmpty(booking.Renter.Email))
                {
                    try
                    {
                        await _emailService.SendEmailAsync(
                            booking.Renter.Email,
                            "Xác nhận đặt phòng thành công",
                            $"Chào {booking.Renter.FullName},<br><br>" +
                            $"Bạn đã đặt cọc thành công cho phòng: {booking.Property.Title}<br>" +
                            $"Số tiền: {booking.Property.Deposit:N0} VNĐ<br>" +
                            $"Chủ nhà sẽ liên hệ với bạn sớm nhất.<br><br>" +
                            $"Trân trọng,<br>NestFlow Team"
                        );

                        // Send in-app notification if service is available
                        if (_notificationService != null)
                        {
                            await _notificationService.CreateAndSendNotificationAsync(
                                booking.RenterId,
                                "Đặt phòng thành công",
                                $"Bạn đã đặt cọc thành công cho phòng: {booking.Property.Title}",
                                "success",
                                $"/Room/Detail?id={booking.PropertyId}"
                            );
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error sending email: {ex.Message}");
                    }
                }

                // Redirect to success page
                return Redirect($"/Room/Detail?id={booking.PropertyId}&bookingSuccess=true");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in payment success: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra" });
            }
        }

        /// <summary>
        /// Xử lý khi thanh toán bị hủy
        /// </summary>
        [HttpGet("payment-cancel")]
        public async Task<IActionResult> PaymentCancel([FromQuery] long bookingId)
        {
            try
            {
                var booking = await _context.Bookings
                    .FirstOrDefaultAsync(b => b.BookingId == bookingId);

                if (booking != null)
                {
                    booking.Status = "Cancelled";
                    booking.UpdatedAt = DateTime.Now;

                    var payment = await _context.Payments
                        .Where(p => p.PaymentType == "Deposit" && p.Status == "Pending")
                        .OrderByDescending(p => p.CreatedAt)
                        .FirstOrDefaultAsync();

                    if (payment != null)
                    {
                        payment.Status = "Cancelled";
                    }

                    await _context.SaveChangesAsync();
                }

                return Redirect($"/Room/Detail?id={booking?.PropertyId}&bookingCancelled=true");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in payment cancel: {ex.Message}");
                return Redirect("/Home/Index");
            }
        }

        /// <summary>
        /// Webhook để nhận thông báo từ PayOS
        /// </summary>
        [HttpPost("payos-webhook")]
        public async Task<IActionResult> PayOSWebhook([FromBody] WebhookType body)
        {
            try
            {
                WebhookData data = _payOS.verifyPaymentWebhookData(body);

                // Find payment by order code
                var payment = await _context.Payments
                    .FirstOrDefaultAsync(p => p.ProviderOrderCode == data.orderCode.ToString());

                if (payment != null)
                {
                    payment.Status = "Completed";
                    payment.PaidAt = DateTime.Now;
                    payment.RawWebhook = System.Text.Json.JsonSerializer.Serialize(data);

                    await _context.SaveChangesAsync();
                }

                return Ok(new { error = 0, message = "Ok", data = (object?)null });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Webhook error: {ex.Message}");
                return Ok(new { error = -1, message = "fail", data = (object?)null });
            }
        }

        /// <summary>
        /// Lấy thông tin thanh toán
        /// </summary>
        [HttpGet("payment-info/{orderCode}")]
        public async Task<IActionResult> GetPaymentInfo(long orderCode)
        {
            try
            {
                PaymentLinkInformation paymentInfo = await _payOS.getPaymentLinkInformation(orderCode);
                return Ok(new { success = true, data = paymentInfo });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Hủy link thanh toán
        /// </summary>
        [HttpPost("cancel-payment/{orderCode}")]
        public async Task<IActionResult> CancelPaymentLink(long orderCode)
        {
            try
            {
                PaymentLinkInformation cancelledPayment = await _payOS.cancelPaymentLink(orderCode);
                return Ok(new { success = true, data = cancelledPayment });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }

    // Request models
    public class CreateBookingPaymentRequest
    {
        public long PropertyId { get; set; }
        public DateTime BookingDate { get; set; }
        public string? Notes { get; set; }
        
        // Thông tin cho guest (không cần login)
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
    }
}
