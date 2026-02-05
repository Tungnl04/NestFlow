using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NestFlow.Models;

namespace NestFlow.Pages.Landlord
{
    public class CommissionSettingsModel : PageModel
    {
        private readonly NestFlowSystemContext _context;

        public CommissionSettingsModel(NestFlowSystemContext context)
        {
            _context = context;
        }

        public List<Property> Properties { get; set; } = new();

        public async Task OnGetAsync()
        {
            var landlordId = HttpContext.Session.GetInt32("UserId");
            
            if (landlordId == null)
            {
                Response.Redirect("/Home/Index");
                return;
            }

            // Get all properties của landlord
            Properties = await _context.Properties
                .Where(p => p.LandlordId == landlordId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
    }
}
