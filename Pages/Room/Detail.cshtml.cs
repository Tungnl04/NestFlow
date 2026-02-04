using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace NestFlow.Pages.Room
{
    public class DetailModel : PageModel
    {
        private readonly NestFlow.Models.NestFlowSystemContext _context;

        public DetailModel(NestFlow.Models.NestFlowSystemContext context)
        {
            _context = context;
        }

        public NestFlow.Models.Property Property { get; set; }

        public async Task<IActionResult> OnGetAsync(long id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            Property = await _context.Properties
                .Include(p => p.PropertyImages)
                .Include(p => p.Landlord)
                .Include(p => p.Reviews)
                    .ThenInclude(r => r.User)
                .Include(p => p.Amenities)
                .FirstOrDefaultAsync(m => m.PropertyId == id);

            if (Property == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}
