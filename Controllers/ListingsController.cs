using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NestFlow.Application.Services.Interfaces;
using NestFlow.Models;
using System.Security.Claims;

namespace NestFlow.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ListingsController : ControllerBase
    {
        private readonly IListingService _listingService;
        private readonly IPropertyService _propertyService;
        private readonly NestFlowSystemContext _context;
        private readonly INotificationService _notificationService;

        public ListingsController(
            IListingService listingService, 
            IPropertyService propertyService,
            NestFlowSystemContext context,
            INotificationService notificationService)
        {
            _listingService = listingService;
            _propertyService = propertyService;
            _context = context;
            _notificationService = notificationService;
        }

        [HttpGet("my-listings")]
        public async Task<IActionResult> GetMyListings()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var listings = await _listingService.GetListingsByLandlordIdAsync(userId.Value);
            return Ok(listings);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetListing(long id)
        {
            var listing = await _listingService.GetListingByIdAsync(id);
            if (listing == null) return NotFound();
            return Ok(listing);
        }

        [HttpPost]
        public async Task<IActionResult> CreateListing([FromBody] Listing listing)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            // Verify property ownership
            var property = await _propertyService.GetPropertyByIdAsync(listing.PropertyId);
            if (property == null) return BadRequest("Property does not exist.");
            if (property.LandlordId != userId.Value) return Forbid();

            var created = await _listingService.CreateListingAsync(listing);
            return CreatedAtAction(nameof(GetListing), new { id = created.ListingId }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateListing(long id, [FromBody] Listing listing)
        {
            if (id != listing.ListingId) return BadRequest();

            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var existing = await _listingService.GetListingByIdAsync(id);
            if (existing == null) return NotFound();
            if (existing.Property.LandlordId != userId.Value) return Forbid();

            await _listingService.UpdateListingAsync(listing);
            return NoContent();
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ToggleStatus(long id, [FromBody] string status)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var existing = await _listingService.GetListingByIdAsync(id);
            if (existing == null) return NotFound();
            if (existing.Property.LandlordId != userId.Value) return Forbid();

            await _listingService.ToggleListingStatusAsync(id, status);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteListing(long id)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var existing = await _listingService.GetListingByIdAsync(id);
            if (existing == null) return NotFound();
            if (existing.Property.LandlordId != userId.Value) return Forbid();

            await _listingService.DeleteListingAsync(id);
            return NoContent();
        }

        private long? GetCurrentUserId()
        {
            var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (long.TryParse(claimId, out var id)) return id;

            var sessionId = HttpContext.Session.GetInt32("UserId");
            if (sessionId.HasValue) return sessionId.Value;

            return null;
        }

        // --- ADMIN ENDPOINTS ---

        [HttpGet("admin/all")]
        public async Task<IActionResult> GetAllAdminListings([FromQuery] string? status)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var user = await _context.Users.FindAsync(userId.Value);
            if (user?.UserType?.ToLower() != "admin") return Forbid();

            var query = _context.Listings
                .Include(l => l.Property)
                    .ThenInclude(p => p.Landlord)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(l => l.Status == status);
            }

            var listings = await query
                .OrderByDescending(l => l.ListingId)
                .Select(l => new {
                    l.ListingId,
                    l.PropertyId,
                    Title = l.Property.Title,
                    LandlordName = l.Property.Landlord.FullName ?? "N/A",
                    LandlordEmail = l.Property.Landlord.Email,
                    Price = l.Property.Price,
                    l.Status
                })
                .ToListAsync();

            return Ok(new { success = true, data = listings });
        }

        [HttpPost("admin/hide/{id}")]
        public async Task<IActionResult> AdminHideListing(long id, [FromBody] ProcessPostRequest req)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var user = await _context.Users.FindAsync(userId.Value);
            if (user?.UserType?.ToLower() != "admin") return Forbid();

            var listing = await _context.Listings.Include(l => l.Property).FirstOrDefaultAsync(l => l.ListingId == id);
            if (listing == null) return NotFound(new { success = false, message = "Không tìm thấy tin đăng" });

            listing.Status = "inactive";
            await _context.SaveChangesAsync();

            await _notificationService.CreateAndSendNotificationAsync(
                listing.Property.LandlordId,
                "Tin đăng đã bị ẩn",
                $"Tin đăng \"{listing.Property.Title}\" của bạn đã bị ẩn bởi Quản trị viên. Lý do: {req.Note}",
                "warning",
                "/Landlord/Listings"
            );

            return Ok(new { success = true, message = "Đã ẩn tin đăng" });
        }

        [HttpPost("admin/delete/{id}")]
        public async Task<IActionResult> AdminDeleteListing(long id, [FromBody] ProcessPostRequest req)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var user = await _context.Users.FindAsync(userId.Value);
            if (user?.UserType?.ToLower() != "admin") return Forbid();

            var listing = await _context.Listings.Include(l => l.Property).FirstOrDefaultAsync(l => l.ListingId == id);
            if (listing == null) return NotFound(new { success = false, message = "Không tìm thấy tin đăng" });

            _context.Listings.Remove(listing);
            await _context.SaveChangesAsync();

            await _notificationService.CreateAndSendNotificationAsync(
                listing.Property.LandlordId,
                "Tin đăng đã bị xóa",
                $"Tin đăng \"{listing.Property.Title}\" của bạn đã bị xóa bởi Quản trị viên. Lý do: {req.Note}",
                "error",
                "/Landlord/Listings"
            );

            return Ok(new { success = true, message = "Đã xóa tin đăng" });
        }
    }

    public class ProcessPostRequest
    {
        public string Note { get; set; } = string.Empty;
    }
}
