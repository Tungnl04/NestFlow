using System;
using System.Collections.Generic;

namespace NestFlow.Models;

public partial class Booking
{
    public long BookingId { get; set; }

    public long PropertyId { get; set; }

    public long RenterId { get; set; }

    public DateOnly BookingDate { get; set; }

    public TimeOnly BookingTime { get; set; }

    public string? Status { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Property Property { get; set; } = null!;

    public virtual User Renter { get; set; } = null!;
}
