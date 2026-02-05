using Microsoft.EntityFrameworkCore;
using NestFlow.Application.Services.Interfaces;
using NestFlow.Application.DTOs;
using NestFlow.Models;

namespace NestFlow.Application.Services
{
    public class UserService : IUserService
    {
        private readonly NestFlowSystemContext _context;
        private readonly ILogger<UserService> _logger;

        public UserService(NestFlowSystemContext context, ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PagedResultDto<UserListDto>> GetUsersAsync(
            int page,
            int pageSize,
            string? search,
            string? userType,
            string? status)
        {
            var query = _context.Users.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u =>
                    u.Email.Contains(search) ||
                    (u.FullName != null && u.FullName.Contains(search)) ||
                    (u.Phone != null && u.Phone.Contains(search))
                );
            }

            if (!string.IsNullOrEmpty(userType))
            {
                query = query.Where(u => u.UserType == userType);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(u => u.Status == status);
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserListDto
                {
                    UserId = u.UserId,
                    Email = u.Email,
                    FullName = u.FullName,
                    Phone = u.Phone,
                    AvatarUrl = u.AvatarUrl,
                    UserType = u.UserType,
                    IsVerified = u.IsVerified,
                    Status = u.Status,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt,
                    ListingsCount = u.Listings.Count,
                    BookingsCount = u.Bookings.Count
                })
                .ToListAsync();

            return new PagedResultDto<UserListDto>
            {
                Data = users,
                Pagination = new PaginationDto
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                }
            };
        }

        public async Task<UserDetailDto?> GetUserByIdAsync(long userId)
        {
            var user = await _context.Users
                .Include(u => u.Listings)
                .Include(u => u.Bookings)
                .Include(u => u.Properties)
                .Include(u => u.Wallet)
                .Where(u => u.UserId == userId)
                .Select(u => new UserDetailDto
                {
                    UserId = u.UserId,
                    Email = u.Email,
                    FullName = u.FullName,
                    Phone = u.Phone,
                    AvatarUrl = u.AvatarUrl,
                    UserType = u.UserType,
                    IsVerified = u.IsVerified,
                    Status = u.Status,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt,
                    Statistics = new UserStatisticsDto
                    {
                        TotalListings = u.Listings.Count,
                        ActiveListings = u.Listings.Count(l => l.Status == "active"),
                        TotalBookings = u.Bookings.Count,
                        TotalProperties = u.Properties.Count,
                        WalletBalance = u.Wallet != null ? u.Wallet.AvailableBalance : 0m
                    }
                })
                .FirstOrDefaultAsync();

            return user;
        }

        public async Task<UsersOverviewStatisticsDto> GetStatisticsAsync()
        {
            var totalUsers = await _context.Users.CountAsync();
            var activeUsers = await _context.Users.CountAsync(u => u.Status == "active");
            var landlords = await _context.Users.CountAsync(u => u.UserType == "landlord");
            var renters = await _context.Users.CountAsync(u => u.UserType == "renter");
            var verifiedUsers = await _context.Users.CountAsync(u => u.IsVerified == true);

            var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
            var newUsersLastWeek = await _context.Users
                .CountAsync(u => u.CreatedAt >= sevenDaysAgo);

            return new UsersOverviewStatisticsDto
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                Landlords = landlords,
                Renters = renters,
                VerifiedUsers = verifiedUsers,
                NewUsersLastWeek = newUsersLastWeek
            };
        }

        public async Task<ApiResponseDto> UpdateUserStatusAsync(long userId, string status)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "Không tìm thấy người dùng"
                    };
                }

                // Validate status
                var validStatuses = new[] { "active", "inactive", "banned", "deleted" };
                if (!validStatuses.Contains(status.ToLower()))
                {
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "Trạng thái không hợp lệ"
                    };
                }

                user.Status = status;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("User {UserId} status updated to {Status}", userId, status);

                return new ApiResponseDto
                {
                    Success = true,
                    Message = "Cập nhật trạng thái thành công",
                    Data = new { Status = status }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId} status", userId);
                return new ApiResponseDto
                {
                    Success = false,
                    Message = "Lỗi khi cập nhật trạng thái"
                };
            }
        }

        public async Task<ApiResponseDto> VerifyUserAsync(long userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "Không tìm thấy người dùng"
                    };
                }

                if (user.IsVerified == true)
                {
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "Người dùng đã được xác thực"
                    };
                }

                user.IsVerified = true;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("User {UserId} verified successfully", userId);

                return new ApiResponseDto
                {
                    Success = true,
                    Message = "Xác thực người dùng thành công"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying user {UserId}", userId);
                return new ApiResponseDto
                {
                    Success = false,
                    Message = "Lỗi khi xác thực người dùng"
                };
            }
        }


        public async Task<ApiResponseDto> UpdateUserAsync(long userId, UpdateUserDto dto)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "Không tìm thấy người dùng"
                    };
                }

                // Update fields
                if (dto.FullName != null)
                    user.FullName = dto.FullName;

                if (dto.Phone != null)
                    user.Phone = dto.Phone;

                if (dto.AvatarUrl != null)
                    user.AvatarUrl = dto.AvatarUrl;

                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("User {UserId} updated successfully", userId);

                return new ApiResponseDto
                {
                    Success = true,
                    Message = "Cập nhật thông tin thành công"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", userId);
                return new ApiResponseDto
                {
                    Success = false,
                    Message = "Lỗi khi cập nhật thông tin"
                };
            }
        }
    }
}