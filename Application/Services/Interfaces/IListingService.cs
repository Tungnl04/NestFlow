using NestFlow.Models;

namespace NestFlow.Application.Services.Interfaces
{
    public interface IListingService
    {
        Task<List<Listing>> GetListingsByLandlordIdAsync(long landlordId);
        Task<Listing?> GetListingByIdAsync(long listingId);
        Task<Listing> CreateListingAsync(Listing listing);
        Task<Listing> UpdateListingAsync(Listing listing);
        Task<bool> DeleteListingAsync(long listingId);
        Task<bool> ToggleListingStatusAsync(long listingId, string status);
    }
}
