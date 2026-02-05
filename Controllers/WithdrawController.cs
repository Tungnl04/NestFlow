using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NestFlow.Application.Services.Interfaces;
using NestFlow.Models;

namespace NestFlow.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WithdrawController : ControllerBase
    {
        private readonly NestFlowSystemContext _context;
        private readonly IWalletService _walletService;
        private readonly IEmailService _emailService;
        private readonly INotificationService? _notificationService;
        private readonly ILogger<WithdrawController> _logger;

        public WithdrawController(
            NestFlowSystemContext context,
            IWalletService walletService,
            IEmailService emailService,
            ILogger<WithdrawController> logger,
            INotificationService? notificationService = null)
        {
            _context = context;
            _walletService = walletService;
            _emailService = emailService;
            _logger = logger;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Landlord tạo yêu cầu rút tiền
        /// </summary>
        [HttpPost("create")]
        public async Task<IActionResult> CreateWithdrawRequest([FromBody] CreateWithdrawRequest request)
        {
            try
            {
                _logger.LogInformation("=== START CreateWithdrawRequest ===");
                
                var landlordId = HttpContext.Session.GetInt32("UserId");
                _logger.LogInformation($"LandlordId from session: {landlordId}");
                
                if (landlordId == null)
                {
                    return Unauthorized(new { success = false, message = "Vui lòng đăng nhập" });
                }

                // Validate input
                if (request.Amount <= 0)
                {
                    return BadRequest(new { success = false, message = "Số tiền không hợp lệ" });
                }

                if (string.IsNullOrEmpty(request.BankName) || 
                    string.IsNullOrEmpty(request.BankAccount) || 
                    string.IsNullOrEmpty(request.AccountHolder))
                {
                    return BadRequest(new { success = false, message = "Vui lòng điền đầy đủ thông tin ngân hàng" });
                }

                _logger.LogInformation($"Request validated. Amount: {request.Amount}, Bank: {request.BankName}");

                // Lấy ví
                var wallet = await _walletService.GetOrCreateWalletAsync(landlordId.Value);
                _logger.LogInformation($"Wallet found. WalletId: {wallet.WalletId}, Available: {wallet.AvailableBalance}");

                // Kiểm tra số dư
                if (wallet.AvailableBalance < request.Amount)
                {
                    return BadRequest(new { 
                        success = false, 
                        message = $"Số dư không đủ. Số dư khả dụng: {wallet.AvailableBalance:N0} VNĐ" 
                    });
                }

                // Tạo withdraw request
                var withdrawRequest = new WithdrawRequest
                {
                    WalletId = wallet.WalletId,
                    LandlordId = landlordId.Value,
                    Amount = request.Amount,
                    BankName = request.BankName,
                    BankAccount = request.BankAccount,
                    AccountHolder = request.AccountHolder,
                    Status = "Pending",
                    RequestedAt = DateTime.Now,
                    Note = request.Note ?? ""
                };

                _logger.LogInformation("Adding withdraw request to context...");
                _context.WithdrawRequests.Add(withdrawRequest);

                // Trừ available balance (chuyển sang pending withdrawal)
                wallet.AvailableBalance -= request.Amount;
                wallet.UpdatedAt = DateTime.Now;
                _logger.LogInformation($"Updated wallet. New available: {wallet.AvailableBalance}");

                // Tạo transaction log (tạm thời RelatedId = 0)
                var transaction = new WalletTransaction
                {
                    WalletId = wallet.WalletId,
                    Direction = "out",
                    Amount = request.Amount,
                    RelatedType = "withdraw_request",
                    RelatedId = 0, // Sẽ update sau
                    Status = "pending",
                    Note = $"Yêu cầu rút tiền - {request.BankName} {request.BankAccount}",
                    CreatedAt = DateTime.Now
                };

                _logger.LogInformation("Adding transaction to context...");
                _context.WalletTransactions.Add(transaction);
                
                _logger.LogInformation("Saving changes to database...");
                await _context.SaveChangesAsync();
                
                // Update RelatedId sau khi có WithdrawId
                _logger.LogInformation($"Updating transaction RelatedId to {withdrawRequest.WithdrawId}...");
                transaction.RelatedId = withdrawRequest.WithdrawId;
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Withdraw request created successfully. WithdrawId: {withdrawRequest.WithdrawId}");

                // Gửi email cho landlord (không throw nếu lỗi)
                try
                {
                    var landlord = await _context.Users.FindAsync(landlordId.Value);
                    if (landlord != null && !string.IsNullOrEmpty(landlord.Email))
                    {
                        _logger.LogInformation($"Sending email to {landlord.Email}...");
                        await _emailService.SendEmailAsync(
                            landlord.Email,
                            "Yêu cầu rút tiền đã được gửi",
                            $"Chào {landlord.FullName},<br><br>" +
                            $"Yêu cầu rút tiền của bạn đã được gửi thành công:<br>" +
                            $"Số tiền: {request.Amount:N0} VNĐ<br>" +
                            $"Ngân hàng: {request.BankName}<br>" +
                            $"Số tài khoản: {request.BankAccount}<br>" +
                            $"Chủ tài khoản: {request.AccountHolder}<br><br>" +
                            $"Admin sẽ xử lý và chuyển tiền trong vòng 1-3 ngày làm việc.<br><br>" +
                            $"Trân trọng,<br>NestFlow Team"
                        );
                        _logger.LogInformation("Email sent successfully");

                        if (_notificationService != null)
                        {
                            _logger.LogInformation("Sending notification...");
                            await _notificationService.CreateAndSendNotificationAsync(
                                landlordId.Value,
                                "Yêu cầu rút tiền đã được gửi",
                                $"Yêu cầu rút {request.Amount:N0} VNĐ đang được xử lý",
                                "info",
                                "/Landlord/Wallet"
                            );
                            _logger.LogInformation("Notification sent successfully");
                        }
                    }
                }
                catch (Exception emailEx)
                {
                    _logger.LogError($"Error sending email/notification: {emailEx.Message}");
                    // KHÔNG throw - vì withdraw request đã tạo thành công
                }

                _logger.LogInformation("=== END CreateWithdrawRequest SUCCESS ===");
                
                return Ok(new
                {
                    success = true,
                    message = "Yêu cầu rút tiền đã được gửi",
                    withdrawId = withdrawRequest.WithdrawId,
                    amount = request.Amount,
                    remainingBalance = wallet.AvailableBalance
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"=== ERROR CreateWithdrawRequest ===");
                _logger.LogError($"Exception: {ex.GetType().Name}");
                _logger.LogError($"Message: {ex.Message}");
                _logger.LogError($"StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    _logger.LogError($"InnerException: {ex.InnerException.Message}");
                }
                
                // Rollback nếu có lỗi
                try
                {
                    // Nếu lỗi xảy ra sau SaveChanges, cần rollback thủ công
                    // Nhưng EF Core đã tự động rollback transaction nếu có lỗi
                }
                catch { }
                
                return StatusCode(500, new { 
                    success = false, 
                    message = "Có lỗi xảy ra khi tạo yêu cầu rút tiền",
                    error = ex.Message,
                    details = ex.InnerException?.Message
                });
            }
        }

        /// <summary>
        /// Admin chấp nhận yêu cầu rút tiền (sau khi đã chuyển tiền thủ công)
        /// </summary>
        [HttpPost("approve/{withdrawId}")]
        public async Task<IActionResult> ApproveWithdraw(long withdrawId, [FromBody] ProcessWithdrawRequest request)
        {
            try
            {
                var adminId = HttpContext.Session.GetInt32("UserId");
                if (adminId == null)
                {
                    return Unauthorized(new { success = false, message = "Vui lòng đăng nhập" });
                }

                // TODO: Kiểm tra quyền admin
                var admin = await _context.Users.FindAsync((long)adminId.Value);
                if (admin == null || admin.UserType != "Admin")
                {
                    return Forbid();
                }

                var withdrawRequest = await _context.WithdrawRequests
                    .Include(w => w.Wallet)
                    .Include(w => w.Landlord)
                    .FirstOrDefaultAsync(w => w.WithdrawId == withdrawId);

                if (withdrawRequest == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy yêu cầu rút tiền" });
                }

                if (withdrawRequest.Status != "Pending")
                {
                    return BadRequest(new { success = false, message = "Yêu cầu đã được xử lý" });
                }

                // Cập nhật withdraw request
                withdrawRequest.Status = "Approved";
                withdrawRequest.ProcessedAt = DateTime.Now;
                withdrawRequest.ProcessedByAdminId = adminId.Value;
                withdrawRequest.Note = $"{withdrawRequest.Note}\n[Admin: {request.Note}]";

                // Cập nhật transaction
                var transaction = await _context.WalletTransactions
                    .Where(t => t.RelatedType == "withdraw_request" && t.RelatedId == withdrawId)
                    .FirstOrDefaultAsync();

                if (transaction != null)
                {
                    transaction.Status = "completed";
                }

                await _context.SaveChangesAsync();

                // Gửi email cho landlord
                if (!string.IsNullOrEmpty(withdrawRequest.Landlord.Email))
                {
                    try
                    {
                        await _emailService.SendEmailAsync(
                            withdrawRequest.Landlord.Email,
                            "Yêu cầu rút tiền đã được chấp nhận",
                            $"Chào {withdrawRequest.Landlord.FullName},<br><br>" +
                            $"Yêu cầu rút tiền của bạn đã được chấp nhận:<br>" +
                            $"Số tiền: {withdrawRequest.Amount:N0} VNĐ<br>" +
                            $"Ngân hàng: {withdrawRequest.BankName}<br>" +
                            $"Số tài khoản: {withdrawRequest.BankAccount}<br><br>" +
                            $"Tiền đã được chuyển vào tài khoản của bạn.<br>" +
                            $"Vui lòng kiểm tra.<br><br>" +
                            $"Trân trọng,<br>NestFlow Team"
                        );

                        if (_notificationService != null)
                        {
                            await _notificationService.CreateAndSendNotificationAsync(
                                withdrawRequest.LandlordId,
                                "Rút tiền thành công",
                                $"Yêu cầu rút {withdrawRequest.Amount:N0} VNĐ đã được chấp nhận. Vui lòng kiểm tra tài khoản.",
                                "success",
                                "/Landlord/Wallet"
                            );
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error sending email: {ex.Message}");
                    }
                }

                return Ok(new
                {
                    success = true,
                    message = "Đã chấp nhận yêu cầu rút tiền",
                    withdrawId = withdrawRequest.WithdrawId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error approving withdraw: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra" });
            }
        }

        /// <summary>
        /// Admin từ chối yêu cầu rút tiền (hoàn lại available balance)
        /// </summary>
        [HttpPost("reject/{withdrawId}")]
        public async Task<IActionResult> RejectWithdraw(long withdrawId, [FromBody] ProcessWithdrawRequest request)
        {
            try
            {
                var adminId = HttpContext.Session.GetInt32("UserId");
                if (adminId == null)
                {
                    return Unauthorized(new { success = false, message = "Vui lòng đăng nhập" });
                }

                // TODO: Kiểm tra quyền admin
                var admin = await _context.Users.FindAsync((long)adminId.Value);
                if (admin == null || admin.UserType != "Admin")
                {
                    return Forbid();
                }

                var withdrawRequest = await _context.WithdrawRequests
                    .Include(w => w.Wallet)
                    .Include(w => w.Landlord)
                    .FirstOrDefaultAsync(w => w.WithdrawId == withdrawId);

                if (withdrawRequest == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy yêu cầu rút tiền" });
                }

                if (withdrawRequest.Status != "Pending")
                {
                    return BadRequest(new { success = false, message = "Yêu cầu đã được xử lý" });
                }

                // Hoàn lại available balance
                withdrawRequest.Wallet.AvailableBalance += withdrawRequest.Amount;
                withdrawRequest.Wallet.UpdatedAt = DateTime.Now;

                // Cập nhật withdraw request
                withdrawRequest.Status = "Rejected";
                withdrawRequest.ProcessedAt = DateTime.Now;
                withdrawRequest.ProcessedByAdminId = adminId.Value;
                withdrawRequest.Note = $"{withdrawRequest.Note}\n[Admin từ chối: {request.Note}]";

                // Cập nhật transaction
                var transaction = await _context.WalletTransactions
                    .Where(t => t.RelatedType == "withdraw_request" && t.RelatedId == withdrawId)
                    .FirstOrDefaultAsync();

                if (transaction != null)
                {
                    transaction.Status = "rejected";
                }

                await _context.SaveChangesAsync();

                // Gửi email cho landlord
                if (!string.IsNullOrEmpty(withdrawRequest.Landlord.Email))
                {
                    try
                    {
                        await _emailService.SendEmailAsync(
                            withdrawRequest.Landlord.Email,
                            "Yêu cầu rút tiền bị từ chối",
                            $"Chào {withdrawRequest.Landlord.FullName},<br><br>" +
                            $"Rất tiếc, yêu cầu rút tiền của bạn bị từ chối:<br>" +
                            $"Số tiền: {withdrawRequest.Amount:N0} VNĐ<br>" +
                            $"Lý do: {request.Note}<br><br>" +
                            $"Số tiền đã được hoàn lại vào ví của bạn.<br>" +
                            $"Vui lòng liên hệ admin để biết thêm chi tiết.<br><br>" +
                            $"Trân trọng,<br>NestFlow Team"
                        );

                        if (_notificationService != null)
                        {
                            await _notificationService.CreateAndSendNotificationAsync(
                                withdrawRequest.LandlordId,
                                "Yêu cầu rút tiền bị từ chối",
                                $"Yêu cầu rút {withdrawRequest.Amount:N0} VNĐ bị từ chối. Số tiền đã được hoàn lại.",
                                "warning",
                                "/Landlord/Wallet"
                            );
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error sending email: {ex.Message}");
                    }
                }

                return Ok(new
                {
                    success = true,
                    message = "Đã từ chối yêu cầu rút tiền",
                    withdrawId = withdrawRequest.WithdrawId,
                    refundedAmount = withdrawRequest.Amount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error rejecting withdraw: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra" });
            }
        }

        /// <summary>
        /// Lấy danh sách yêu cầu rút tiền của landlord
        /// </summary>
        [HttpGet("my-requests")]
        public async Task<IActionResult> GetMyWithdrawRequests()
        {
            try
            {
                var landlordId = HttpContext.Session.GetInt32("UserId");
                if (landlordId == null)
                {
                    return Unauthorized(new { success = false, message = "Vui lòng đăng nhập" });
                }

                var requests = await _context.WithdrawRequests
                    .Where(w => w.LandlordId == landlordId.Value)
                    .OrderByDescending(w => w.RequestedAt)
                    .Select(w => new
                    {
                        w.WithdrawId,
                        w.Amount,
                        w.BankName,
                        w.BankAccount,
                        w.AccountHolder,
                        w.Status,
                        w.RequestedAt,
                        w.ProcessedAt,
                        w.Note
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = requests });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting withdraw requests: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra" });
            }
        }

        /// <summary>
        /// Admin lấy tất cả yêu cầu rút tiền
        /// </summary>
        [HttpGet("admin/all")]
        public async Task<IActionResult> GetAllWithdrawRequests([FromQuery] string? status = null)
        {
            try
            {
                var adminId = HttpContext.Session.GetInt32("UserId");
                if (adminId == null)
                {
                    return Unauthorized(new { success = false, message = "Vui lòng đăng nhập" });
                }

                // TODO: Kiểm tra quyền admin
                var admin = await _context.Users.FindAsync((long)adminId.Value);
                if (admin == null || admin.UserType != "Admin")
                {
                    return Forbid();
                }

                var query = _context.WithdrawRequests
                    .Include(w => w.Landlord)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(w => w.Status == status);
                }

                var requests = await query
                    .OrderByDescending(w => w.RequestedAt)
                    .Select(w => new
                    {
                        w.WithdrawId,
                        w.LandlordId,
                        LandlordName = w.Landlord.FullName,
                        LandlordPhone = w.Landlord.Phone,
                        LandlordEmail = w.Landlord.Email,
                        w.Amount,
                        w.BankName,
                        w.BankAccount,
                        w.AccountHolder,
                        w.Status,
                        w.RequestedAt,
                        w.ProcessedAt,
                        w.Note
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = requests });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting all withdraw requests: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra" });
            }
        }

        /// <summary>
        /// Lấy thông tin ví của landlord
        /// </summary>
        [HttpGet("wallet-info")]
        public async Task<IActionResult> GetWalletInfo()
        {
            try
            {
                var landlordId = HttpContext.Session.GetInt32("UserId");
                if (landlordId == null)
                {
                    return Unauthorized(new { success = false, message = "Vui lòng đăng nhập" });
                }

                var wallet = await _walletService.GetOrCreateWalletAsync(landlordId.Value);

                // Tính tổng pending withdrawals
                var pendingWithdrawals = await _context.WithdrawRequests
                    .Where(w => w.LandlordId == landlordId.Value && w.Status == "Pending")
                    .SumAsync(w => (decimal?)w.Amount) ?? 0;

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        wallet.WalletId,
                        wallet.AvailableBalance,
                        wallet.LockedBalance,
                        PendingWithdrawals = pendingWithdrawals,
                        TotalBalance = wallet.AvailableBalance + wallet.LockedBalance + pendingWithdrawals,
                        wallet.Currency
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting wallet info: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra" });
            }
        }
    }

    public class CreateWithdrawRequest
    {
        public decimal Amount { get; set; }
        public string BankName { get; set; } = null!;
        public string BankAccount { get; set; } = null!;
        public string AccountHolder { get; set; } = null!;
        public string? Note { get; set; }
    }

    public class ProcessWithdrawRequest
    {
        public string Note { get; set; } = "Đã xử lý";
    }
}
