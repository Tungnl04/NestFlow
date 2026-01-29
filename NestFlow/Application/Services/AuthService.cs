using Microsoft.EntityFrameworkCore;
using NestFlow.Application.DTOs;
using NestFlow.Application.Services.Interfaces;
using NestFlow.Models;
using System.Security.Cryptography;
using System.Text;

namespace NestFlow.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly NestFlowSystemContext _context;

        public AuthService(NestFlowSystemContext context)
        {
            _context = context;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDTO loginDTO)
        {
            try
            {
                // Tìm user theo email
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == loginDTO.Email);

                if (user == null)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Email hoặc mật khẩu không đúng"
                    };
                }

                // Kiểm tra mật khẩu
                if (!VerifyPassword(loginDTO.Password, user.PasswordHash))
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Email hoặc mật khẩu không đúng"
                    };
                }

                // Kiểm tra trạng thái tài khoản
                if (user.Status == "banned")
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Tài khoản của bạn đã bị khóa"
                    };
                }

                // Đăng nhập thành công
                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Đăng nhập thành công",
                    User = new UserInfoDto
                    {
                        UserId = user.UserId,
                        Email = user.Email,
                        FullName = user.FullName,
                        Phone = user.Phone,
                        AvatarUrl = user.AvatarUrl,
                        UserType = user.UserType,
                        IsVerified = user.IsVerified
                    }
                };
            }
            catch (Exception ex)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = $"Đã xảy ra lỗi: {ex.Message}"
                };
            }
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                // Kiểm tra email đã tồn tại
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == registerDto.Email);

                if (existingUser != null)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Email đã được sử dụng"
                    };
                }

                // Hash mật khẩu
                var passwordHash = HashPassword(registerDto.Password);

                // Tạo user mới
                var newUser = new User
                {
                    Email = registerDto.Email,
                    PasswordHash = passwordHash,
                    FullName = registerDto.FullName,
                    Phone = registerDto.Phone,
                    UserType = registerDto.UserType,
                    IsVerified = false,
                    Status = "active",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Đăng ký thành công",
                    User = new UserInfoDto
                    {
                        UserId = newUser.UserId,
                        Email = newUser.Email,
                        FullName = newUser.FullName,
                        Phone = newUser.Phone,
                        AvatarUrl = newUser.AvatarUrl,
                        UserType = newUser.UserType,
                        IsVerified = newUser.IsVerified
                    }
                };
            }
            catch (Exception ex)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = $"Đã xảy ra lỗi: {ex.Message}"
                };
            }
        }

        public async Task<UserInfoDto?> GetCurrentUserAsync(long userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.Status == "banned")
            {
                return null;
            }

            return new UserInfoDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                Phone = user.Phone,
                AvatarUrl = user.AvatarUrl,
                UserType = user.UserType,
                IsVerified = user.IsVerified
            };
        }

        // Hash password sử dụng SHA256
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        // Verify password
        private bool VerifyPassword(string password, string hashedPassword)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == hashedPassword;
        }
    }
}
