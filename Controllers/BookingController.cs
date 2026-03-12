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
