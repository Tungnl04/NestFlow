using Microsoft.EntityFrameworkCore;
using NestFlow.Application.Services.Interfaces;
using NestFlow.Models;

namespace NestFlow.Application.Services
{
    public class ListingService : IListingService
    {
        private readonly NestFlowSystemContext _context;

        public ListingService(NestFlowSystemContext context)
        {
            _context = context;
        }

        public async Task<List<Listing>> GetListingsByLandlordIdAsync(long landlordId)
        {
            return await _context.Listings
                .Include(l => l.Property)
                .Where(l => l.Property.LandlordId == landlordId && l.Status != "deleted")
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<Listing?> GetListingByIdAsync(long listingId)
        {
            return await _context.Listings
                .Include(l => l.Property)
                .FirstOrDefaultAsync(l => l.ListingId == listingId);
        }

        public async Task<Listing> CreateListingAsync(Listing listing)
        {
            listing.CreatedAt = DateTime.UtcNow;
            listing.UpdatedAt = DateTime.UtcNow;
            listing.Status = "draft"; // Default to draft, user publishes later
            listing.ViewCount = 0;
            listing.LikeCount = 0;

            _context.Listings.Add(listing);
            await _context.SaveChangesAsync();
            return listing;
        }

        public async Task<Listing> UpdateListingAsync(Listing listing)
        {
            var existing = await _context.Listings.FindAsync(listing.ListingId);
            if (existing == null) throw new KeyNotFoundException("Listing not found");

            existing.Title = listing.Title;
            existing.Content = listing.Content;
            existing.Status = listing.Status;
            
            // Should check logic: can only be active if property is available?
            // "When listing active: check subscription..." -> This is complex logic for later (Subscription).
            
            existing.UpdatedAt = DateTime.UtcNow;
            existing.PublishedAt = listing.PublishedAt;
            existing.ExpiresAt = listing.ExpiresAt;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteListingAsync(long listingId)
        {
            var listing = await _context.Listings.FindAsync(listingId);
            if (listing == null) return false;

            _context.Listings.Remove(listing); // Hard delete for now
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleListingStatusAsync(long listingId, string status)
        {
            var existing = await _context.Listings.FindAsync(listingId);
            if (existing == null) return false;

            existing.Status = status;
            if (status == "active") existing.PublishedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
