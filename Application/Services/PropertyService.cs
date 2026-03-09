using Microsoft.EntityFrameworkCore;
using NestFlow.Application.Services.Interfaces;
using NestFlow.Models;
using NestFlow.Models.ViewModels;

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
                // Lọc bỏ những phòng trọ đã có bài đăng tin đang hoạt động hoặc bản nháp
                // Để chủ trọ không bị bối rối khi chọn phòng để đăng tin mới
                .Where(p => !p.Listings.Any(l => l.Status == "active" || l.Status == "draft"))
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

        public async Task<PropertyImage> AddPropertyImageAsync(long propertyId, string imageUrl, bool isPrimary = false)
        {
            // Auto order logic
            int displayOrder = 1;
            var currentMaxOrder = await _context.PropertyImages
                .Where(x => x.PropertyId == propertyId)
                .MaxAsync(x => (int?)x.DisplayOrder);
            
            if (currentMaxOrder.HasValue) 
                displayOrder = currentMaxOrder.Value + 1;

            var img = new PropertyImage
            {
                PropertyId = propertyId,
                ImageUrl = imageUrl,
                IsPrimary = isPrimary,
                DisplayOrder = displayOrder,
                UploadedAt = DateTime.UtcNow
            };

            _context.PropertyImages.Add(img);
            await _context.SaveChangesAsync();
            return img;
        }

        public async Task<bool> DeletePropertyImageAsync(long imageId)
        {
            var img = await _context.PropertyImages.FindAsync(imageId);
            if (img == null) return false;

            _context.PropertyImages.Remove(img);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<RoomSearchResponse> SearchRoomsAsync(RoomSearchRequest request)
        {
            var query = _context.Properties
                .AsNoTracking()
                .AsSplitQuery() // Prevent cartesian explosion with multiple includes
                .Where(p => p.Status == "available");

            // Filter by Keyword
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.Trim().ToLower();
                query = query.Where(p => 
                    p.Title.ToLower().Contains(keyword) || 
                    (p.Address != null && p.Address.ToLower().Contains(keyword)) ||
                    (p.District != null && p.District.ToLower().Contains(keyword)) ||
                    (p.Ward != null && p.Ward.ToLower().Contains(keyword)));
            }

            // Filter by Location
            if (!string.IsNullOrWhiteSpace(request.City))
            {
                var city = request.City.Trim().ToLower();
                query = query.Where(p => p.City.ToLower() == city);
            }
            if (!string.IsNullOrWhiteSpace(request.District))
            {
                var district = request.District.Trim().ToLower();
                query = query.Where(p => p.District.ToLower() == district);
            }
            if (!string.IsNullOrWhiteSpace(request.Ward))
            {
                var ward = request.Ward.Trim().ToLower();
                query = query.Where(p => p.Ward.ToLower() == ward);
            }

            // Filter by Price
            if (request.MinPrice.HasValue)
                query = query.Where(p => p.Price >= request.MinPrice.Value);
            if (request.MaxPrice.HasValue && request.MaxPrice > 0)
                query = query.Where(p => p.Price <= request.MaxPrice.Value);

            // Filter by Area
            if (request.MinArea.HasValue)
                query = query.Where(p => p.Area >= request.MinArea.Value);
            if (request.MaxArea.HasValue && request.MaxArea > 0)
                query = query.Where(p => p.Area <= request.MaxArea.Value);

            // Filter by PropertyType
            if (request.PropertyTypes != null && request.PropertyTypes.Any(x => !string.IsNullOrEmpty(x)))
            {
                query = query.Where(p => request.PropertyTypes.Contains(p.PropertyType));
            }

            // Filter by Amenities (must have ALL selected amenities)
            if (request.Amenities != null && request.Amenities.Any())
            {
                foreach (var amenityId in request.Amenities)
                {
                    query = query.Where(p => p.PropertyAmenities.Any(pa => pa.AmenityId == amenityId));
                }
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);
            if (totalPages == 0) totalPages = 1;

            // 1. Get current active subscription for each property's landlord to determine Priority
            // Note: In EF Core, we can use a subquery in OrderBy or Join.
            
            IOrderedQueryable<Property> propertiesQuery;
            
            // Define a helper to get priority
            // We use (int?) Max... ?? 0 to handle landlords without active subscriptions
            
            switch (request.SortBy)
            {
                case "price_asc":
                    propertiesQuery = query
                        .OrderByDescending(p => p.Landlord.LandlordSubscriptions
                            .Where(s => s.Status == "active" && s.EndAt > DateTime.UtcNow)
                            .Max(s => (int?)s.Plan.PriorityLevel) ?? 0)
                        .ThenBy(p => p.Price);
                    break;
                case "price_desc":
                    propertiesQuery = query
                        .OrderByDescending(p => p.Landlord.LandlordSubscriptions
                            .Where(s => s.Status == "active" && s.EndAt > DateTime.UtcNow)
                            .Max(s => (int?)s.Plan.PriorityLevel) ?? 0)
                        .ThenByDescending(p => p.Price);
                    break;
                case "area_desc":
                    propertiesQuery = query
                        .OrderByDescending(p => p.Landlord.LandlordSubscriptions
                            .Where(s => s.Status == "active" && s.EndAt > DateTime.UtcNow)
                            .Max(s => (int?)s.Plan.PriorityLevel) ?? 0)
                        .ThenByDescending(p => p.Area);
                    break;
                default:
                    propertiesQuery = query
                        .OrderByDescending(p => p.Landlord.LandlordSubscriptions
                            .Where(s => s.Status == "active" && s.EndAt > DateTime.UtcNow)
                            .Max(s => (int?)s.Plan.PriorityLevel) ?? 0)
                        .ThenByDescending(p => p.CreatedAt);
                    break;
            }

            var properties = await propertiesQuery
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(p => new PropertyViewModel
                {
                    Id = p.PropertyId,
                    Name = p.Title,
                    Price = p.Price ?? 0,
                    Location = $"{p.Ward}, {p.District}",
                    Rating = p.Reviews.Any() ? p.Reviews.Average(r => r.Rating ?? 0) : 0,
                    ReviewCount = p.Reviews.Count,
                    PostedDate = p.CreatedAt ?? DateTime.Now,
                    IsFeatured = p.ViewCount > 100,
                    Image = p.PropertyImages.OrderBy(img => img.DisplayOrder).Select(img => img.ImageUrl).FirstOrDefault() 
                            ?? "https://via.placeholder.com/300x200?text=No+Image",
                    
                    // Determine VIP Type for badge display
                    PriorityLevel = p.Landlord.LandlordSubscriptions
                        .Where(s => s.Status == "active" && s.EndAt > DateTime.UtcNow)
                        .Select(s => (int?)s.Plan.PriorityLevel)
                        .Max() ?? 0,
                    VipType = p.Landlord.LandlordSubscriptions
                        .Where(s => s.Status == "active" && s.EndAt > DateTime.UtcNow)
                        .OrderByDescending(s => s.Plan.PriorityLevel)
                        .Select(s => s.Plan.PlanName)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return new RoomSearchResponse
            {
                Properties = properties,
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPage = request.Page,
                PageSize = request.PageSize
            };
        }
        public async Task<List<Amenity>> GetAllAmenitiesAsync()
        {
            return await _context.Amenities.OrderBy(a => a.Category).ThenBy(a => a.Name).ToListAsync();
        }
    }
}
