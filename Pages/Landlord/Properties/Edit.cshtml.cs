using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NestFlow.Application.Services.Interfaces;
using NestFlow.Models;

namespace NestFlow.Pages.Landlord.Properties
{
    public class EditModel : PageModel
    {
        private readonly IPropertyService _propertyService;

        public EditModel(IPropertyService propertyService)
        {
            _propertyService = propertyService;
        }

        [BindProperty]
        public Property EditingProperty { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(long id)
        {
             var userId = HttpContext.Session.GetInt32("UserId");
             var userType = HttpContext.Session.GetString("UserType");
             if (userId == null || userType != "landlord") return RedirectToPage("/Home/Index");

             var prop = await _propertyService.GetPropertyByIdAsync(id);
             if (prop == null) return NotFound();
             
             // Security check
             if (prop.LandlordId != userId.Value) return Forbid();

             EditingProperty = prop;
             return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
             var userId = HttpContext.Session.GetInt32("UserId");
             if (userId == null) return RedirectToPage("/Home/Index");

             // Validate ownership again just in case hidden field was tampered? 
             // Ideally service handles it, but service takes the object. 
             // Let's ensure ID matches.
             if(EditingProperty.LandlordId != userId.Value) return Forbid();

             ModelState.Remove("EditingProperty.Landlord");

             if (!ModelState.IsValid)
             {
                 return Page();
             }

             await _propertyService.UpdatePropertyAsync(EditingProperty);
             
             TempData["Success"] = "Cập nhật thành công!";
             return RedirectToPage("./Index");
        }
    }
}
