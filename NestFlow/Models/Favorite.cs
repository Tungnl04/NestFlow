using System;
using System.Collections.Generic;

namespace NestFlow.Models;

public partial class Favorite
{
    public long FavoriteId { get; set; }

    public long UserId { get; set; }

    public long PropertyId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Property Property { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
