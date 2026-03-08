using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NestFlow.Models;

namespace NestFlow.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RentalController : ControllerBase
    {
        private readonly NestFlowSystemContext _context;

        public RentalController(NestFlowSystemContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy danh sách hợp đồng thuê của landlord
        /// </summary>
        [HttpGet("landlord/{landlordId}")]
        public async Task<IActionResult> GetLandlordRentals(long landlordId)
        {
            try
            {
                var rentals = await _context.Rentals
                    .Include(r => r.Property)
                    .Include(r => r.Renter)
                    .Where(r => r.LandlordId == landlordId && r.Status == "active")
                    .Select(r => new
                    {
                        r.RentalId,
                        r.PropertyId,
                        PropertyTitle = r.Property.Title,
                        r.RenterId,
                        RenterName = r.Renter.FullName,
                        r.StartDate,
                        r.EndDate,
                        r.MonthlyRent,
                        r.Status
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = rentals });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting rentals: {ex.Message}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
