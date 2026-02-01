using System;
using System.Collections.Generic;

namespace NestFlow.Models;

public partial class RentalOccupant
{
    public long OccupantId { get; set; }

    public long RentalId { get; set; }

    public long? UserId { get; set; }

    public string? FullName { get; set; }

    public string? Phone { get; set; }

    public string? IdNumber { get; set; }

    public DateOnly? MoveInDate { get; set; }

    public DateOnly? MoveOutExpected { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Rental Rental { get; set; } = null!;

    public virtual User? User { get; set; }
}
