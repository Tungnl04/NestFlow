using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NestFlow.Models;

namespace NestFlow.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommissionController : ControllerBase
    {
        private readonly NestFlowSystemContext _context;
        private readonly ILogger<CommissionController> _logger;

        public CommissionController(NestFlowSystemContext context, ILogger<CommissionController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Update commission settings cho property
        /// </summary>
        [HttpPost("update")]
        public async Task<IActionResult> UpdateCommission([FromBody] UpdateCommissionRequest request)
        {
            try
            {
                var landlordId = HttpContext.Session.GetInt32("UserId");
                if (landlordId == null)
                {
                    return Unauthorized(new { success = false, message = "Vui lòng đăng nhập" });
                }

                var property = await _context.Properties
                    .FirstOrDefaultAsync(p => p.PropertyId == request.PropertyId && p.LandlordId == landlordId);

                if (property == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy property" });
                }

                // Validate
                if (request.CommissionRate < 0 || request.CommissionRate > 100)
                {
                    return BadRequest(new { success = false, message = "Commission rate phải từ 0-100%" });
                }

                if (request.UserDiscount < 0)
                {
                    return BadRequest(new { success = false, message = "User discount không được âm" });
                }

                // Check discount không vượt quá commission
                var deposit = property.Deposit ?? 0;
                var commissionAmount = deposit * (request.CommissionRate / 100);
                
                if (request.UserDiscount > commissionAmount)
                {
                    return BadRequest(new { 
                        success = false, 
                        message = $"User discount ({request.UserDiscount:N0}) không được vượt quá commission amount ({commissionAmount:N0})" 
                    });
                }

                // Update
                property.CommissionRate = request.CommissionRate;
                property.UserDiscount = request.UserDiscount;
                property.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Updated commission for property {property.PropertyId}: Rate={request.CommissionRate}%, Discount={request.UserDiscount}");

                return Ok(new
                {
                    success = true,
                    message = "Đã cập nhật hoa hồng",
                    data = new
                    {
                        propertyId = property.PropertyId,
                        commissionRate = property.CommissionRate,
                        userDiscount = property.UserDiscount,
                        landlordReceives = deposit - commissionAmount
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating commission: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra" });
            }
        }

        /// <summary>
        /// Get commission report
        /// </summary>
        [HttpGet("report")]
        public async Task<IActionResult> GetCommissionReport([FromQuery] int? year, [FromQuery] int? month)
        {
            try
            {
                var landlordId = HttpContext.Session.GetInt32("UserId");
                if (landlordId == null)
                {
                    return Unauthorized(new { success = false, message = "Vui lòng đăng nhập" });
                }

                var query = _context.Payments
                    .Where(p => p.LandlordId == landlordId && 
                                p.PaymentType == "Deposit" && 
                                p.Status == "Completed");

                if (year.HasValue)
                {
                    query = query.Where(p => p.CreatedAt.Year == year.Value);
                }

                if (month.HasValue)
                {
                    query = query.Where(p => p.CreatedAt.Month == month.Value);
                }

                var payments = await query.ToListAsync();

                var report = new
                {
                    totalBookings = payments.Count,
                    totalUserPaid = payments.Sum(p => p.Amount),
                    totalCommission = payments.Sum(p => p.PlatformCommission ?? 0),
                    totalDiscount = payments.Sum(p => p.UserDiscountApplied ?? 0),
                    totalLandlordReceived = payments.Sum(p => p.LandlordAmount ?? 0),
                    totalPlatformKeep = payments.Sum(p => (p.PlatformCommission ?? 0) - (p.UserDiscountApplied ?? 0))
                };

                return Ok(new { success = true, data = report });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting commission report: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra" });
            }
        }
    }

    public class UpdateCommissionRequest
    {
        public long PropertyId { get; set; }
        public decimal CommissionRate { get; set; }
        public decimal UserDiscount { get; set; }
    }
}
