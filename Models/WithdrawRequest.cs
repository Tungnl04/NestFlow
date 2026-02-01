using System;
using System.Collections.Generic;

namespace NestFlow.Models;

public partial class WithdrawRequest
{
    public long WithdrawId { get; set; }

    public long WalletId { get; set; }

    public long LandlordId { get; set; }

    public decimal Amount { get; set; }

    public string BankName { get; set; } = null!;

    public string BankAccount { get; set; } = null!;

    public string AccountHolder { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime RequestedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public long? ProcessedByAdminId { get; set; }

    public string? Note { get; set; }

    public virtual User Landlord { get; set; } = null!;

    public virtual User? ProcessedByAdmin { get; set; }

    public virtual Wallet Wallet { get; set; } = null!;
}
