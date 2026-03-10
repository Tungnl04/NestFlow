using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace NestFlow.Pages.Landlord
{
    public class InvoicesModel : PageModel
    {
        public int? LandlordId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Check session for login
            var userId = HttpContext.Session.GetInt32("UserId");
            var userType = HttpContext.Session.GetString("UserType");
            LandlordId = HttpContext.Session.GetInt32("UserId");

            if (userId == null || userType != "landlord")
            {
                // Redirect if not logged in or not landlord
                return RedirectToPage("/Home/Index");
            }

            return Page();
        }
    }
}
