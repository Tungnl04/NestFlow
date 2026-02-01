using System;
using System.Collections.Generic;

namespace NestFlow.Models;

public partial class Payment
{
    public long PaymentId { get; set; }

    public long PayerUserId { get; set; }

    public long? LandlordId { get; set; }

    public long? RentalId { get; set; }

    public long? InvoiceId { get; set; }

    public long? SubscriptionId { get; set; }

    public string PaymentType { get; set; } = null!;

    public decimal Amount { get; set; }

    public string Provider { get; set; } = null!;

    public string? ProviderOrderCode { get; set; }

    public string? PayUrl { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? PaidAt { get; set; }

    public string? RawWebhook { get; set; }

    public virtual Invoice? Invoice { get; set; }

    public virtual User? Landlord { get; set; }

    public virtual User PayerUser { get; set; } = null!;

    public virtual ICollection<RentSchedule> RentSchedules { get; set; } = new List<RentSchedule>();

    public virtual Rental? Rental { get; set; }

    public virtual LandlordSubscription? Subscription { get; set; }
}
