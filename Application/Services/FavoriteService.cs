using Microsoft.EntityFrameworkCore;
using NestFlow.Application.Services.Interfaces;
using NestFlow.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NestFlow.Application.Services
{
    public class FavoriteService : IFavoriteService
    {
        private readonly NestFlowSystemContext _context;

        public FavoriteService(NestFlowSystemContext context)
        {
            _context = context;
        }

        public async Task<bool> ToggleFavoriteAsync(long userId, long propertyId)
        {
            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.PropertyId == propertyId);

            if (favorite != null)
            {
                // Remove
                _context.Favorites.Remove(favorite);
                await _context.SaveChangesAsync();
                return false; // Not favorited anymore
            }
            else
            {
                // Add
                favorite = new Favorite
                {
                    UserId = userId,
                    PropertyId = propertyId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Favorites.Add(favorite);
                await _context.SaveChangesAsync();
                return true; // Favorited
            }
        }

        public async Task<List<Favorite>> GetUserFavoritesAsync(long userId)
        {
            return await _context.Favorites
                .Where(f => f.UserId == userId)
                .Include(f => f.Property)
                    .ThenInclude(p => p.PropertyImages) // Eager load Images
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> IsFavoritedAsync(long userId, long propertyId)
        {
            return await _context.Favorites
                .AnyAsync(f => f.UserId == userId && f.PropertyId == propertyId);
        }
    }
}
