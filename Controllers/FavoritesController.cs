using Microsoft.AspNetCore.Mvc;
using NestFlow.Application.Services.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NestFlow.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FavoritesController : ControllerBase
    {
        private readonly IFavoriteService _favoriteService;

        public FavoritesController(IFavoriteService favoriteService)
        {
            _favoriteService = favoriteService;
        }

        private long? GetCurrentUserId()
        {
            // 1. Try Claims
            var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(claimId) && long.TryParse(claimId, out var id))
                return id;

            // 2. Try Session (Dev/Support)
            return HttpContext.Session.GetInt32("UserId");
        }

        [HttpPost("toggle/{propertyId}")]
        public async Task<IActionResult> ToggleFavorite(long propertyId)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized(new { message = "Bạn cần đăng nhập để thực hiện chức năng này." });

            var result = await _favoriteService.ToggleFavoriteAsync(userId.Value, propertyId);
            return Ok(new { isFavorited = result, message = result ? "Đã lưu tin!" : "Đã bỏ lưu tin." });
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyFavorites()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized(new { message = "Bạn cần đăng nhập." });

            var favorites = await _favoriteService.GetUserFavoritesAsync(userId.Value);
            
            // Map to simpler result
            var result = favorites.Select(f => new 
            {
                f.FavoriteId,
                f.PropertyId,
                PropertyName = f.Property.Title ?? "Phòng trọ",
                Price = f.Property.Price,
                Image = f.Property.PropertyImages?.FirstOrDefault()?.ImageUrl ?? "/images/default-room.jpg",
                f.CreatedAt
            });

            return Ok(result);
        }

        [HttpGet("check/{propertyId}")]
        public async Task<IActionResult> CheckFavorite(long propertyId)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return Ok(new { isFavorited = false });

            var result = await _favoriteService.IsFavoritedAsync(userId.Value, propertyId);
            return Ok(new { isFavorited = result });
        }
    }
}
