using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NestFlow.Application.Services.Interfaces;
using NestFlow.Models;

namespace NestFlow.Pages.Landlord.Posts
{
    public class EditModel : PageModel
    {
        private readonly IListingService _listingService;
        private readonly IPropertyService _propertyService;
        private readonly IWebHostEnvironment _env;

        public EditModel(IListingService listingService, IPropertyService propertyService, IWebHostEnvironment env)
        {
            _listingService = listingService;
            _propertyService = propertyService;
            _env = env;
        }

        [BindProperty]
        public Listing EditingListing { get; set; } = null!;

        [BindProperty]
        public Property EditingProperty { get; set; } = null!;

        [BindProperty]
        public List<long> SelectedAmenityIds { get; set; } = new List<long>();

        public List<Amenity> AvailableAmenities { get; set; } = new List<Amenity>();

        [BindProperty]
        public List<IFormFile> UploadedImages { get; set; } = new List<IFormFile>();

        [BindProperty]
        public List<IFormFile> Uploaded360Images { get; set; } = new List<IFormFile>();

        public List<PropertyImage> ExistingImages { get; set; } = new List<PropertyImage>();

        public async Task<IActionResult> OnGetAsync(long id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userType = HttpContext.Session.GetString("UserType");
            if (userId == null || userType != "landlord") return RedirectToPage("/Home/Index");

            var listing = await _listingService.GetListingByIdAsync(id);
            if (listing == null) return NotFound();

            var property = await _propertyService.GetPropertyByIdAsync(listing.PropertyId);
            if (property == null || property.LandlordId != userId.Value) return Forbid();

            EditingListing = listing;
            EditingProperty = property;
            ExistingImages = property.PropertyImages.ToList();
            SelectedAmenityIds = property.Amenities.Select(a => a.AmenityId).ToList();
            AvailableAmenities = await _propertyService.GetAllAmenitiesAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToPage("/Home/Index");

            // Verify ownership
            var currentListing = await _listingService.GetListingByIdAsync(EditingListing.ListingId);
            if (currentListing == null || currentListing.Property.LandlordId != userId.Value) return Forbid();

            AvailableAmenities = await _propertyService.GetAllAmenitiesAsync();

            // ModelState basic cleanup
            ModelState.Remove("EditingListing.Property");
            ModelState.Remove("EditingListing.Landlord");
            ModelState.Remove("EditingProperty.Landlord");
            ModelState.Remove("EditingProperty.Title");
            ModelState.Remove("EditingProperty.PropertyType");

            if (!ModelState.IsValid)
            {
                // Re-load images for view
                var prop = await _propertyService.GetPropertyByIdAsync(EditingProperty.PropertyId);
                ExistingImages = prop?.PropertyImages.ToList() ?? new List<PropertyImage>();
                return Page();
            }

            try
            {
                // Sync property info
                EditingProperty.Title = EditingListing.Title;

                var imageEntities = new List<PropertyImage>();
                // Handle new images if any
                if ((UploadedImages != null && UploadedImages.Count > 0) || (Uploaded360Images != null && Uploaded360Images.Count > 0))
                {
                    // For editing, we might want to keep images in the same existing folder if we can find it
                    // but for simplicity of this "All-in-one" refactor, we just use a subfolder for these new uploads 
                    // or keep it simple. Let's find existing path from first image if exists
                    string propertyFolder = Guid.NewGuid().ToString();
                    var existingImg = (await _propertyService.GetPropertyByIdAsync(EditingProperty.PropertyId))?.PropertyImages.FirstOrDefault();
                    if (existingImg != null)
                    {
                        var parts = existingImg.ImageUrl.Split('/');
                        if (parts.Length >= 4) propertyFolder = parts[3];
                    }

                    var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "properties", propertyFolder);
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    // Re-calculate display order
                    var prop = await _propertyService.GetPropertyByIdAsync(EditingProperty.PropertyId);
                    int displayOrder = prop?.PropertyImages.Count > 0 ? (prop.PropertyImages.Max(i => i.DisplayOrder) ?? 0) + 1 : 0;

                    if (UploadedImages != null)
                    {
                        foreach (var file in UploadedImages)
                        {
                            var extension = Path.GetExtension(file.FileName);
                            var newFileName = $"{Guid.NewGuid()}{extension}";
                            await SaveFile(file, Path.Combine(uploadsFolder, newFileName));
                            imageEntities.Add(new PropertyImage { 
                                ImageUrl = $"/uploads/properties/{propertyFolder}/{newFileName}", 
                                DisplayOrder = displayOrder++ 
                            });
                        }
                    }

                    if (Uploaded360Images != null)
                    {
                        foreach (var file in Uploaded360Images)
                        {
                            var extension = Path.GetExtension(file.FileName);
                            var newFileName = $"{Guid.NewGuid()}_360{extension}";
                            await SaveFile(file, Path.Combine(uploadsFolder, newFileName));
                            imageEntities.Add(new PropertyImage { 
                                ImageUrl = $"/uploads/properties/{propertyFolder}/{newFileName}", 
                                DisplayOrder = displayOrder++ 
                            });
                        }
                    }
                }

                await _listingService.UpdateListingWithPropertyAsync(EditingListing, EditingProperty, imageEntities, SelectedAmenityIds);

                TempData["Success"] = "Cập nhật bài đăng thành công!";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Lỗi: " + ex.Message);
                return Page();
            }
        }

        private async Task SaveFile(IFormFile file, string path)
        {
            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
        }

        public async Task<IActionResult> OnPostDeleteImageAsync(long imageId, long listingId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            // Simple delete check
            await _propertyService.DeletePropertyImageAsync(imageId);
            return RedirectToPage(new { id = listingId });
        }
    }
}
