using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NestFlow.Models;

namespace NestFlow.Pages.Payment
{
    public class CheckoutModel : PageModel
    {
        private readonly NestFlowSystemContext _context;

        public CheckoutModel(NestFlowSystemContext context)
        {
            _context = context;
        }

        public Property? Property { get; set; }
        public bool IsLoggedIn { get; set; }
        public User? CurrentUser { get; set; }

        public async Task<IActionResult> OnGetAsync(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Check if user is logged in (optional)
            var userId = HttpContext.Session.GetInt32("UserId");
            IsLoggedIn = userId != null;

            if (IsLoggedIn)
            {
                CurrentUser = await _context.Users.FindAsync((long)userId.Value);
            }

            Property = await _context.Properties
                .Include(p => p.PropertyImages)
                .Include(p => p.Landlord)
                .FirstOrDefaultAsync(p => p.PropertyId == id);

            if (Property == null)
            {
                return NotFound();
            }

            // Check if property is available
            if (Property.Status != "Available")
            {
                TempData["ErrorMessage"] = "Bất động sản này hiện không khả dụng";
                return RedirectToPage("/Room/Detail", new { id = Property.PropertyId });
            }

            return Page();
        }
    }
}
