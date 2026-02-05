using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NestFlow.Application.Services.Interfaces;
using NestFlow.Models;
using System.Security.Claims;

namespace NestFlow.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PropertiesController : ControllerBase
    {
        private readonly IPropertyService _propertyService;

        public PropertiesController(IPropertyService propertyService)
        {
            _propertyService = propertyService;
        }

        // GET: api/Properties/my-properties
        [HttpGet("my-properties")]
        public async Task<IActionResult> GetMyProperties()
        {
            // Try get userId from Session or Claims
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var properties = await _propertyService.GetPropertiesByLandlordIdAsync(userId.Value);
            return Ok(properties);
        }

        // GET: api/Properties/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProperty(long id)
        {
            var property = await _propertyService.GetPropertyByIdAsync(id);
            if (property == null) return NotFound();
            return Ok(property);
        }

        // POST: api/Properties
        [HttpPost]
        public async Task<IActionResult> CreateProperty([FromBody] Property property)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            // Force landlord ID
            property.LandlordId = userId.Value;

            var created = await _propertyService.CreatePropertyAsync(property);
            return CreatedAtAction(nameof(GetProperty), new { id = created.PropertyId }, created);
        }

        // PUT: api/Properties/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProperty(long id, [FromBody] Property property)
        {
            if (id != property.PropertyId) return BadRequest();
            
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            // Verify ownership
            var existing = await _propertyService.GetPropertyByIdAsync(id);
            if (existing == null) return NotFound();
            if (existing.LandlordId != userId.Value) return Forbid();

            await _propertyService.UpdatePropertyAsync(property);
            return NoContent();
        }

        // DELETE: api/Properties/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProperty(long id)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var existing = await _propertyService.GetPropertyByIdAsync(id);
            if (existing == null) return NotFound();
            if (existing.LandlordId != userId.Value) return Forbid();

            var success = await _propertyService.DeletePropertyAsync(id);
            if (!success) return BadRequest("Cannot delete property (it may have bookings or listings).");

            return NoContent();
        }

        private long? GetCurrentUserId()
        {
            // 1. Check Claims (if Cookie Auth)
            var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (long.TryParse(claimId, out var id)) return id;

            // 2. Check Session
            var sessionId = HttpContext.Session.GetInt32("UserId");
            if (sessionId.HasValue) return sessionId.Value;

            return null;
        }
    }
}
