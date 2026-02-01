using System;
using System.Collections.Generic;

namespace NestFlow.Models;

public partial class ListingFavorite
{
    public long ListingFavoriteId { get; set; }

    public long ListingId { get; set; }

    public long RenterId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Listing Listing { get; set; } = null!;

    public virtual User Renter { get; set; } = null!;
}
