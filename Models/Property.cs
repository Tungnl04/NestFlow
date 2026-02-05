using System;
using System.Collections.Generic;

namespace NestFlow.Models;

public partial class Property
{
    public long PropertyId { get; set; }

    public long LandlordId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string PropertyType { get; set; } = null!;

    public string? Address { get; set; }

    public string? Ward { get; set; }

    public string? District { get; set; }

    public string? City { get; set; }

    public decimal? Area { get; set; }

    public decimal? Price { get; set; }

    public decimal? Deposit { get; set; }

    public int? MaxOccupants { get; set; }

    public DateOnly? AvailableFrom { get; set; }

    public string? Status { get; set; }

    public int? ViewCount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public decimal? CommissionRate { get; set; }

    public decimal? UserDiscount { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual User Landlord { get; set; } = null!;

    public virtual ICollection<Listing> Listings { get; set; } = new List<Listing>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<PropertyImage> PropertyImages { get; set; } = new List<PropertyImage>();

    public virtual ICollection<Rental> Rentals { get; set; } = new List<Rental>();

    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<Amenity> Amenities { get; set; } = new List<Amenity>();
}
