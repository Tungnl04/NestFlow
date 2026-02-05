using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NestFlow.Application.Services.Interfaces;
using NestFlow.Models;

namespace NestFlow.Pages.Landlord.Properties
{
    public class IndexModel : PageModel
    {
        private readonly IPropertyService _propertyService;

        public IndexModel(IPropertyService propertyService)
        {
            _propertyService = propertyService;
        }

        public List<Property> Properties { get; set; } = new List<Property>();

        public async Task<IActionResult> OnGetAsync()
        {
            // Check session for login
            var userId = HttpContext.Session.GetInt32("UserId");
            var userType = HttpContext.Session.GetString("UserType");

            if (userId == null || userType != "landlord")
            {
                // Redirect if not logged in or not landlord
                return RedirectToPage("/Home/Index");
                // In real app, redirect to Login Page or Access Denied
            }

            Properties = await _propertyService.GetPropertiesByLandlordIdAsync(userId.Value);
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(long id)
        {
             var userId = HttpContext.Session.GetInt32("UserId");
             if (userId == null) return RedirectToPage("/Home/Index");

             // Verify ownership handles in service/controller, but here we call service delete directly
             // Or verify first
             var prop = await _propertyService.GetPropertyByIdAsync(id);
             if(prop == null || prop.LandlordId != userId.Value)
             {
                 return NotFound();
             }

             var result = await _propertyService.DeletePropertyAsync(id);
             if (!result)
             {
                 TempData["Error"] = "Không thể xóa phòng này (đang có dữ liệu liên quan).";
             }
             else
             {
                 TempData["Success"] = "Đã xóa phòng thành công.";
             }

             return RedirectToPage();
        }
    }
}
