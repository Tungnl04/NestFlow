using Microsoft.AspNetCore.Mvc.RazorPages;

namespace NestFlow.Pages.Landlord
{
    public class CommissionReportModel : PageModel
    {
        public void OnGet()
        {
            var landlordId = HttpContext.Session.GetInt32("UserId");
            
            if (landlordId == null)
            {
                Response.Redirect("/Home/Index");
                return;
            }
        }
    }
}
