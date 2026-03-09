using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NestFlow.Application.Services.Interfaces;
using NestFlow.Models;

namespace NestFlow.Pages.Landlord.Posts
{
    public class IndexModel : PageModel
    {
        private readonly IListingService _listingService;

        public IndexModel(IListingService listingService)
        {
            _listingService = listingService;
        }

        public List<Listing> Listings { get; set; } = new List<Listing>();

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userType = HttpContext.Session.GetString("UserType");

            if (userId == null || userType != "landlord") return RedirectToPage("/Home/Index");

            Listings = await _listingService.GetListingsByLandlordIdAsync(userId.Value);
            return Page();
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(long id, string status)
        {
             var userId = HttpContext.Session.GetInt32("UserId");
             if (userId == null) return RedirectToPage("/Home/Index");

             var list = await _listingService.GetListingByIdAsync(id);
             if(list == null || list.LandlordId != userId.Value) return NotFound();

             bool success = await _listingService.ToggleListingStatusAsync(id, status);
             if (!success && status == "active")
             {
                 TempData["Error"] = "Không thể kích hoạt tin đăng. Vui lòng kiểm tra lại thời hạn gói hoặc hạn mức (quota) của quý khách.";
             }
             else if (success)
             {
                 TempData["Success"] = status == "active" ? "Đã kích hoạt tin đăng thành công!" : "Đã chuyển tin đăng về trạng thái bản nháp.";
             }

             return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(long id)
        {
             var userId = HttpContext.Session.GetInt32("UserId");
             if (userId == null) return RedirectToPage("/Home/Index");

             var list = await _listingService.GetListingByIdAsync(id);
             if(list == null || list.Property.LandlordId != userId.Value) return NotFound();

             await _listingService.DeleteListingAsync(id);
             TempData["Success"] = "Đã xóa bài đăng thành công.";
             return RedirectToPage();
        }
    }
}
