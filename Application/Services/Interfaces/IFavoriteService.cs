using NestFlow.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NestFlow.Application.Services.Interfaces
{
    public interface IFavoriteService
    {
        Task<bool> ToggleFavoriteAsync(long userId, long propertyId);
        Task<List<Favorite>> GetUserFavoritesAsync(long userId);
        Task<bool> IsFavoritedAsync(long userId, long propertyId);
    }
}
