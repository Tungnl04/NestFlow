using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NestFlow.Models;

namespace NestFlow.Pages.Room
{
    public class ContactModel : PageModel
    {
        private readonly NestFlowSystemContext _context;

        public ContactModel(NestFlowSystemContext context)
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

            // Kiểm tra đăng nhập
            var userId = HttpContext.Session.GetInt32("UserId");
            
            if (userId != null)
            {
                IsLoggedIn = true;
                CurrentUser = await _context.Users.FindAsync((long)userId.Value);
            }
            else
            {
                IsLoggedIn = false;
                CurrentUser = null;
            }

            Property = await _context.Properties
                .Include(p => p.PropertyImages)
                .Include(p => p.Landlord)
                .FirstOrDefaultAsync(p => p.PropertyId == id);

            if (Property == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}
