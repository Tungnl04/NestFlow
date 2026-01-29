using System;
using System.Collections.Generic;

namespace NestFlow.Models;

public partial class Message
{
    public long MessageId { get; set; }

    public long SenderId { get; set; }

    public long ReceiverId { get; set; }

    public long? PropertyId { get; set; }

    public string Content { get; set; } = null!;

    public bool? IsRead { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Property? Property { get; set; }

    public virtual User Receiver { get; set; } = null!;

    public virtual User Sender { get; set; } = null!;
}
