using NestFlow.Models;

namespace NestFlow.Application.Services.Interfaces
{
    public interface IPropertyService
    {
        Task<List<Property>> GetPropertiesByLandlordIdAsync(long landlordId);
        Task<Property?> GetPropertyByIdAsync(long propertyId);
        Task<Property> CreatePropertyAsync(Property property);
        Task<Property> UpdatePropertyAsync(Property property);
        Task<bool> DeletePropertyAsync(long propertyId);
        
        // Image Management
        Task<PropertyImage> AddPropertyImageAsync(long propertyId, string imageUrl, bool isPrimary = false);
        Task<bool> DeletePropertyImageAsync(long imageId);

        // Amenities
        Task<List<Amenity>> GetAllAmenitiesAsync();

        // Search
        Task<NestFlow.Models.ViewModels.RoomSearchResponse> SearchRoomsAsync(NestFlow.Models.ViewModels.RoomSearchRequest request);
    }
}
