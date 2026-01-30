namespace NestFlow.Application.DTOs
{
    public class AuthResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public UserInfoDto? User { get; set; }
    }

    public class UserInfoDto
    {
        public long UserId { get; set; }
        public string Email { get; set; } = null!;
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? AvatarUrl { get; set; }
        public string UserType { get; set; } = null!;
        public bool? IsVerified { get; set; }
    }
}
