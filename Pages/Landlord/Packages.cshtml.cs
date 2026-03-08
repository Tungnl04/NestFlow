using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NestFlow.Models;

namespace NestFlow.Pages.Landlord
{
    public class PackagesModel : PageModel
    {
        private readonly NestFlowSystemContext _context;

        public PackagesModel(NestFlowSystemContext context)
        {
            _context = context;
        }

        public LandlordSubscription? ActiveSubscription { get; set; }
        public Plan? ActivePlan { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Home/Index");
            }

            // Lấy subscription đang active
            ActiveSubscription = await _context.LandlordSubscriptions
                .Include(s => s.Plan)
                .Where(s => s.LandlordId == userId.Value && s.Status == "Active" && s.EndAt > DateTime.Now)
                .OrderByDescending(s => s.EndAt)
                .FirstOrDefaultAsync();

            if (ActiveSubscription != null)
            {
                ActivePlan = ActiveSubscription.Plan;
            }

            return Page();
        }
    }
}
