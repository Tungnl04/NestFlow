

namespace NestFlow.DTOs
{
    // =====================================================================
    // BUILDING DTOs
    // =====================================================================

    public class BuildingListDto
    {
        public long BuildingId { get; set; }
        public string BuildingName { get; set; } = null!;
        public string? FullAddress { get; set; }
        public int TotalFloors { get; set; }
        public int TotalRooms { get; set; }
        public bool IsSetupCompleted { get; set; }
        public BuildingStatisticsDto? Statistics { get; set; }
    }

    public class BuildingDetailDto
    {
        public long BuildingId { get; set; }
        public string BuildingName { get; set; } = null!;
        public string? Address { get; set; }
        public string? Ward { get; set; }
        public string? District { get; set; }
        public string? City { get; set; }
        public int TotalFloors { get; set; }
        public int TotalRooms { get; set; }
        public string? Description { get; set; }
        public bool IsSetupCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public BuildingStatisticsDto Statistics { get; set; } = new();
        public List<FloorWithRoomsDto> Floors { get; set; } = new();
    }

    public class BuildingStatisticsDto
    {
        public int TotalRooms { get; set; }
        public int AvailableRooms { get; set; }
        public int OccupiedRooms { get; set; }
        public int MaintenanceRooms { get; set; }
        public int InactiveRooms { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal OccupancyRate { get; set; } // Tỷ lệ lấp đầy (%)
    }

    public class CreateBuildingDto
    {
        public string BuildingName { get; set; } = null!;
        public string? Address { get; set; }
        public string? Ward { get; set; }
        public string? District { get; set; }
        public string? City { get; set; }
        public string? Description { get; set; }
        public decimal? DefaultArea { get; set; }
        public int? DefaultMaxOccupants { get; set; }
        public decimal? DefaultPrice { get; set; }
        public decimal? DefaultDeposit { get; set; }

    }

    public class UpdateBuildingDto
    {
        public string? BuildingName { get; set; }
        public string? Address { get; set; }
        public string? Ward { get; set; }
        public string? District { get; set; }
        public string? City { get; set; }
        public string? Description { get; set; }
    }

    public class InitializeBuildingDto
    {
        public int TotalFloors { get; set; }
        public int RoomsPerFloor { get; set; }
        public decimal? DefaultArea { get; set; }
        public int? DefaultMaxOccupants { get; set; }
        public decimal? DefaultPrice { get; set; }
        public decimal? DefaultDeposit { get; set; }

    }

    // =====================================================================
    // FLOOR DTOs
    // =====================================================================

    public class FloorDto
    {
        public long FloorId { get; set; }
        public long BuildingId { get; set; }
        public int FloorNumber { get; set; }
        public string FloorName { get; set; } = null!;
        public int RoomsCount { get; set; }
    }

    public class FloorWithRoomsDto
    {
        public long FloorId { get; set; }
        public int FloorNumber { get; set; }
        public string FloorName { get; set; } = null!;
        public int RoomsCount { get; set; }
        public List<RoomCardDto> Rooms { get; set; } = new();
    }

    // =====================================================================
    // ROOM DTOs
    // =====================================================================

    /// <summary>
    /// DTO cho hiển thị room dạng card/tile trong giao diện
    /// </summary>
    public class RoomCardDto
    {
        public long PropertyId { get; set; }
        public string RoomNumber { get; set; } = null!;
        public string Title { get; set; } = null!;
        public decimal? Area { get; set; }
        public decimal? Price { get; set; }
        public string Status { get; set; } = null!; // available, occupied, maintenance, inactive
        public string StatusDisplay { get; set; } = null!; // Trống, Đang thuê, Bảo trì, Ngừng hoạt động
        public int CurrentOccupants { get; set; }
        public int MaxOccupants { get; set; }
        public string? RenterName { get; set; }
        public string? RenterPhone { get; set; }
        public DateTime? RentalStartDate { get; set; }
        public DateTime? RentalEndDate { get; set; }
    }

    /// <summary>
    /// DTO cho chi tiết đầy đủ của room
    /// </summary>
    public class RoomDetailDto
    {
        // Basic info
        public long PropertyId { get; set; }
        public string RoomNumber { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public decimal? Area { get; set; }
        public decimal? Price { get; set; }
        public decimal? Deposit { get; set; }
        public int MaxOccupants { get; set; }
        public string Status { get; set; } = null!;
        public string StatusDisplay { get; set; } = null!;

        // Building & Floor info
        public string BuildingName { get; set; } = null!;
        public string FloorName { get; set; } = null!;

        // Amenities
        public List<string> Amenities { get; set; } = new();

        // Current rental info
        public RoomRentalInfoDto? CurrentRental { get; set; }

        // Latest invoice
        public RoomInvoiceInfoDto? LatestInvoice { get; set; }
    }

    public class RoomRentalInfoDto
    {
        public long RentalId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int DaysRemaining { get; set; }
        public decimal MonthlyRent { get; set; }
        public decimal DepositAmount { get; set; }

        // Occupants
        public List<OccupantInfoDto> Occupants { get; set; } = new();
    }

    public class OccupantInfoDto
    {
        public long OccupantId { get; set; }
        public string FullName { get; set; } = null!;
        public string? Phone { get; set; }
        public string? IdNumber { get; set; }
        public bool IsPrimaryRenter { get; set; }
        public DateTime? MoveInDate { get; set; }
    }

    public class RoomInvoiceInfoDto
    {
        public long InvoiceId { get; set; }
        public string InvoiceMonth { get; set; } = null!;
        public decimal RoomRent { get; set; }
        public decimal ElectricAmount { get; set; }
        public decimal WaterAmount { get; set; }
        public decimal InternetFee { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? DueDate { get; set; }
        public DateTime? PaymentDate { get; set; }
    }

    public class CreateRoomDto
    {
        public long BuildingId { get; set; }
        public long FloorId { get; set; }
        public string RoomNumber { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Area { get; set; }
        public decimal Price { get; set; }
        public decimal Deposit { get; set; }
        public int MaxOccupants { get; set; }
    }

    public class UpdateRoomDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal? Area { get; set; }
        public decimal? Price { get; set; }
        public decimal? Deposit { get; set; }
        public int? MaxOccupants { get; set; }
        public string? Status { get; set; }
    }

    public class UpdateRoomStatusDto
    {
        public string Status { get; set; } = null!; // available, occupied, maintenance, inactive
    }

    // =====================================================================
    // RENTAL/OCCUPANT DTOs
    // =====================================================================

    public class AddOccupantDto
    {
        public long PropertyId { get; set; }
        public long? UserId { get; set; } // Nếu là user đã có trong hệ thống
        public string FullName { get; set; } = null!;
        public string? Phone { get; set; }
        public string? IdNumber { get; set; }
        public bool IsPrimaryRenter { get; set; }
        public DateTime MoveInDate { get; set; }
        public DateTime? MoveOutExpected { get; set; }

        // Rental info (chỉ cần khi thêm người thuê đầu tiên)
        public decimal? MonthlyRent { get; set; }
        public decimal? DepositAmount { get; set; }
        public int? PaymentDueDate { get; set; } // Ngày đóng tiền hàng tháng (1-31)
    }

    public class RemoveOccupantDto
    {
        public long OccupantId { get; set; }
        public DateTime MoveOutDate { get; set; }
        public string? Reason { get; set; }
    }

    public class EndRentalDto
    {
        public long RentalId { get; set; }
        public DateTime TerminationDate { get; set; }
        public string? TerminationReason { get; set; }
    }
}