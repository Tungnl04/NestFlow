using Microsoft.EntityFrameworkCore;
using NestFlow.Application.DTOs;
using NestFlow.Application.Services.Interfaces;
using NestFlow.DTOs;
using NestFlow.Models;

namespace NestFlow.Application.Services
{
    public class RoomService : IRoomService
    {
        private readonly NestFlowSystemContext _context;
        private readonly ILogger<RoomService> _logger;

        public RoomService(NestFlowSystemContext context, ILogger<RoomService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<RoomDetailDto?> GetRoomDetailAsync(long propertyId, long landlordId)
        {
            var room = await _context.Properties
                .Where(p => p.PropertyId == propertyId && p.LandlordId == landlordId)
                .Select(p => new RoomDetailDto
                {
                    PropertyId = p.PropertyId,
                    RoomNumber = p.RoomNumber ?? "",
                    Title = p.Title,
                    Description = p.Description,
                    Area = p.Area,
                    Price = p.Price,
                    Deposit = p.Deposit,
                    MaxOccupants = p.MaxOccupants ?? 2,
                    Status = p.Status ?? "available",

                    BuildingName = p.Building.BuildingName,
                    FloorName = p.Floor.FloorName,

                    Amenities = p.PropertyAmenities
                        .Select(pa => pa.Amenity.Name)
                        .ToList(),

                    CurrentRental = p.CurrentRental == null ? null : new RoomRentalInfoDto
                    {
                        RentalId = p.CurrentRental.RentalId,
                        StartDate = p.CurrentRental.StartDate.ToDateTime(TimeOnly.MinValue),
                        EndDate = p.CurrentRental.EndDate.HasValue
                            ? p.CurrentRental.EndDate.Value.ToDateTime(TimeOnly.MinValue)
                            : null,
                        MonthlyRent = p.CurrentRental.MonthlyRent ?? 0,
                        DepositAmount = p.CurrentRental.DepositAmount ?? 0,

                        Occupants = p.CurrentRental.RentalOccupants
                            .Where(ro => ro.Status == "active")
                            .Select(ro => new OccupantInfoDto
                            {
                                OccupantId = ro.OccupantId,
                                FullName = ro.FullName ?? (ro.User != null ? ro.User.FullName : ""),
                                Phone = ro.Phone ?? (ro.User != null ? ro.User.Phone : null),
                                IdNumber = ro.IdNumber,
                                MoveInDate = ro.MoveInDate.HasValue
                                    ? ro.MoveInDate.Value.ToDateTime(TimeOnly.MinValue)
                                    : null
                            }).ToList()
                    }
                })
                .FirstOrDefaultAsync();

            if (room == null) return null;

            room.StatusDisplay = GetStatusDisplay(room.Status);

            if (room.CurrentRental?.EndDate != null)
            {
                room.CurrentRental.DaysRemaining =
                    (int)(room.CurrentRental.EndDate.Value - DateTime.Now).TotalDays;
            }

            return room;
        }
        public async Task<ApiResponseDto> CreateRoomAsync(long landlordId, CreateRoomDto dto)
        {
            try
            {
                // Kiểm tra building và floor có thuộc landlord không
                var floor = await _context.Floors
                    .Include(f => f.Building)
                    .FirstOrDefaultAsync(f => f.FloorId == dto.FloorId && f.Building.LandlordId == landlordId);

                if (floor == null)
                {
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "Không tìm thấy tầng"
                    };
                }

                // Kiểm tra room number đã tồn tại chưa
                var exists = await _context.Properties
                    .AnyAsync(p => p.BuildingId == dto.BuildingId && p.RoomNumber == dto.RoomNumber);

                if (exists)
                {
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "Mã phòng đã tồn tại"
                    };
                }

                var property = new Property
                {
                    LandlordId = landlordId,
                    BuildingId = dto.BuildingId,
                    FloorId = dto.FloorId,
                    RoomNumber = dto.RoomNumber,
                    Title = dto.Title,
                    Description = dto.Description,
                    PropertyType = "phong_tro",
                    Area = dto.Area,
                    Price = dto.Price,
                    Deposit = dto.Deposit,
                    MaxOccupants = dto.MaxOccupants,
                    Status = "available",
                    CurrentOccupantsCount = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Properties.Add(property);

                // Cập nhật số phòng của floor và building
                floor.RoomsCount += 1;
                floor.Building.TotalRooms = (floor.Building.TotalRooms ?? 0) + 1;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Room {RoomNumber} created in building {BuildingId}", dto.RoomNumber, dto.BuildingId);

                return new ApiResponseDto
                {
                    Success = true,
                    Message = "Tạo phòng thành công",
                    Data = new { PropertyId = property.PropertyId }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating room");
                return new ApiResponseDto
                {
                    Success = false,
                    Message = "Lỗi khi tạo phòng"
                };
            }
        }

        public async Task<ApiResponseDto> UpdateRoomAsync(long propertyId, long landlordId, UpdateRoomDto dto)
        {

            try
            {
                var property = await _context.Properties
                    .FirstOrDefaultAsync(p => p.PropertyId == propertyId && p.LandlordId == landlordId);


                if (property == null)
                {
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "Không tìm thấy phòng"
                    };
                }
                if (dto.Status != null)
                {
                    if ((dto.Status == "available" || dto.Status == "inactive")
                        && property.Status == "occupied")
                        return new ApiResponseDto
                        {
                            Success = false,
                            Message = "Không thể đổi trạng thái khi phòng đang có người thuê"
                        };
                    property.Status = dto.Status;
                }
                if (dto.Title != null) property.Title = dto.Title;
                if (dto.Description != null) property.Description = dto.Description;
                if (dto.Area.HasValue) property.Area = dto.Area.Value;
                if (dto.Price.HasValue) property.Price = dto.Price.Value;
                if (dto.Deposit.HasValue) property.Deposit = dto.Deposit.Value;
                if (dto.MaxOccupants.HasValue) property.MaxOccupants = dto.MaxOccupants.Value;
                if (dto.Status != null) property.Status = dto.Status;

                property.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Room {PropertyId} updated", propertyId);

                return new ApiResponseDto
                {
                    Success = true,
                    Message = "Cập nhật thông tin phòng thành công"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating room {PropertyId}", propertyId);
                return new ApiResponseDto
                {
                    Success = false,
                    Message = "Lỗi khi cập nhật thông tin phòng"
                };
            }
        }

        public async Task<ApiResponseDto> UpdateRoomStatusAsync(long propertyId, long landlordId, string status)
        {
            try
            {
                var property = await _context.Properties
                    .FirstOrDefaultAsync(p => p.PropertyId == propertyId && p.LandlordId == landlordId);

                if (property == null)
                {
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "Không tìm thấy phòng"
                    };
                }

                var validStatuses = new[] { "available", "occupied", "maintenance", "inactive" };
                if (!validStatuses.Contains(status))
                {
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "Trạng thái không hợp lệ"
                    };
                }

                property.Status = status;
                property.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return new ApiResponseDto
                {
                    Success = true,
                    Message = "Cập nhật trạng thái phòng thành công"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating room status");
                return new ApiResponseDto
                {
                    Success = false,
                    Message = "Lỗi khi cập nhật trạng thái phòng"
                };
            }
        }

