using System;
using System.Collections.Generic;

namespace NestFlow.Models;

public partial class Amenity
{
    public long AmenityId { get; set; }

    public string Name { get; set; } = null!;

    public string? IconUrl { get; set; }

    public string? Category { get; set; }

    public virtual ICollection<Property> Properties { get; set; } = new List<Property>();
}
