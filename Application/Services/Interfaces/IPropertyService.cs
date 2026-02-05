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
    }
}
