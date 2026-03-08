using NestFlow.Application.DTOs;
using NestFlow.DTOs;

namespace NestFlow.Application.Services.Interfaces
{
    public interface IRoomService
    {
        Task<RoomDetailDto?> GetRoomDetailAsync(long propertyId, long landlordId);
        Task<ApiResponseDto> CreateRoomAsync(long landlordId, CreateRoomDto dto);
        Task<ApiResponseDto> UpdateRoomAsync(long propertyId, long landlordId, UpdateRoomDto dto);
        Task<ApiResponseDto> UpdateRoomStatusAsync(long propertyId, long landlordId, string status);
        Task<ApiResponseDto> DeleteRoomAsync(long propertyId, long landlordId);
        Task<ApiResponseDto> AddOccupantAsync(long landlordId, AddOccupantDto dto);
        Task<ApiResponseDto> RemoveOccupantAsync(long landlordId, RemoveOccupantDto dto);
        Task<ApiResponseDto> EndRentalAsync(long landlordId, EndRentalDto dto);
    }
}
