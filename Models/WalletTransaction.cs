using System;
using System.Collections.Generic;

namespace NestFlow.Models;

public partial class WalletTransaction
{
    public long WalletTxnId { get; set; }

    public long WalletId { get; set; }

    public string Direction { get; set; } = null!;

    public decimal Amount { get; set; }

    public string RelatedType { get; set; } = null!;

    public long? RelatedId { get; set; }

    public string Status { get; set; } = null!;

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Wallet Wallet { get; set; } = null!;
}
