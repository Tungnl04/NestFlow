using System;
using System.Collections.Generic;

namespace NestFlow.Models;

public partial class Floor
{
    public long FloorId { get; set; }

    public long BuildingId { get; set; }

    public int FloorNumber { get; set; }

    public string FloorName { get; set; } = null!;

    public int RoomsCount { get; set; }

    public int DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Building Building { get; set; } = null!;

    public virtual ICollection<Property> Properties { get; set; } = new List<Property>();
}
