using Microsoft.AspNetCore.Mvc;
using NestFlow.Application.DTOs;
using NestFlow.Application.Services.Interfaces;
namespace NestFlow.Controllers
{
    /// <summary>
    /// API Controller để quản lý người dùng
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách người dùng với phân trang và bộ lọc
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResultDto<UserListDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResultDto<UserListDto>>> GetUsers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? userType = null,
            [FromQuery] string? status = null)
        {
            try
            {
                // Validate input
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var result = await _userService.GetUsersAsync(page, pageSize, search, userType, status);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users list");
                return StatusCode(500, new { message = "Lỗi khi tải danh sách người dùng" });
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết một người dùng
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserDetailDto>> GetUser(long id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);

                if (user == null)
                {
                    return NotFound(new { message = "Không tìm thấy người dùng" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user {UserId}", id);
                return StatusCode(500, new { message = "Lỗi khi tải thông tin người dùng" });
            }
        }

        /// <summary>
        /// Cập nhật trạng thái người dùng
        /// </summary>
        [HttpPut("{id}/status")]
        [ProducesResponseType(typeof(ApiResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto>> UpdateUserStatus(long id, [FromBody] UpdateUserStatusDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Status))
                {
                    return BadRequest(new { message = "Trạng thái không được để trống" });
                }

                var result = await _userService.UpdateUserStatusAsync(id, request.Status);

                if (!result.Success)
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for user {UserId}", id);
                return StatusCode(500, new { message = "Lỗi khi cập nhật trạng thái" });
            }
        }

        /// <summary>
        /// Xác thực người dùng
        /// </summary>
        [HttpPut("{id}/verify")]
        [ProducesResponseType(typeof(ApiResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto>> VerifyUser(long id)
        {
            try
            {
                var result = await _userService.VerifyUserAsync(id);

                if (!result.Success)
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying user {UserId}", id);
                return StatusCode(500, new { message = "Lỗi khi xác thực người dùng" });
            }
        }


        /// <summary>
        /// Lấy thống kê tổng quan về người dùng
        /// </summary>
        /// <returns>Thống kê tổng quan</returns>
        [HttpGet("statistics")]
        [ProducesResponseType(typeof(UsersOverviewStatisticsDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<UsersOverviewStatisticsDto>> GetStatistics()
        {
            try
            {
                var statistics = await _userService.GetStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user statistics");
                return StatusCode(500, new { message = "Lỗi khi tải thống kê" });
            }
        }

        /// <summary>
        /// Cập nhật thông tin người dùng
        /// </summary>
        /// <param name="id">ID của người dùng</param>
        /// <param name="request">Thông tin cập nhật</param>
        /// <returns>Kết quả cập nhật</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto>> UpdateUser(long id, [FromBody] UpdateUserDto request)
        {
            try
            {
                var result = await _userService.UpdateUserAsync(id, request);

                if (!result.Success)
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", id);
                return StatusCode(500, new { message = "Lỗi khi cập nhật thông tin người dùng" });
            }
        }
    }
}