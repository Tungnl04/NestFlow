using System;
using System.Collections.Generic;

namespace NestFlow.Models;

public partial class RentSchedule
{
    public long ScheduleId { get; set; }

    public long RentalId { get; set; }

    public string PeriodMonth { get; set; } = null!;

    public DateOnly DueDate { get; set; }

    public decimal Amount { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? PaidAt { get; set; }

    public long? PaymentId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Payment? Payment { get; set; }

    public virtual Rental Rental { get; set; } = null!;
}
