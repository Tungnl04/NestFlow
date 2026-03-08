using System;
using System.Collections.Generic;

namespace NestFlow.Models;

public partial class VwRoomDetail
{
    public long PropertyId { get; set; }

    public long? BuildingId { get; set; }

    public long? FloorId { get; set; }

    public string? RoomNumber { get; set; }

    public string Title { get; set; } = null!;

    public decimal? Area { get; set; }

    public decimal? Price { get; set; }

    public decimal? Deposit { get; set; }

    public int? MaxOccupants { get; set; }

    public int CurrentOccupantsCount { get; set; }

    public string? Status { get; set; }

    public string? BuildingName { get; set; }

    public int? FloorNumber { get; set; }

    public string? FloorName { get; set; }

    public long? CurrentRentalId { get; set; }

    public long? RenterId { get; set; }

    public DateOnly? RentalStartDate { get; set; }

    public DateOnly? RentalEndDate { get; set; }

    public string? RenterName { get; set; }

    public string? RenterPhone { get; set; }

    public string? StatusDisplay { get; set; }
}
