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
        private readonly IWalletService _walletService;

        public PaymentController(
            NestFlowSystemContext context,
            PayOS payOS,
            IHttpContextAccessor httpContextAccessor,
            IEmailService emailService,
            IWalletService walletService,
            INotificationService? notificationService = null)
        {
            _context = context;
            _payOS = payOS;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;
            _notificationService = notificationService;
            _walletService = walletService;
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

                // Calculate amount (deposit) - ÁP DỤNG GIẢM GIÁ 500K
                var amount = (int)(property.Deposit ?? 0);
                if (amount <= 0)
                {
                    Console.WriteLine($"Invalid deposit amount: {amount}");
                    return BadRequest(new { success = false, message = "Số tiền đặt cọc không hợp lệ" });
                }

                Console.WriteLine($"Deposit amount: {amount}");

                decimal platformDiscount = 500000;
                amount = amount - (int)platformDiscount;
                if (amount < 0) amount = 0;

                Console.WriteLine($"Amount after discount: {amount}");

                // KIỂM TRA VÍ USER - Nếu có số dư, cho phép dùng ví
                var userWallet = await _context.Wallets
                    .FirstOrDefaultAsync(w => w.LandlordId == userId);

                bool useWallet = request.UseWallet && userWallet != null && userWallet.AvailableBalance >= amount;

                if (useWallet)
                {
                    // Dùng ví - không cần PayOS
                    Console.WriteLine($"Using wallet balance: {userWallet!.AvailableBalance}");

                    // Trừ tiền từ ví user
                    userWallet.AvailableBalance -= amount;
                    userWallet.UpdatedAt = DateTime.Now;

                    // Tạo transaction log cho user
                    var userTxn = new WalletTransaction
                    {
                        WalletId = userWallet.WalletId,
                        Direction = "out",
                        Amount = amount,
                        RelatedType = "booking_payment",
                        RelatedId = 0, // Sẽ update sau khi có bookingId
                        Status = "completed",
                        Note = $"Thanh toán booking - {property.Title}",
                        CreatedAt = DateTime.Now
                    };
                    _context.WalletTransactions.Add(userTxn);
                }

                // Create booking
                var booking = new Booking
                {
                    PropertyId = request.PropertyId,
                    RenterId = userId,
                    BookingDate = DateOnly.FromDateTime(request.BookingDate),
                    BookingTime = TimeOnly.FromDateTime(request.BookingDate),
                    Status = useWallet ? "Confirmed" : "Pending", // Nếu dùng ví thì confirmed luôn
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

                if (useWallet)
                {
                    // Thanh toán bằng ví - Không cần PayOS
                    // Khóa tiền vào ví landlord ngay
                    var landlordWallet = await _walletService.GetOrCreateWalletAsync(property.LandlordId);
                    await _walletService.LockBalanceAsync(
                        landlordWallet.WalletId,
                        amount,
                        "booking",
                        booking.BookingId,
                        $"Đặt cọc booking #{booking.BookingId} - {property.Title}"
                    );

                    // Tạo payment record
                    var payment = new Models.Payment
                    {
                        PayerUserId = userId,
                        LandlordId = property.LandlordId,
                        PaymentType = "Deposit",
                        Amount = amount,
                        Provider = "Wallet",
                        ProviderOrderCode = $"WALLET_{booking.BookingId}",
                        Status = "Completed",
                        CreatedAt = DateTime.Now,
                        PaidAt = DateTime.Now
                    };
                    _context.Payments.Add(payment);
                    await _context.SaveChangesAsync();

                    // Gửi notification
                    await SendBookingNotifications(booking, property, user);

                    return Ok(new
                    {
                        success = true,
                        message = "Đặt phòng thành công bằng ví",
                        bookingId = booking.BookingId,
                        paymentMethod = "wallet",
                        redirectUrl = $"/Room/Detail?id={property.PropertyId}&bookingSuccess=true"
                    });
                }

                // Thanh toán bằng PayOS
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
                    $"{baseUrl}/api/Payment/payment-cancel?bookingId={booking.BookingId}",
                    $"{baseUrl}/api/Payment/payment-success?bookingId={booking.BookingId}"
                );

                CreatePaymentResult createPayment = await _payOS.createPaymentLink(paymentData);

                Console.WriteLine($"PayOS payment link created: {createPayment.checkoutUrl}");

                // Save payment record
                var payosPayment = new Models.Payment
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

                Console.WriteLine($"Creating payment record: PayerUserId={payosPayment.PayerUserId}, LandlordId={payosPayment.LandlordId}, Amount={payosPayment.Amount}");

                _context.Payments.Add(payosPayment);
                
                try
                {
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"Payment saved successfully: PaymentId={payosPayment.PaymentId}");
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
                    bookingId = booking.BookingId,
                    paymentId = payosPayment.PaymentId
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

        private async Task SendBookingNotifications(Booking booking, Property property, User user)
        {
            // Send notification to RENTER
            if (!string.IsNullOrEmpty(user.Email))
            {
                try
                {
                    await _emailService.SendEmailAsync(
                        user.Email,
                        "Xác nhận đặt phòng thành công",
                        $"Chào {user.FullName},<br><br>" +
                        $"Bạn đã đặt cọc thành công cho phòng: {property.Title}<br>" +
                        $"Số tiền: {property.Deposit:N0} VNĐ<br>" +
                        $"Chủ nhà sẽ xem xét và phản hồi sớm nhất.<br><br>" +
                        $"Trân trọng,<br>NestFlow Team"
                    );

                    if (_notificationService != null)
                    {
                        await _notificationService.CreateAndSendNotificationAsync(
                            booking.RenterId,
                            "Đặt phòng thành công",
                            $"Bạn đã đặt cọc thành công cho phòng: {property.Title}. Chờ chủ nhà xác nhận.",
                            "success",
                            $"/Room/Detail?id={booking.PropertyId}"
                        );
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending email to renter: {ex.Message}");
                }
            }

            // Send notification to LANDLORD
            var landlord = await _context.Users.FindAsync(property.LandlordId);
            if (landlord != null && !string.IsNullOrEmpty(landlord.Email))
            {
                try
                {
                    await _emailService.SendEmailAsync(
                        landlord.Email,
                        "Có đơn đặt cọc mới",
                        $"Chào {landlord.FullName},<br><br>" +
                        $"Bạn có đơn đặt cọc mới cho phòng: {property.Title}<br>" +
                        $"Người đặt: {user.FullName}<br>" +
                        $"Số điện thoại: {user.Phone}<br>" +
                        $"Ngày xem phòng: {booking.BookingDate:dd/MM/yyyy} lúc {booking.BookingTime}<br>" +
                        $"Số tiền cọc: {property.Deposit:N0} VNĐ<br><br>" +
                        $"Vui lòng vào trang quản lý để chấp nhận hoặc từ chối.<br><br>" +
                        $"Trân trọng,<br>NestFlow Team"
                    );

                    if (_notificationService != null)
                    {
                        await _notificationService.CreateAndSendNotificationAsync(
                            property.LandlordId,
                            "Có đơn đặt cọc mới",
                            $"Bạn có đơn đặt cọc mới cho phòng: {property.Title}. Số tiền: {property.Deposit:N0} VNĐ",
                            "info",
                            $"/Landlord/Bookings"
                        );
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending email to landlord: {ex.Message}");
                }
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

                // KHÓA TIỀN VÀO VÍ LANDLORD (chờ approve)
                try
                {
                    var wallet = await _walletService.GetOrCreateWalletAsync(booking.Property.LandlordId);
                    await _walletService.LockBalanceAsync(
                        wallet.WalletId,
                        booking.Property.Deposit ?? 0,
                        "booking",
                        booking.BookingId,
                        $"Đặt cọc booking #{booking.BookingId} - {booking.Property.Title}"
                    );
                }
                catch (Exception walletEx)
                {
                    Console.WriteLine($"Error locking balance: {walletEx.Message}");
                }

                // Send notification to RENTER
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
                            $"Chủ nhà sẽ xem xét và phản hồi sớm nhất.<br><br>" +
                            $"Trân trọng,<br>NestFlow Team"
                        );

                        if (_notificationService != null)
                        {
                            await _notificationService.CreateAndSendNotificationAsync(
                                booking.RenterId,
                                "Đặt phòng thành công",
                                $"Bạn đã đặt cọc thành công cho phòng: {booking.Property.Title}. Chờ chủ nhà xác nhận.",
                                "success",
                                $"/Room/Detail?id={booking.PropertyId}"
                            );
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error sending email to renter: {ex.Message}");
                    }
                }

                // Send notification to LANDLORD
                var landlord = await _context.Users.FindAsync(booking.Property.LandlordId);
                if (landlord != null && !string.IsNullOrEmpty(landlord.Email))
                {
                    try
                    {
                        await _emailService.SendEmailAsync(
                            landlord.Email,
                            "Có đơn đặt cọc mới",
                            $"Chào {landlord.FullName},<br><br>" +
                            $"Bạn có đơn đặt cọc mới cho phòng: {booking.Property.Title}<br>" +
                            $"Người đặt: {booking.Renter.FullName}<br>" +
                            $"Số điện thoại: {booking.Renter.Phone}<br>" +
                            $"Ngày xem phòng: {booking.BookingDate:dd/MM/yyyy} lúc {booking.BookingTime}<br>" +
                            $"Số tiền cọc: {booking.Property.Deposit:N0} VNĐ<br><br>" +
                            $"Vui lòng vào trang quản lý để chấp nhận hoặc từ chối.<br><br>" +
                            $"Trân trọng,<br>NestFlow Team"
                        );

                        if (_notificationService != null)
                        {
                            await _notificationService.CreateAndSendNotificationAsync(
                                booking.Property.LandlordId,
                                "Có đơn đặt cọc mới",
                                $"Bạn có đơn đặt cọc mới cho phòng: {booking.Property.Title}. Số tiền: {booking.Property.Deposit:N0} VNĐ",
                                "info",
                                $"/Landlord/Dashboard"
                            );
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error sending email to landlord: {ex.Message}");
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
        
        // Sử dụng ví
        public bool UseWallet { get; set; }
    }
}
