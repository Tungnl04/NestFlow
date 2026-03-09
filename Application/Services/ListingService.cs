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
            // Kiểm tra gói đăng ký và hạn mức
            var subscription = await _context.LandlordSubscriptions
                .Include(s => s.Plan)
                .Where(s => s.LandlordId == listing.LandlordId && s.Status == "active" && s.EndAt > DateTime.UtcNow)
                .OrderByDescending(s => s.Plan.PriorityLevel)
                .FirstOrDefaultAsync();

            if (subscription == null)
            {
                throw new InvalidOperationException("Quý khách hiện không có gói đăng tin nào còn hiệu lực. Vui lòng mua gói để tiếp tục.");
            }

            if (subscription.QuotaRemaining <= 0)
            {
                throw new InvalidOperationException("Quý khách đã sử dụng hết số lượt đăng tin của gói hiện tại.");
            }

            listing.CreatedAt = DateTime.UtcNow;
            listing.UpdatedAt = DateTime.UtcNow;
            
            // Mặc định là draft nếu không set active, hoặc nếu active thì trừ quota
            if (listing.Status == "active")
            {
                subscription.QuotaRemaining--;
                listing.PublishedAt = DateTime.UtcNow;
                listing.ExpiresAt = DateTime.UtcNow.AddDays(subscription.Plan.DurationDays > 0 ? subscription.Plan.DurationDays : 30);
            }
            else
            {
                listing.Status = "draft";
            }

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
            
            // Việc thay đổi status từ draft -> active nên được xử lý riêng ở ToggleListingStatusAsync 
            // hoặc phải kiểm tra quota ở đây nếu status thay đổi.
            // Để an toàn và tách biệt, chúng ta giữ logic update content ở đây.
            
            existing.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteListingAsync(long listingId)
        {
            var listing = await _context.Listings.FindAsync(listingId);
            if (listing == null) return false;

            _context.Listings.Remove(listing);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleListingStatusAsync(long listingId, string status)
        {
            var existing = await _context.Listings.FindAsync(listingId);
            if (existing == null) return false;

            if (status == "active" && existing.Status != "active")
            {
                // Kiểm tra quota khi kích hoạt lại tin
                var subscription = await _context.LandlordSubscriptions
                    .Include(s => s.Plan)
                    .Where(s => s.LandlordId == existing.LandlordId && s.Status == "active" && s.EndAt > DateTime.UtcNow)
                    .OrderByDescending(s => s.Plan.PriorityLevel)
                    .FirstOrDefaultAsync();

                if (subscription == null || subscription.QuotaRemaining <= 0)
                {
                    return false; // Trả về false nếu không đủ điều kiện kích hoạt
                }

                subscription.QuotaRemaining--;
                existing.PublishedAt = DateTime.UtcNow;
                existing.ExpiresAt = DateTime.UtcNow.AddDays(subscription.Plan.DurationDays > 0 ? subscription.Plan.DurationDays : 30);
            }

            existing.Status = status;
            existing.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<Listing> PublishNewListingWithPropertyAsync(Listing listing, Property property, List<PropertyImage> images, List<long> amenityIds)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Kiểm tra gói đăng ký và hạn mức
                var subscription = await _context.LandlordSubscriptions
                    .Include(s => s.Plan)
                    .Where(s => s.LandlordId == listing.LandlordId && s.Status == "active" && s.EndAt > DateTime.UtcNow)
                    .OrderByDescending(s => s.Plan.PriorityLevel)
                    .FirstOrDefaultAsync();

                if (subscription == null)
                {
                    throw new InvalidOperationException("Quý khách hiện không có gói đăng tin nào còn hiệu lực. Vui lòng mua gói để tiếp tục.");
                }

                if (subscription.QuotaRemaining <= 0)
                {
                    throw new InvalidOperationException("Quý khách đã sử dụng hết số lượt đăng tin của gói hiện tại.");
                }

                // 2. Lưu Property trước để lấy PropertyId
                property.CreatedAt = DateTime.UtcNow;
                property.UpdatedAt = DateTime.UtcNow;
                property.Status = "available";
                _context.Properties.Add(property);
                await _context.SaveChangesAsync();

                // 3. Lưu danh sách ảnh
                if (images != null && images.Count > 0)
                {
                    foreach (var img in images)
                    {
                        img.PropertyId = property.PropertyId;
                        img.UploadedAt = DateTime.UtcNow;
                        _context.PropertyImages.Add(img);
                    }
                }

                // 4. Lưu tiện nghi
                if (amenityIds != null && amenityIds.Count > 0)
                {
                    foreach (var amenityId in amenityIds)
                    {
                        _context.PropertyAmenities.Add(new PropertyAmenity { PropertyId = property.PropertyId, AmenityId = amenityId });
                    }
                }

                await _context.SaveChangesAsync();

                // 5. Lưu Listing liên kết
                listing.PropertyId = property.PropertyId;
                listing.CreatedAt = DateTime.UtcNow;
                listing.UpdatedAt = DateTime.UtcNow;
                listing.ViewCount = 0;
                listing.LikeCount = 0;

                // Xử lý status và quota
                if (listing.Status == "active")
                {
                    subscription.QuotaRemaining--;
                    listing.PublishedAt = DateTime.UtcNow;
                    listing.ExpiresAt = DateTime.UtcNow.AddDays(subscription.Plan.DurationDays > 0 ? subscription.Plan.DurationDays : 30);
                }
                else
                {
                    listing.Status = "draft";
                }

                _context.Listings.Add(listing);
                await _context.SaveChangesAsync();

                // 6. Hoàn tất
                await transaction.CommitAsync();
                return listing;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Listing> UpdateListingWithPropertyAsync(Listing listing, Property property, List<PropertyImage> newImages, List<long> amenityIds)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Cập nhật Listing
                var existingListing = await _context.Listings.FindAsync(listing.ListingId);
                if (existingListing == null) throw new KeyNotFoundException("Không tìm thấy tin đăng.");

                existingListing.Title = listing.Title;
                existingListing.Content = listing.Content;
                existingListing.Status = listing.Status;
                existingListing.UpdatedAt = DateTime.UtcNow;

                // 2. Cập nhật Property
                var existingProperty = await _context.Properties.FindAsync(property.PropertyId);
                if (existingProperty == null) throw new KeyNotFoundException("Không tìm thấy thông tin phòng.");

                existingProperty.Title = property.Title;
                existingProperty.PropertyType = property.PropertyType;
                existingProperty.Price = property.Price;
                existingProperty.Deposit = property.Deposit;
                existingProperty.Area = property.Area;
                existingProperty.MaxOccupants = property.MaxOccupants;
                existingProperty.City = property.City;
                existingProperty.District = property.District;
                existingProperty.Ward = property.Ward;
                existingProperty.Address = property.Address;
                existingProperty.UpdatedAt = DateTime.UtcNow;

                // 3. Cập nhật ảnh mới (nếu có)
                if (newImages != null && newImages.Count > 0)
                {
                    foreach (var img in newImages)
                    {
                        img.PropertyId = property.PropertyId;
                        img.UploadedAt = DateTime.UtcNow;
                        _context.PropertyImages.Add(img);
                    }
                }

                // 4. Cập nhật tiện nghi (Xóa cũ thêm mới)
                var currentAmenities = await _context.PropertyAmenities.Where(pa => pa.PropertyId == property.PropertyId).ToListAsync();
                _context.PropertyAmenities.RemoveRange(currentAmenities);

                if (amenityIds != null && amenityIds.Count > 0)
                {
                    foreach (var amenityId in amenityIds)
                    {
                        _context.PropertyAmenities.Add(new PropertyAmenity { PropertyId = property.PropertyId, AmenityId = amenityId });
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return existingListing;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
