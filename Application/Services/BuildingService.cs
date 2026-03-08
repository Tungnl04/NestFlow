using Microsoft.EntityFrameworkCore;
using NestFlow.Application.DTOs;
using NestFlow.Application.Services.Interfaces;
using NestFlow.DTOs;
using NestFlow.Models;

namespace NestFlow.Application.Services
{

    public class BuildingService : IBuildingService
    {
        private readonly NestFlowSystemContext _context;
        private readonly ILogger<BuildingService> _logger;

        public BuildingService(NestFlowSystemContext context, ILogger<BuildingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<BuildingListDto>> GetBuildingsByLandlordAsync(long landlordId)
        {
            var buildings = await _context.Buildings
                .Where(b => b.LandlordId == landlordId)
                .Select(b => new BuildingListDto
                {
                    BuildingId = b.BuildingId,
                    BuildingName = b.BuildingName,
                    FullAddress = (b.Address ?? "") + ", " + (b.Ward ?? "") + ", " + (b.District ?? "") + ", " + (b.City ?? ""),
                    TotalFloors = b.TotalFloors ?? 0,
                    TotalRooms = b.TotalRooms ?? 0,
                    IsSetupCompleted = b.IsSetupCompleted,
                    Statistics = new BuildingStatisticsDto
                    {
                        TotalRooms = b.Properties.Count(),
                        AvailableRooms = b.Properties.Count(p => p.Status == "available"),
                        OccupiedRooms = b.Properties.Count(p => p.Status == "occupied"),
                        MaintenanceRooms = b.Properties.Count(p => p.Status == "maintenance"),
                        InactiveRooms = b.Properties.Count(p => p.Status == "inactive"),
                        MonthlyRevenue = b.Properties.Where(p => p.Status == "occupied").Sum(p => p.Price ?? 0),
                        OccupancyRate = b.Properties.Any()
                            ? Math.Round((decimal)b.Properties.Count(p => p.Status == "occupied") / b.Properties.Count() * 100, 2)
                            : 0
                    }
                })
                .ToListAsync();

            return buildings;
        }

        public async Task<BuildingDetailDto?> GetBuildingDetailAsync(long buildingId, long landlordId)
        {
            var building = await _context.Buildings
                .Include(b => b.Floors)
                    .ThenInclude(f => f.Properties)
                        .ThenInclude(p => p.CurrentRental)
                            .ThenInclude(r => r.RentalOccupants)
                .Where(b => b.BuildingId == buildingId && b.LandlordId == landlordId)
                .Select(b => new BuildingDetailDto
                {
                    BuildingId = b.BuildingId,
                    BuildingName = b.BuildingName,
                    Address = b.Address,
                    Ward = b.Ward,
                    District = b.District,
                    City = b.City,
                    TotalFloors = b.TotalFloors ?? 0,
                    TotalRooms = b.TotalRooms ?? 0,
                    Description = b.Description,
                    IsSetupCompleted = b.IsSetupCompleted,
                    CreatedAt = b.CreatedAt,
                    Statistics = new BuildingStatisticsDto
                    {
                        TotalRooms = b.Properties.Count(),
                        AvailableRooms = b.Properties.Count(p => p.Status == "available"),
                        OccupiedRooms = b.Properties.Count(p => p.Status == "occupied"),
                        MaintenanceRooms = b.Properties.Count(p => p.Status == "maintenance"),
                        InactiveRooms = b.Properties.Count(p => p.Status == "inactive"),
                        MonthlyRevenue = b.Properties.Where(p => p.Status == "occupied").Sum(p => p.Price ?? 0),
                        OccupancyRate = b.Properties.Any()
                            ? Math.Round((decimal)b.Properties.Count(p => p.Status == "occupied") / b.Properties.Count() * 100, 2)
                            : 0
                    },
                    Floors = b.Floors.OrderByDescending(f => f.FloorNumber).Select(f => new FloorWithRoomsDto
                    {
                        FloorId = f.FloorId,
                        FloorNumber = f.FloorNumber,
                        FloorName = f.FloorName,
                        RoomsCount = f.RoomsCount,
                        Rooms = f.Properties.OrderBy(p => p.RoomNumber).Select(p => new RoomCardDto
                        {
                            PropertyId = p.PropertyId,
                            RoomNumber = p.RoomNumber ?? "",
                            Title = p.Title,
                            Area = p.Area,
                            Price = p.Price,
                            Status = p.Status ?? "available",
                            StatusDisplay = GetStatusDisplay(p.Status ?? "available"),

                            CurrentOccupants = p.CurrentRental != null
                            ? p.CurrentRental.RentalOccupants.Count(ro => ro.Status == "active")
                            : 0,

                            MaxOccupants = p.MaxOccupants ?? 2,

                            RenterName = p.CurrentRental != null ? p.CurrentRental.Renter.FullName : null,
                            RenterPhone = p.CurrentRental != null ? p.CurrentRental.Renter.Phone : null,

                            RentalStartDate = p.CurrentRental != null
                            ? p.CurrentRental.StartDate.ToDateTime(TimeOnly.MinValue)
                            : null,

                            RentalEndDate = p.CurrentRental != null && p.CurrentRental.EndDate.HasValue
                            ? p.CurrentRental.EndDate.Value.ToDateTime(TimeOnly.MinValue)
                            : null,
                        }).ToList()
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            return building;
        }

        public async Task<ApiResponseDto> CreateBuildingAsync(long landlordId, CreateBuildingDto dto)
        {
            try
            {
                var building = new Building
                {
                    LandlordId = landlordId,
                    BuildingName = dto.BuildingName,
                    Address = dto.Address,
                    Ward = dto.Ward,
                    District = dto.District,
                    City = dto.City,
                    Description = dto.Description,
                    TotalFloors = 0,
                    TotalRooms = 0,
                    IsSetupCompleted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Buildings.Add(building);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Building {BuildingId} created by landlord {LandlordId}", building.BuildingId, landlordId);

                return new ApiResponseDto
                {
                    Success = true,
                    Message = "Tạo nhà trọ thành công",
                    Data = new { BuildingId = building.BuildingId }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating building for landlord {LandlordId}", landlordId);
                return new ApiResponseDto
                {
                    Success = false,
                    Message = "Lỗi khi tạo nhà trọ"
                };
            }
        }

        public async Task<ApiResponseDto> UpdateBuildingAsync(long buildingId, long landlordId, UpdateBuildingDto dto)
        {
            try
            {
                var building = await _context.Buildings
                    .FirstOrDefaultAsync(b => b.BuildingId == buildingId && b.LandlordId == landlordId);

                if (building == null)
                {
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "Không tìm thấy nhà trọ"
                    };
                }

                if (dto.BuildingName != null) building.BuildingName = dto.BuildingName;
                if (dto.Address != null) building.Address = dto.Address;
                if (dto.Ward != null) building.Ward = dto.Ward;
                if (dto.District != null) building.District = dto.District;
                if (dto.City != null) building.City = dto.City;
                if (dto.Description != null) building.Description = dto.Description;

                building.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Building {BuildingId} updated", buildingId);

                return new ApiResponseDto
                {
                    Success = true,
                    Message = "Cập nhật thông tin nhà trọ thành công"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating building {BuildingId}", buildingId);
                return new ApiResponseDto
                {
                    Success = false,
                    Message = "Lỗi khi cập nhật thông tin nhà trọ"
                };
            }
        }

        public async Task<ApiResponseDto> DeleteBuildingAsync(long buildingId, long landlordId)
        {
            try
            {
                var building = await _context.Buildings
                    .Include(b => b.Properties)
                    .FirstOrDefaultAsync(b => b.BuildingId == buildingId && b.LandlordId == landlordId);

                if (building == null)
                {
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "Không tìm thấy nhà trọ"
                    };
                }

                // Kiểm tra có phòng đang thuê không
                var hasOccupiedRooms = building.Properties.Any(p => p.Status == "occupied");
                if (hasOccupiedRooms)
                {
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "Không thể xóa nhà trọ khi còn phòng đang được thuê"
                    };
                }

                _context.Buildings.Remove(building);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Building {BuildingId} deleted", buildingId);

                return new ApiResponseDto
                {
                    Success = true,
                    Message = "Xóa nhà trọ thành công"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting building {BuildingId}", buildingId);
                return new ApiResponseDto
                {
                    Success = false,
                    Message = "Lỗi khi xóa nhà trọ"
                };
            }
        }

        public async Task<ApiResponseDto> InitializeBuildingAsync(long buildingId, long landlordId, InitializeBuildingDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var building = await _context.Buildings
                    .Include(b => b.Floors)
                    .Include(b => b.Properties)
                    .FirstOrDefaultAsync(b => b.BuildingId == buildingId && b.LandlordId == landlordId);

                if (building == null)
                {
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "Không tìm thấy nhà trọ"
                    };
                }

                // Xóa dữ liệu cũ nếu có
                if (building.Floors.Any())
                {
                    var propertyIds = building.Properties
                        .Select(p => p.PropertyId)
                        .ToList();

                    // Xóa images
                    var images = await _context.PropertyImages
                        .Where(i => propertyIds.Contains(i.PropertyId))
                        .ToListAsync();

                    _context.PropertyImages.RemoveRange(images);

                    // Xóa bookings
                    var bookings = await _context.Bookings
                        .Where(b => propertyIds.Contains(b.PropertyId))
                        .ToListAsync();

                    _context.Bookings.RemoveRange(bookings);

                    // Xóa properties
                    _context.Properties.RemoveRange(building.Properties);

                    // Xóa floors
                    _context.Floors.RemoveRange(building.Floors);
                }

                // Tạo các tầng và phòng
                for (int floorNum = 1; floorNum <= dto.TotalFloors; floorNum++)
                {
                    var floor = new Floor
                    {
                        BuildingId = buildingId,
                        FloorNumber = floorNum,
                        FloorName = $"Tầng {floorNum}",
                        RoomsCount = dto.RoomsPerFloor,
                        DisplayOrder = floorNum,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Floors.Add(floor);
                    await _context.SaveChangesAsync(); // Save để có FloorId

                    // Tạo phòng cho tầng này
                    for (int roomNum = 1; roomNum <= dto.RoomsPerFloor; roomNum++)
                    {
                        var roomNumber = $"P{floorNum}{roomNum:D2}"; // P101, P102, P201...

                        var property = new Property
                        {
                            LandlordId = landlordId,
                            BuildingId = buildingId,
                            FloorId = floor.FloorId,
                            RoomNumber = roomNumber,
                            Title = $"Phòng {roomNumber}",
                            PropertyType = "phong_tro",
                            Status = "available",
                            Area = 25.00m,
                            Price = 2000000.00m,
                            Deposit = 2000000.00m,
                            MaxOccupants = 2,
                            CurrentOccupantsCount = 0,
                            ViewCount = 0,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        _context.Properties.Add(property);
                    }
                }

                // Cập nhật thông tin building
                building.TotalFloors = dto.TotalFloors;
                building.TotalRooms = dto.TotalFloors * dto.RoomsPerFloor;
                building.IsSetupCompleted = true;
                building.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Building {BuildingId} initialized with {Floors} floors and {Rooms} rooms",
                    buildingId, dto.TotalFloors, building.TotalRooms);

                return new ApiResponseDto
                {
                    Success = true,
                    Message = $"Khởi tạo thành công {dto.TotalFloors} tầng và {building.TotalRooms} phòng"
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error initializing building {BuildingId}", buildingId);
                return new ApiResponseDto
                {
                    Success = false,
                    Message = "Lỗi khi khởi tạo tầng và phòng"
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