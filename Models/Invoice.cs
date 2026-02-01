using System;
using System.Collections.Generic;

namespace NestFlow.Models;

public partial class Invoice
{
    public long InvoiceId { get; set; }

    public long RentalId { get; set; }

    public string? InvoiceMonth { get; set; }

    public decimal? RoomRent { get; set; }

    public int? ElectricOldReading { get; set; }

    public int? ElectricNewReading { get; set; }

    public int? ElectricUsage { get; set; }

    public decimal? ElectricAmount { get; set; }

    public int? WaterOldReading { get; set; }

    public int? WaterNewReading { get; set; }

    public int? WaterUsage { get; set; }

    public decimal? WaterAmount { get; set; }

    public decimal? InternetFee { get; set; }

    public string? OtherFees { get; set; }

    public decimal? TotalAmount { get; set; }

    public DateOnly? DueDate { get; set; }

    public DateOnly? PaymentDate { get; set; }

    public string? PaymentMethod { get; set; }

    public string? PaymentProofUrl { get; set; }

    public string? Status { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Rental Rental { get; set; } = null!;
}
