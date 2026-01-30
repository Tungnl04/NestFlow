using System;
using System.Collections.Generic;

namespace NestFlow.Models;

public partial class Review
{
    public long ReviewId { get; set; }

    public long PropertyId { get; set; }

    public long UserId { get; set; }

    public int? Rating { get; set; }

    public string? Comment { get; set; }

    public string? Images { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Property Property { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
