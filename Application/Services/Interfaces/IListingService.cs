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
        
        /// <summary>
        /// Luồng đăng tin All-in-one: Tạo cả Property và Listing trong một Transaction
        /// </summary>
        Task<Listing> PublishNewListingWithPropertyAsync(Listing listing, Property property, List<PropertyImage> images, List<long> amenityIds);

        /// <summary>
        /// Cập nhật tin đăng All-in-one: Cập nhật cả Property và Listing
        /// </summary>
        Task<Listing> UpdateListingWithPropertyAsync(Listing listing, Property property, List<PropertyImage> newImages, List<long> amenityIds);
    }
}
