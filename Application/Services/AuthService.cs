using Microsoft.EntityFrameworkCore;
using NestFlow.Application.DTOs;
using NestFlow.Application.Services.Interfaces;
using NestFlow.Models;

namespace NestFlow.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly NestFlowSystemContext _context;
        private readonly IEmailService _emailService;
        public AuthService(NestFlowSystemContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
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
                // Gửi email chào mừng (không chặn luồng chính)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _emailService.SendWelcomeEmailAsync(newUser.Email, newUser.FullName);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error sending welcome email: {ex.Message}");
                    }
                });
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

        // Hash password sử dụng BCrypt
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11);
        }

        // Verify password
        private bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch
            {
                return false;
            }
        }
        public async Task<AuthResponseDto> UpdateProfileAsync(long userId, UpdateProfileDto updateDto)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Không tìm thấy người dùng"
                    };
                }

                // Kiểm tra email mới có bị trùng không
                if (!string.IsNullOrEmpty(updateDto.Email) && updateDto.Email != user.Email)
                {
                    var existingUser = await _context.Users
                        .FirstOrDefaultAsync(u => u.Email == updateDto.Email && u.UserId != userId);

                    if (existingUser != null)
                    {
                        return new AuthResponseDto
                        {
                            Success = false,
                            Message = "Email đã được sử dụng"
                        };
                    }
                    user.Email = updateDto.Email;
                }

                // Cập nhật thông tin
                if (!string.IsNullOrEmpty(updateDto.FullName))
                    user.FullName = updateDto.FullName;

                if (!string.IsNullOrEmpty(updateDto.Phone))
                    user.Phone = updateDto.Phone;

                if (!string.IsNullOrEmpty(updateDto.AvatarUrl))
                    user.AvatarUrl = updateDto.AvatarUrl;

                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Cập nhật thông tin thành công",
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
        public async Task<AuthResponseDto> ChangePasswordAsync(long userId, ChangePasswordDto changePasswordDto)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Không tìm thấy người dùng"
                    };
                }

                // Kiểm tra mật khẩu cũ
                if (!VerifyPassword(changePasswordDto.OldPassword, user.PasswordHash))
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Mật khẩu cũ không đúng"
                    };
                }

                // Kiểm tra mật khẩu mới và xác nhận
                if (changePasswordDto.NewPassword != changePasswordDto.ConfirmPassword)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Mật khẩu mới không khớp"
                    };
                }

                // Hash mật khẩu mới
                user.PasswordHash = HashPassword(changePasswordDto.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Đổi mật khẩu thành công"
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
        public async Task<AuthResponseDto> ForgotPasswordAsync(string email)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    // Trả về success để tránh leak thông tin user
                    return new AuthResponseDto
                    {
                        Success = true,
                        Message = "Nếu email tồn tại, mã xác thực đã được gửi"
                    };
                }

                // Tạo mã xác thực 6 số
                var random = new Random();
                var verificationCode = random.Next(100000, 999999).ToString();

                // Lưu token vào database
                var resetToken = new PasswordResetToken
                {
                    UserId = user.UserId,
                    Token = verificationCode,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                    IsUsed = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.PasswordResetTokens.Add(resetToken);
                await _context.SaveChangesAsync();

                // Gửi email với mã xác thực
                try
                {
                    await _emailService.SendVerificationCodeAsync(
                        user.Email,
                        verificationCode,
                        user.FullName
                    );
                }
                catch (Exception emailEx)
                {
                    Console.WriteLine($"Error sending email: {emailEx.Message}");
                    // Vẫn trả về success nhưng log lỗi
                }

                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Mã xác thực đã được gửi đến email của bạn"
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

        public async Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordDto resetDto)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == resetDto.Email);

                if (user == null)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Email không tồn tại"
                    };
                }

                // Tìm token hợp lệ
                var token = await _context.PasswordResetTokens
                    .Where(t => t.UserId == user.UserId
                        && t.Token == resetDto.VerificationCode
                        && !t.IsUsed
                        && t.ExpiresAt > DateTime.UtcNow)
                    .OrderByDescending(t => t.CreatedAt)
                    .FirstOrDefaultAsync();

                if (token == null)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Mã xác thực không hợp lệ hoặc đã hết hạn"
                    };
                }

                // Đánh dấu token đã sử dụng
                token.IsUsed = true;

                // Cập nhật mật khẩu mới
                user.PasswordHash = HashPassword(resetDto.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Đặt lại mật khẩu thành công"
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
    }
}
