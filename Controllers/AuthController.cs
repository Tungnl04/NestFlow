using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NestFlow.Application.DTOs;
using NestFlow.Application.Services.Interfaces;

namespace NestFlow.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "Dữ liệu không hợp lệ"
                });
            }

            var result = await _authService.LoginAsync(loginDto);

            if (!result.Success)
            {
                return Unauthorized(result);
            }

            // Set session
            HttpContext.Session.SetInt32("UserId", (int)result.User!.UserId);
            HttpContext.Session.SetString("UserType", result.User.UserType);
            HttpContext.Session.SetString("Email", result.User.Email);

            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "Dữ liệu không hợp lệ"
                });
            }

            var result = await _authService.RegisterAsync(registerDto);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            // Set session sau khi đăng ký thành công
            HttpContext.Session.SetInt32("UserId", (int)result.User!.UserId);
            HttpContext.Session.SetString("UserType", result.User.UserType);
            HttpContext.Session.SetString("Email", result.User.Email);

            return Ok(result);
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new AuthResponseDto
            {
                Success = true,
                Message = "Đăng xuất thành công"
            });
        }

        [HttpGet("current-user")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Unauthorized(new AuthResponseDto
                {
                    Success = false,
                    Message = "Chưa đăng nhập"
                });
            }

            var user = await _authService.GetCurrentUserAsync(userId.Value);
            if (user == null)
            {
                return NotFound(new AuthResponseDto
                {
                    Success = false,
                    Message = "Không tìm thấy người dùng"
                });
            }

            return Ok(new AuthResponseDto
            {
                Success = true,
                Message = "Lấy thông tin thành công",
                User = user
            });
        }

        [HttpGet("check-session")]
        public IActionResult CheckSession()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userType = HttpContext.Session.GetString("UserType");

            if (userId == null)
            {
                return Ok(new
                {
                    isLoggedIn = false
                });
            }

            return Ok(new
            {
                isLoggedIn = true,
                userId = userId,
                userType = userType
            });
        }
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto updateDto)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Unauthorized(new AuthResponseDto
                {
                    Success = false,
                    Message = "Chưa đăng nhập"
                });
            }

            var result = await _authService.UpdateProfileAsync(userId.Value, updateDto);

            if (result.Success && result.User != null)
            {
                // Cập nhật session nếu email thay đổi
                if (!string.IsNullOrEmpty(updateDto.Email))
                {
                    HttpContext.Session.SetString("Email", result.User.Email);
                }
            }

            return Ok(result);
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Unauthorized(new AuthResponseDto
                {
                    Success = false,
                    Message = "Chưa đăng nhập"
                });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "Dữ liệu không hợp lệ"
                });
            }

            var result = await _authService.ChangePasswordAsync(userId.Value, changePasswordDto);

            return Ok(result);
        }
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "Dữ liệu không hợp lệ"
                });
            }

            var result = await _authService.ForgotPasswordAsync(forgotPasswordDto.Email);
            return Ok(result);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "Dữ liệu không hợp lệ"
                });
            }

            var result = await _authService.ResetPasswordAsync(resetPasswordDto);
            return Ok(result);
        }
    }
}
