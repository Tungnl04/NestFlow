using System;
using System.Collections.Generic;

namespace NestFlow.Models;

public partial class VwBuildingStatistic
{
    public long BuildingId { get; set; }

    public string BuildingName { get; set; } = null!;

    public long LandlordId { get; set; }

    public int? TotalFloors { get; set; }

    public int? TotalRooms { get; set; }

    public int? ActualRoomsCount { get; set; }

    public int? AvailableRooms { get; set; }

    public int? OccupiedRooms { get; set; }

    public int? MaintenanceRooms { get; set; }

    public int? InactiveRooms { get; set; }

    public decimal? MonthlyRevenue { get; set; }
}