        public async Task<ApiResponseDto> DeleteRoomAsync(long propertyId, long landlordId)
        {
            try
            {
                var property = await _context.Properties
                    .Include(p => p.Floor)
                        .ThenInclude(f => f!.Building)
                    .FirstOrDefaultAsync(p => p.PropertyId == propertyId && p.LandlordId == landlordId);

                if (property == null)
                {
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "Không tìm thấy phòng"
                    };
                }

                if (property.Status == "occupied")
                {
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "Không thể xóa phòng đang được thuê"
                    };
                }

                _context.Properties.Remove(property);

                // Cập nhật số phòng
                if (property.Floor != null)
                {
                    property.Floor.RoomsCount -= 1;
                    if (property.Floor.Building != null)
                    {
                        property.Floor.Building.TotalRooms = (property.Floor.Building.TotalRooms ?? 0) - 1;
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Room {PropertyId} deleted", propertyId);

                return new ApiResponseDto
                {
                    Success = true,
                    Message = "Xóa phòng thành công"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting room {PropertyId}", propertyId);
                return new ApiResponseDto
                {
                    Success = false,
                    Message = "Lỗi khi xóa phòng"
                };
            }
        }

        public async Task<ApiResponseDto> AddOccupantAsync(long landlordId, AddOccupantDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var property = await _context.Properties
                    .Include(p => p.CurrentRental)
                        .ThenInclude(r => r!.RentalOccupants)
                    .FirstOrDefaultAsync(p => p.PropertyId == dto.PropertyId && p.LandlordId == landlordId);

                if (property == null)
                {
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "Không tìm thấy phòng"
                    };
                }

                Rental rental;

                // Nếu chưa có rental (người thuê đầu tiên) thì tạo mới
                if (property.CurrentRental == null)
                {
                    if (!dto.MonthlyRent.HasValue || !dto.DepositAmount.HasValue)
                    {
                        return new ApiResponseDto
                        {
                            Success = false,
                            Message = "Vui lòng nhập thông tin tiền thuê và tiền cọc"
                        };
                    }

                    rental = new Rental
                    {
                        PropertyId = dto.PropertyId,
                        LandlordId = landlordId,
                        RenterId = dto.UserId,
                        StartDate = DateOnly.FromDateTime(dto.MoveInDate),
                        EndDate = dto.MoveOutExpected != null
                                    ? DateOnly.FromDateTime(dto.MoveOutExpected.Value): null,
                        MonthlyRent = dto.MonthlyRent.Value,
                        DepositAmount = dto.DepositAmount.Value,
                        PaymentDueDate = dto.PaymentDueDate ?? 5,
                        Status = "active",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.Rentals.Add(rental);
                    await _context.SaveChangesAsync();

                    property.CurrentRentalId = rental.RentalId;
                    property.Status = "occupied";
                }
                else
                {
                    rental = property.CurrentRental;

                    // Kiểm tra số người tối đa
                    var currentOccupantsCount = rental.RentalOccupants.Count(ro => ro.Status == "active");
                    if (currentOccupantsCount >= property.MaxOccupants)
                    {
                        return new ApiResponseDto
                        {
                            Success = false,
                            Message = $"Phòng đã đủ {property.MaxOccupants} người"
                        };
                    }
                }

                // Thêm người ở
                var occupant = new RentalOccupant
                {
                    RentalId = rental.RentalId,
                    UserId = dto.UserId,
                    FullName = dto.FullName,
                    Phone = dto.Phone,
                    IdNumber = dto.IdNumber,
                    MoveInDate = DateOnly.FromDateTime(dto.MoveInDate),
                    MoveOutExpected = dto.MoveOutExpected != null
                            ? DateOnly.FromDateTime(dto.MoveOutExpected.Value):null,
                    Status = "active",
                    CreatedAt = DateTime.UtcNow
                };

                _context.RentalOccupants.Add(occupant);

                // Nếu là người thuê chính và chưa có renter_id     
                if (dto.IsPrimaryRenter && dto.UserId.HasValue)
                {
                    rental.RenterId = dto.UserId.Value;
                }

                // Cập nhật số người đang ở
                property.CurrentOccupantsCount = (property.CurrentRental?.RentalOccupants.Count(ro => ro.Status == "active") ?? 0) + 1;
                property.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Occupant added to rental {RentalId}", rental.RentalId);

                return new ApiResponseDto
                {
                    Success = true,
                    Message = "Thêm người thuê thành công"
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error adding occupant");
                return new ApiResponseDto
                {
                    Success = false,
                    Message = "Lỗi khi thêm người thuê"
                };
            }
        }

        public async Task<ApiResponseDto> RemoveOccupantAsync(long landlordId, RemoveOccupantDto dto)
        {
            try
            {
                var occupant = await _context.RentalOccupants
                    .Include(ro => ro.Rental)
                        .ThenInclude(r => r.Property)
                    .FirstOrDefaultAsync(ro => ro.OccupantId == dto.OccupantId);

                if (occupant == null || occupant.Rental.Property.LandlordId != landlordId)
                {
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "Không tìm thấy người thuê"
                    };
                }

                occupant.Status = "left";
                occupant.MoveOutExpected = DateOnly.FromDateTime(dto.MoveOutDate);

                await _context.SaveChangesAsync();

                var property = occupant.Rental.Property;

                property.CurrentOccupantsCount = await _context.RentalOccupants
                    .CountAsync(ro => ro.RentalId == occupant.RentalId && ro.Status == "active");

                property.UpdatedAt = DateTime.UtcNow;

                if (property.CurrentOccupantsCount == 0)
                {
                    property.Status = "available";
                    property.CurrentRentalId = null;
                    occupant.Rental.Status = "terminated";
                    occupant.Rental.TerminationDate = DateOnly.FromDateTime(dto.MoveOutDate);
                    occupant.Rental.TerminationReason = dto.Reason;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Occupant {OccupantId} removed", dto.OccupantId);

                return new ApiResponseDto
                {
                    Success = true,
                    Message = "Xóa người thuê thành công"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing occupant");
                return new ApiResponseDto
                {
                    Success = false,
                    Message = "Lỗi khi xóa người thuê"
                };
            }
        }

        public async Task<ApiResponseDto> EndRentalAsync(long landlordId, EndRentalDto dto)
        {
            try
            {
                var rental = await _context.Rentals
                    .Include(r => r.Property)
                    .Include(r => r.RentalOccupants)
                    .FirstOrDefaultAsync(r => r.RentalId == dto.RentalId && r.LandlordId == landlordId);

                if (rental == null)
                {
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "Không tìm thấy hợp đồng thuê"
                    };
                }

                rental.Status = "terminated";
                rental.TerminationDate = DateOnly.FromDateTime(dto.TerminationDate);
                rental.TerminationReason = dto.TerminationReason;
                rental.UpdatedAt = DateTime.UtcNow;

                // Cập nhật tất cả occupants
                foreach (var occupant in rental.RentalOccupants.Where(ro => ro.Status == "active"))
                {
                    occupant.Status = "left";
                    occupant.MoveOutExpected = DateOnly.FromDateTime(dto.TerminationDate);
                }

                // Cập nhật phòng
                rental.Property.Status = "available";
                rental.Property.CurrentRentalId = null;
                rental.Property.CurrentOccupantsCount = 0;
                rental.Property.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Rental {RentalId} ended", dto.RentalId);

                return new ApiResponseDto
                {
                    Success = true,
                    Message = "Kết thúc hợp đồng thuê thành công"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ending rental");
                return new ApiResponseDto
                {
                    Success = false,
                    Message = "Lỗi khi kết thúc hợp đồng thuê"
                };
            }
        }

        private static string GetStatusDisplay(string status)
        {
            return status switch
            {
                "available" => "Trống",
                "occupied" => "Đang thuê",
                "maintenance" => "Bảo trì",
                "inactive" => "Ngừng hoạt động",
                _ => status
            };
        }
    }
}