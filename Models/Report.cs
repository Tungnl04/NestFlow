using System;
using System.Collections.Generic;

namespace NestFlow.Models;

public partial class Report
{
    public long ReportId { get; set; }

    public long PropertyId { get; set; }

    public long ReporterId { get; set; }

    public string? Reason { get; set; }

    public string? Description { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public virtual Property Property { get; set; } = null!;

    public virtual User Reporter { get; set; } = null!;
}
