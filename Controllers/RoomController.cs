using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NestFlow.Application.DTOs;
using NestFlow.Application.Services.Interfaces;
using NestFlow.DTOs;

namespace NestFlow.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly IRoomService _roomService;
        private readonly ILogger<RoomController> _logger;

        public RoomController(
            IRoomService roomService,
            ILogger<RoomController> logger)
        {
            _roomService = roomService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy chi tiết phòng
        /// </summary>
        [HttpGet("{propertyId}/landlord/{landlordId}")]
        [ProducesResponseType(typeof(RoomDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RoomDetailDto>> GetRoomDetail(long propertyId, long landlordId)
        {
            try
            {
                var room = await _roomService.GetRoomDetailAsync(propertyId, landlordId);

                if (room == null)
                {
                    return NotFound(new { message = "Không tìm thấy phòng" });
                }

                return Ok(room);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting room detail {PropertyId}", propertyId);
                return StatusCode(500, new { message = "Lỗi khi tải thông tin phòng" });
            }
        }

        /// <summary>
        /// Tạo phòng mới
        /// </summary>
        [HttpPost("landlord/{landlordId}")]
        [ProducesResponseType(typeof(ApiResponseDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponseDto>> CreateRoom(long landlordId, [FromBody] CreateRoomDto dto)
        {
            try
            {
                var result = await _roomService.CreateRoomAsync(landlordId, dto);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating room");
                return StatusCode(500, new { message = "Lỗi khi tạo phòng" });
            }
        }

        /// <summary>
        /// Cập nhật thông tin phòng
        /// </summary>
        [HttpPut("{propertyId}/landlord/{landlordId}")]
        [ProducesResponseType(typeof(ApiResponseDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponseDto>> UpdateRoom(
            long propertyId,
            long landlordId,
            [FromBody] UpdateRoomDto dto)
        {
            try
            {
                var result = await _roomService.UpdateRoomAsync(propertyId, landlordId, dto);

                if (!result.Success)
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating room {PropertyId}", propertyId);
                return StatusCode(500, new { message = "Lỗi khi cập nhật phòng" });
            }
        }

        /// <summary>
        /// Cập nhật trạng thái phòng
        /// </summary>
        [HttpPut("{propertyId}/landlord/{landlordId}/status")]
        [ProducesResponseType(typeof(ApiResponseDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponseDto>> UpdateRoomStatus(
            long propertyId,
            long landlordId,
            [FromBody] UpdateRoomStatusDto dto)
        {
            try
            {
                var result = await _roomService.UpdateRoomStatusAsync(propertyId, landlordId, dto.Status);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating room status");
                return StatusCode(500, new { message = "Lỗi khi cập nhật trạng thái phòng" });
            }
        }

        /// <summary>
        /// Xóa phòng
        /// </summary>
        [HttpDelete("{propertyId}/landlord/{landlordId}")]
        [ProducesResponseType(typeof(ApiResponseDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponseDto>> DeleteRoom(long propertyId, long landlordId)
        {
            try
            {
                var result = await _roomService.DeleteRoomAsync(propertyId, landlordId);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting room {PropertyId}", propertyId);
                return StatusCode(500, new { message = "Lỗi khi xóa phòng" });
            }
        }

        /// <summary>
        /// Thêm người thuê vào phòng
        /// </summary>
        [HttpPost("landlord/{landlordId}/add-occupant")]
        [ProducesResponseType(typeof(ApiResponseDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponseDto>> AddOccupant(
            long landlordId,
            [FromBody] AddOccupantDto dto)
        {
            try
            {
                var result = await _roomService.AddOccupantAsync(landlordId, dto);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding occupant");
                return StatusCode(500, new { message = "Lỗi khi thêm người thuê" });
            }
        }

        /// <summary>
        /// Xóa người thuê khỏi phòng
        /// </summary>
        [HttpPost("landlord/{landlordId}/remove-occupant")]
        [ProducesResponseType(typeof(ApiResponseDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponseDto>> RemoveOccupant(
            long landlordId,
            [FromBody] RemoveOccupantDto dto)
        {
            try
            {
                var result = await _roomService.RemoveOccupantAsync(landlordId, dto);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing occupant");
                return StatusCode(500, new { message = "Lỗi khi xóa người thuê" });
            }
        }

        /// <summary>
        /// Kết thúc hợp đồng thuê
        /// </summary>
        [HttpPost("landlord/{landlordId}/end-rental")]
        [ProducesResponseType(typeof(ApiResponseDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponseDto>> EndRental(
            long landlordId,
            [FromBody] EndRentalDto dto)
        {
            try
            {
                var result = await _roomService.EndRentalAsync(landlordId, dto);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ending rental");
                return StatusCode(500, new { message = "Lỗi khi kết thúc hợp đồng" });
            }
        }
    }
}
