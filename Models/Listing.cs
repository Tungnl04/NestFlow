using System;
using System.Collections.Generic;

namespace NestFlow.Models;

public partial class Listing
{
    public long ListingId { get; set; }

    public long PropertyId { get; set; }

    public long LandlordId { get; set; }

    public string Title { get; set; } = null!;

    public string? Content { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? PublishedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public int ViewCount { get; set; }

    public int LikeCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User Landlord { get; set; } = null!;

    public virtual ICollection<ListingFavorite> ListingFavorites { get; set; } = new List<ListingFavorite>();

    public virtual Property Property { get; set; } = null!;
}
