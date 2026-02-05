using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NestFlow.Application.Services.Interfaces;
using NestFlow.Models.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NestFlow.Pages.Home
{
    public class SavedModel : PageModel
    {
        private readonly IFavoriteService _favoriteService;

        public SavedModel(IFavoriteService favoriteService)
        {
            _favoriteService = favoriteService;
        }

        public List<PropertyViewModel> SavedRooms { get; set; } = new List<PropertyViewModel>();
        public bool IsLoggedIn { get; set; } = false;

        public async Task<IActionResult> OnGetAsync()
        {
            long? userId = null;

            // 1. Try Claims
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userIdStr) && long.TryParse(userIdStr, out long parsedId))
            {
                userId = parsedId;
            }

            // 2. Try Session (Fallback)
            if (!userId.HasValue)
            {
                userId = HttpContext.Session.GetInt32("UserId");
            }

            if (!userId.HasValue)
            {
                return Page();
            }

            IsLoggedIn = true;

            // userId at this point is guaranteed to be long
            {
                var favorites = await _favoriteService.GetUserFavoritesAsync(userId.Value);
                
                SavedRooms = favorites.Select(f => new PropertyViewModel
                {
                    Id = f.PropertyId,
                    Name = f.Property.Title ?? "Phòng trọ",
                    Price = f.Property.Price ?? 0,
                    Location = $"{f.Property.Ward}, {f.Property.District}", // Simplified
                    Rating = 0, // Favorites usually don't load full reviews eager, unless updated service
                    ReviewCount = 0,
                    PostedDate = f.Property.CreatedAt ?? System.DateTime.Now,
                    IsFeatured = false,
                    Image = f.Property.PropertyImages?.FirstOrDefault()?.ImageUrl ?? "https://via.placeholder.com/300x200",
                    CreatedAt = f.CreatedAt // Date Saved
                }).ToList();
            }

            return Page();
        }
    }
}
