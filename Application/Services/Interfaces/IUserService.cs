using NestFlow.Application.DTOs;

namespace NestFlow.Application.Services.Interfaces
{
    public interface IUserService
    {
        Task<PagedResultDto<UserListDto>> GetUsersAsync(int page, int pageSize, string? search, string? userType, string? status);
        Task<UserDetailDto?> GetUserByIdAsync(long userId);
        Task<UsersOverviewStatisticsDto> GetStatisticsAsync();
        Task<ApiResponseDto> UpdateUserStatusAsync(long userId, string status);
        Task<ApiResponseDto> VerifyUserAsync(long userId);
        Task<ApiResponseDto> UpdateUserAsync(long userId, UpdateUserDto dto);
    }
}
