using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using NestFlow.Application.Services.Interfaces;
using NestFlow.Models;

namespace NestFlow.Pages.Landlord.Posts
{
    public class CreateModel : PageModel
    {
        private readonly IListingService _listingService;
        private readonly IPropertyService _propertyService;

        public CreateModel(IListingService listingService, IPropertyService propertyService)
        {
            _listingService = listingService;
            _propertyService = propertyService;
        }

        [BindProperty]
        public Listing NewListing { get; set; } = new Listing();

        public SelectList PropertyOptions { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userType = HttpContext.Session.GetString("UserType");
            if (userId == null || userType != "landlord") return RedirectToPage("/Home/Index");

            var properties = await _propertyService.GetPropertiesByLandlordIdAsync(userId.Value);
            // Filter: Should ideally filter out properties that already have an active listing.
            // For now, list all.
            PropertyOptions = new SelectList(properties, "PropertyId", "Title");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
             var userId = HttpContext.Session.GetInt32("UserId");
             if (userId == null) return RedirectToPage("/Home/Index");

             if (!ModelState.IsValid)
             {
                 // Ignore navigation properties
                 ModelState.Remove("NewListing.Property");
                 ModelState.Remove("NewListing.Landlord");
                 
                 // Re-check
                 if (!ModelState.IsValid)
                 {
                     var properties = await _propertyService.GetPropertiesByLandlordIdAsync(userId.Value);
                     PropertyOptions = new SelectList(properties, "PropertyId", "Title");
                     return Page();
                 }
             }

             if (NewListing.Status == "active")
             {
                 NewListing.PublishedAt = DateTime.UtcNow;
                 NewListing.ExpiresAt = DateTime.UtcNow.AddDays(30); // Default 30 days
             }

             await _listingService.CreateListingAsync(NewListing);
             
             TempData["Success"] = "Đăng tin thành công!";
             return RedirectToPage("./Index");
        }
    }
}
