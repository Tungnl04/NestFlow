using Microsoft.EntityFrameworkCore;
using NestFlow.Application.Services.Interfaces;
using NestFlow.Models;

namespace NestFlow.Application.Services
{
    public class PropertyService : IPropertyService
    {
        private readonly NestFlowSystemContext _context;

        public PropertyService(NestFlowSystemContext context)
        {
            _context = context;
        }

        public async Task<List<Property>> GetPropertiesByLandlordIdAsync(long landlordId)
        {
            return await _context.Properties
                .Where(p => p.LandlordId == landlordId && p.Status != "deleted") // Assuming logical delete, or hard delete
                // Note: DB check constraint says status IN ('available', 'rented', 'unavailable'). 
                // We will use 'unavailable' for hidden/deleted logically if not implementing soft delete column.
                // Or just standard query. 
                // Let's stick to standard status logic.
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Property?> GetPropertyByIdAsync(long propertyId)
        {
            return await _context.Properties
                .Include(p => p.PropertyImages)
                .Include(p => p.Amenities)
                .FirstOrDefaultAsync(p => p.PropertyId == propertyId);
        }

        public async Task<Property> CreatePropertyAsync(Property property)
        {
            property.CreatedAt = DateTime.UtcNow;
            property.UpdatedAt = DateTime.UtcNow;
            property.Status = "available"; 
            property.ViewCount = 0;

            _context.Properties.Add(property);
            await _context.SaveChangesAsync();
            return property;
        }

        public async Task<Property> UpdatePropertyAsync(Property property)
        {
            var existing = await _context.Properties.FindAsync(property.PropertyId);
            if (existing == null) throw new KeyNotFoundException("Property not found");

            // Update fields manually to avoid overwriting unrelated fields or use Mapper
            existing.Title = property.Title;
            existing.Description = property.Description;
            existing.PropertyType = property.PropertyType;
            existing.Address = property.Address;
            existing.Ward = property.Ward;
            existing.District = property.District;
            existing.City = property.City;
            existing.Area = property.Area;
            existing.Price = property.Price;
            existing.Deposit = property.Deposit;
            existing.MaxOccupants = property.MaxOccupants;
            existing.AvailableFrom = property.AvailableFrom;
            existing.Status = property.Status;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeletePropertyAsync(long propertyId)
        {
            var property = await _context.Properties.FindAsync(propertyId);
            if (property == null) return false;

            // Hard delete or Soft delete?
            // "unavailable" is a valid status. Let's use hard delete for now as per simple CRUD, 
            // OR set status to unavailable. 
            // Let's go with Hard Delete for simplicity unless constraints block it.
            // Constraints: Bookings, Listings might reference it.
            // Safer: Set status to 'unavailable'.
            // But user might want to really delete drafting errors.
            // Let's try remove. If error, we catch it in Controller.
            
            try 
            {
                _context.Properties.Remove(property);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                // Likely FK constraint
                return false;
            }
        }
    }
}
