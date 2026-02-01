using System;
using System.Collections.Generic;

namespace NestFlow.Models;

public partial class Wallet
{
    public long WalletId { get; set; }

    public long LandlordId { get; set; }

    public decimal LockedBalance { get; set; }

    public decimal AvailableBalance { get; set; }

    public string Currency { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User Landlord { get; set; } = null!;

    public virtual ICollection<WalletTransaction> WalletTransactions { get; set; } = new List<WalletTransaction>();

    public virtual ICollection<WithdrawRequest> WithdrawRequests { get; set; } = new List<WithdrawRequest>();
}
