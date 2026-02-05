using Microsoft.AspNetCore.Mvc;
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

        public ListingsController(IListingService listingService, IPropertyService propertyService)
        {
            _listingService = listingService;
            _propertyService = propertyService;
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
    }
}
