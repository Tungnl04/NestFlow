using Microsoft.AspNetCore.Mvc;
using NestFlow.Application.DTOs;
using NestFlow.Application.Services.Interfaces;
using NestFlow.DTOs;

namespace NestFlow.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BuildingController : ControllerBase
    {
        private readonly IBuildingService _buildingService;
        private readonly ILogger<BuildingController> _logger;

        public BuildingController(
            IBuildingService buildingService,
            ILogger<BuildingController> logger)
        {
            _buildingService = buildingService;
            _logger = logger;
        }

        [HttpGet("landlord/{landlordId}")]
        [ProducesResponseType(typeof(List<BuildingListDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<BuildingListDto>>> GetBuildingsByLandlord(long landlordId)
        {
            try
            {
                var buildings = await _buildingService.GetBuildingsByLandlordAsync(landlordId);
                return Ok(buildings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting buildings for landlord {LandlordId}", landlordId);
                return StatusCode(500, new { message = "Lỗi khi tải danh sách nhà trọ" });
            }
        }

        /// <summary>
        /// Lấy chi tiết nhà trọ với tầng và phòng
        /// </summary>
        [HttpGet("{buildingId}/landlord/{landlordId}")]
        [ProducesResponseType(typeof(BuildingDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BuildingDetailDto>> GetBuildingDetail(long buildingId, long landlordId)
        {
            try
            {
                var building = await _buildingService.GetBuildingDetailAsync(buildingId, landlordId);

                if (building == null)
                {
                    return NotFound(new { message = "Không tìm thấy nhà trọ" });
                }

                return Ok(building);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting building detail {BuildingId}", buildingId);
                return StatusCode(500, new { message = "Lỗi khi tải thông tin nhà trọ" });
            }
        }

        /// <summary>
        /// Tạo nhà trọ mới
        /// </summary>
        [HttpPost("landlord/{landlordId}")]
        [ProducesResponseType(typeof(ApiResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponseDto>> CreateBuilding(long landlordId, [FromBody] CreateBuildingDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.BuildingName))
                {
                    return BadRequest(new { message = "Tên nhà trọ không được để trống" });
                }

                var result = await _buildingService.CreateBuildingAsync(landlordId, dto);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating building");
                return StatusCode(500, new { message = "Lỗi khi tạo nhà trọ" });
            }
        }

        /// <summary>
        /// Cập nhật thông tin nhà trọ
        /// </summary>
        [HttpPut("{buildingId}/landlord/{landlordId}")]
        [ProducesResponseType(typeof(ApiResponseDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponseDto>> UpdateBuilding(
            long buildingId,
            long landlordId,
            [FromBody] UpdateBuildingDto dto)
        {
            try
            {
                var result = await _buildingService.UpdateBuildingAsync(buildingId, landlordId, dto);

                if (!result.Success)
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating building {BuildingId}", buildingId);
                return StatusCode(500, new { message = "Lỗi khi cập nhật nhà trọ" });
            }
        }

        /// <summary>
        /// Xóa nhà trọ
        /// </summary>
        [HttpDelete("{buildingId}/landlord/{landlordId}")]
        [ProducesResponseType(typeof(ApiResponseDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponseDto>> DeleteBuilding(long buildingId, long landlordId)
        {
            try
            {
                var result = await _buildingService.DeleteBuildingAsync(buildingId, landlordId);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting building {BuildingId}", buildingId);
                return StatusCode(500, new { message = "Lỗi khi xóa nhà trọ" });
            }
        }

        /// <summary>
        /// Khởi tạo tầng và phòng cho nhà trọ
        /// </summary>
        [HttpPost("{buildingId}/landlord/{landlordId}/initialize")]
        [ProducesResponseType(typeof(ApiResponseDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponseDto>> InitializeBuilding(
            long buildingId,
            long landlordId,
            [FromBody] InitializeBuildingDto dto)
        {
            try
            {
                if (dto.TotalFloors < 1 || dto.TotalFloors > 20)
                {
                    return BadRequest(new { message = "Số tầng phải từ 1 đến 20" });
                }

                if (dto.RoomsPerFloor < 1 || dto.RoomsPerFloor > 50)
                {
                    return BadRequest(new { message = "Số phòng mỗi tầng phải từ 1 đến 50" });
                }

                var result = await _buildingService.InitializeBuildingAsync(buildingId, landlordId, dto);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing building {BuildingId}", buildingId);
                return StatusCode(500, new { message = "Lỗi khi khởi tạo tầng và phòng" });
            }
        }
    }
}