using System;
using System.Collections.Generic;

namespace NestFlow.Models;

public partial class User
{
    public long UserId { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? FullName { get; set; }

    public string? Phone { get; set; }

    public string? AvatarUrl { get; set; }

    public string UserType { get; set; } = null!;

    public bool? IsVerified { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual ICollection<LandlordSubscription> LandlordSubscriptions { get; set; } = new List<LandlordSubscription>();

    public virtual ICollection<ListingFavorite> ListingFavorites { get; set; } = new List<ListingFavorite>();

    public virtual ICollection<Listing> Listings { get; set; } = new List<Listing>();

    public virtual ICollection<Message> MessageReceivers { get; set; } = new List<Message>();

    public virtual ICollection<Message> MessageSenders { get; set; } = new List<Message>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Payment> PaymentLandlords { get; set; } = new List<Payment>();

    public virtual ICollection<Payment> PaymentPayerUsers { get; set; } = new List<Payment>();

    public virtual ICollection<Property> Properties { get; set; } = new List<Property>();

    public virtual ICollection<Rental> RentalLandlords { get; set; } = new List<Rental>();

    public virtual ICollection<RentalOccupant> RentalOccupants { get; set; } = new List<RentalOccupant>();

    public virtual ICollection<Rental> RentalRenters { get; set; } = new List<Rental>();

    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual Wallet? Wallet { get; set; }

    public virtual ICollection<WithdrawRequest> WithdrawRequestLandlords { get; set; } = new List<WithdrawRequest>();

    public virtual ICollection<WithdrawRequest> WithdrawRequestProcessedByAdmins { get; set; } = new List<WithdrawRequest>();
}
