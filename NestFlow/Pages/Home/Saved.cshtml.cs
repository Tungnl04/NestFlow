using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace NestFlow.Pages.Home
{
    public class SavedModel : PageModel
    {
        public bool RequireTenantRole { get; set; }

        public void OnGet()
        {
        }
    }
}
