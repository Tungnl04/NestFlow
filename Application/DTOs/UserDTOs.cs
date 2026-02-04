namespace NestFlow.Application.DTOs
{
    /// <summary>
    /// DTO cho danh sách user (list view)
    /// </summary>
    public class UserListDto
    {
        public long UserId { get; set; }
        public string Email { get; set; } = null!;
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? AvatarUrl { get; set; }
        public string UserType { get; set; } = null!;
        public bool? IsVerified { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int ListingsCount { get; set; }
        public int BookingsCount { get; set; }
    }

    /// <summary>
    /// DTO cho chi tiết user
    /// </summary>
    public class UserDetailDto
    {
        public long UserId { get; set; }
        public string Email { get; set; } = null!;
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? AvatarUrl { get; set; }
        public string UserType { get; set; } = null!;
        public bool? IsVerified { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public UserStatisticsDto Statistics { get; set; } = new();
    }

    /// <summary>
    /// DTO cho thống kê user
    /// </summary>
    public class UserStatisticsDto
    {
        public int TotalListings { get; set; }
        public int ActiveListings { get; set; }
        public int TotalBookings { get; set; }
        public int TotalProperties { get; set; }
        public decimal WalletBalance { get; set; }
    }

    /// <summary>
    /// DTO cho thống kê tổng quan
    /// </summary>
    public class UsersOverviewStatisticsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int Landlords { get; set; }
        public int Renters { get; set; }
        public int VerifiedUsers { get; set; }
        public int NewUsersLastWeek { get; set; }
    }

    /// <summary>
    /// DTO cho phân trang
    /// </summary>
    public class PaginationDto
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// DTO cho response có phân trang
    /// </summary>
    public class PagedResultDto<T>
    {
        public List<T> Data { get; set; } = new();
        public PaginationDto Pagination { get; set; } = new();
    }

    /// <summary>
    /// DTO cho request cập nhật trạng thái
    /// </summary>
    public class UpdateUserStatusDto
    {
        public string Status { get; set; } = null!;
    }

    /// <summary>
    /// DTO cho request cập nhật thông tin user
    /// </summary>
    public class UpdateUserDto
    {
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? AvatarUrl { get; set; }
    }

    /// <summary>
    /// DTO cho response thông báo
    /// </summary>
    public class ApiResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public object? Data { get; set; }
    }
    
}
