using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using NestFlow.Application.Services.Interfaces;
using NestFlow.Models;

namespace NestFlow.Pages.Landlord.Posts
{
    public class EditModel : PageModel
    {
        private readonly IListingService _listingService;
        private readonly IPropertyService _propertyService;

        public EditModel(IListingService listingService, IPropertyService propertyService)
        {
            _listingService = listingService;
            _propertyService = propertyService;
        }

        [BindProperty]
        public Listing EditingListing { get; set; } = null!;
        public SelectList PropertyOptions { get; set; }

        public async Task<IActionResult> OnGetAsync(long id)
        {
             var userId = HttpContext.Session.GetInt32("UserId");
             var userType = HttpContext.Session.GetString("UserType");
             if (userId == null || userType != "landlord") return RedirectToPage("/Home/Index");

             var list = await _listingService.GetListingByIdAsync(id);
             if (list == null) return NotFound();
             if (list.Property.LandlordId != userId.Value) return Forbid();

             EditingListing = list;

             var properties = await _propertyService.GetPropertiesByLandlordIdAsync(userId.Value);
             PropertyOptions = new SelectList(properties, "PropertyId", "Title");

             return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
             var userId = HttpContext.Session.GetInt32("UserId");
             if (userId == null) return RedirectToPage("/Home/Index");

             // Verify ownership
             var current = await _listingService.GetListingByIdAsync(EditingListing.ListingId);
             if(current == null || current.Property.LandlordId != userId.Value) return Forbid();

             if (!ModelState.IsValid)
             {
                 ModelState.Remove("EditingListing.Property");
                 ModelState.Remove("EditingListing.Landlord");

                 if (!ModelState.IsValid)
                 {
                     var properties = await _propertyService.GetPropertiesByLandlordIdAsync(userId.Value);
                     PropertyOptions = new SelectList(properties, "PropertyId", "Title");
                     return Page();
                 }
             }
             
             // Update publish date if switching to active
             if (EditingListing.Status == "active" && current.Status != "active")
             {
                 EditingListing.PublishedAt = DateTime.UtcNow;
                 if(EditingListing.ExpiresAt == null) EditingListing.ExpiresAt = DateTime.UtcNow.AddDays(30);
             }

             await _listingService.UpdateListingAsync(EditingListing);
             
             TempData["Success"] = "Cập nhật bài đăng thành công!";
             return RedirectToPage("./Index");
        }
    }
}
