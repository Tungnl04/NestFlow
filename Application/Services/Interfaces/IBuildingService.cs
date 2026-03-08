using NestFlow.Application.DTOs;
using NestFlow.DTOs;

namespace NestFlow.Application.Services.Interfaces
{
    public interface IBuildingService
    {
        Task<List<BuildingListDto>> GetBuildingsByLandlordAsync(long landlordId);
        Task<BuildingDetailDto?> GetBuildingDetailAsync(long buildingId, long landlordId);
        Task<ApiResponseDto> CreateBuildingAsync(long landlordId, CreateBuildingDto dto);
        Task<ApiResponseDto> UpdateBuildingAsync(long buildingId, long landlordId, UpdateBuildingDto dto);
        Task<ApiResponseDto> DeleteBuildingAsync(long buildingId, long landlordId);
        Task<ApiResponseDto> InitializeBuildingAsync(long buildingId, long landlordId, InitializeBuildingDto dto);
    }
}
