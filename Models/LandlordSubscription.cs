using System;
using System.Collections.Generic;

namespace NestFlow.Models;

public partial class LandlordSubscription
{
    public long SubscriptionId { get; set; }

    public long LandlordId { get; set; }

    public long PlanId { get; set; }

    public DateTime StartAt { get; set; }

    public DateTime EndAt { get; set; }

    public string Status { get; set; } = null!;

    public int QuotaRemaining { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User Landlord { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Plan Plan { get; set; } = null!;
}
