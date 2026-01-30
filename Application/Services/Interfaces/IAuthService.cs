using NestFlow.Application.DTOs;

namespace NestFlow.Application.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDTO loginDto);
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<UserInfoDto?> GetCurrentUserAsync(long userId);
    }
}
