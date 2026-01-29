using System;
using System.Collections.Generic;

namespace NestFlow.Models;

public partial class PropertyImage
{
    public long ImageId { get; set; }

    public long PropertyId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public bool? IsPrimary { get; set; }

    public int? DisplayOrder { get; set; }

    public DateTime? UploadedAt { get; set; }

    public virtual Property Property { get; set; } = null!;
}
