using NestFlow.Application.DTOs;

namespace NestFlow.Application.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDTO loginDto);
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<UserInfoDto?> GetCurrentUserAsync(long userId);

        Task<AuthResponseDto> UpdateProfileAsync(long userId, UpdateProfileDto updateDto);

        Task<AuthResponseDto> ChangePasswordAsync(long userId, ChangePasswordDto changePasswordDto);

        Task<AuthResponseDto> ForgotPasswordAsync(string email);

        Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordDto resetDto);
    }
}
