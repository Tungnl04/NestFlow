using System;
using System.Collections.Generic;

namespace NestFlow.Models;

public partial class Building
{
    public long BuildingId { get; set; }

    public long LandlordId { get; set; }

    public string BuildingName { get; set; } = null!;

    public string? Address { get; set; }

    public string? Ward { get; set; }

    public string? District { get; set; }

    public string? City { get; set; }

    public int? TotalFloors { get; set; }

    public int? TotalRooms { get; set; }

    public string? Description { get; set; }

    public bool IsSetupCompleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Floor> Floors { get; set; } = new List<Floor>();

    public virtual User Landlord { get; set; } = null!;

    public virtual ICollection<Property> Properties { get; set; } = new List<Property>();
}
