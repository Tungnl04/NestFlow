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
        private readonly IWebHostEnvironment _env;

        public PropertiesController(IPropertyService propertyService, IWebHostEnvironment env)
        {
            _propertyService = propertyService;
            _env = env;
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

        [HttpPost("{id}/images")]
        public async Task<IActionResult> UploadImages(long id, [FromForm] IFormFileCollection files, [FromForm] bool is360 = false)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var property = await _propertyService.GetPropertyByIdAsync(id);
            if (property == null) return NotFound("Property not found");
            if (property.LandlordId != userId.Value) return Forbid();

            if (files == null || files.Count == 0) return BadRequest("No files uploaded");

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "properties", id.ToString());
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uploadedImages = new List<PropertyImage>();

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    // If marked as 360, append _360 to filename so client script recognizes it
                    var extension = Path.GetExtension(file.FileName);
                    var originalName = Path.GetFileNameWithoutExtension(file.FileName);
                    var newFileName = is360 ? $"{Guid.NewGuid()}_360{extension}" : $"{Guid.NewGuid()}{extension}";
                    
                    var filePath = Path.Combine(uploadsFolder, newFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Store relative URL in DB
                    var imageUrl = $"/uploads/properties/{id}/{newFileName}";
                    
                    // Add to DB
                    var isPrimary = !property.PropertyImages.Any() && uploadedImages.Count == 0;
                    var savedImg = await _propertyService.AddPropertyImageAsync(id, imageUrl, isPrimary);
                    uploadedImages.Add(savedImg);
                }
            }

            // Return a simple DTO to avoid JSON object cycle with Property -> PropertyImages
            var resultDto = uploadedImages.Select(img => new { 
                imageId = img.ImageId, 
                imageUrl = img.ImageUrl, 
                isPrimary = img.IsPrimary 
            });

            return Ok(resultDto);
        }

        [HttpDelete("images/{imageId}")]
        public async Task<IActionResult> DeleteImage(long imageId)
        {
            // Note: Should verify ownership securely. 
            // For now, rely on UI only exposing this to owner or simple check
            // A more robust check would query the image -> property -> landlordId
            
            var success = await _propertyService.DeletePropertyImageAsync(imageId);
            if (!success) return NotFound();

            // Ideally, delete physical file here too based on URL

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
