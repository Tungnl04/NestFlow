using System;
using System.Collections.Generic;

namespace NestFlow.Models;

public partial class Rental
{
    public long RentalId { get; set; }

    public long PropertyId { get; set; }

    public long LandlordId { get; set; }

    public long RenterId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public decimal? MonthlyRent { get; set; }

    public decimal? DepositAmount { get; set; }

    public int? PaymentDueDate { get; set; }

    public decimal? ElectricPrice { get; set; }

    public decimal? WaterPrice { get; set; }

    public decimal? InternetFee { get; set; }

    public string? OtherFees { get; set; }

    public string? Status { get; set; }

    public DateOnly? TerminationDate { get; set; }

    public string? TerminationReason { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual User Landlord { get; set; } = null!;

    public virtual Property Property { get; set; } = null!;

    public virtual User Renter { get; set; } = null!;
}
