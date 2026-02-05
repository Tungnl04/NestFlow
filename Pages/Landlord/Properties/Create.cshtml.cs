using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NestFlow.Application.Services.Interfaces;
using NestFlow.Models;

namespace NestFlow.Pages.Landlord.Properties
{
    public class CreateModel : PageModel
    {
        private readonly IPropertyService _propertyService;

        public CreateModel(IPropertyService propertyService)
        {
            _propertyService = propertyService;
        }

        [BindProperty]
        public Property NewProperty { get; set; } = new Property();

        public IActionResult OnGet()
        {
             var userId = HttpContext.Session.GetInt32("UserId");
             var userType = HttpContext.Session.GetString("UserType");
             if (userId == null || userType != "landlord") return RedirectToPage("/Home/Index");

             // Set default
             NewProperty.AvailableFrom = DateOnly.FromDateTime(DateTime.Today);
             return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
             var userId = HttpContext.Session.GetInt32("UserId");
             if (userId == null) return RedirectToPage("/Home/Index");

             // Force landlord ID
             NewProperty.LandlordId = userId.Value;
             NewProperty.Status = "available"; // Default

             // Remove navigation property from validation
             ModelState.Remove("NewProperty.Landlord");

             if (!ModelState.IsValid)
             {
                 return Page();
             }

             await _propertyService.CreatePropertyAsync(NewProperty);
             
             TempData["Success"] = "Đã thêm phòng trọ thành công!";
             return RedirectToPage("./Index");
        }
    }
}
