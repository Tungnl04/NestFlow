using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace NestFlow.Models;

public partial class NestFlowSystemContext : DbContext
{
    public NestFlowSystemContext()
    {
    }

    public NestFlowSystemContext(DbContextOptions<NestFlowSystemContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Amenity> Amenities { get; set; }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<Favorite> Favorites { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<LandlordSubscription> LandlordSubscriptions { get; set; }

    public virtual DbSet<Listing> Listings { get; set; }

    public virtual DbSet<ListingFavorite> ListingFavorites { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Plan> Plans { get; set; }

    public virtual DbSet<Property> Properties { get; set; }

    public virtual DbSet<PropertyImage> PropertyImages { get; set; }

    public virtual DbSet<RentSchedule> RentSchedules { get; set; }

    public virtual DbSet<Rental> Rentals { get; set; }

    public virtual DbSet<RentalOccupant> RentalOccupants { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Wallet> Wallets { get; set; }

    public virtual DbSet<WalletTransaction> WalletTransactions { get; set; }

    public virtual DbSet<WithdrawRequest> WithdrawRequests { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=TungLaptop;uid=sa;password=sa;database=NestFlowSystem;Encrypt=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Amenity>(entity =>
        {
            entity.HasKey(e => e.AmenityId).HasName("PK__Amenitie__E908452D5F4C85D8");

            entity.HasIndex(e => e.Name, "UQ__Amenitie__72E12F1BFC3CE0F2").IsUnique();

            entity.Property(e => e.AmenityId).HasColumnName("amenity_id");
            entity.Property(e => e.Category)
                .HasMaxLength(20)
                .HasColumnName("category");
            entity.Property(e => e.IconUrl)
                .HasMaxLength(500)
                .HasColumnName("icon_url");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.BookingId).HasName("PK__Bookings__5DE3A5B1165E52E7");

            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.BookingDate).HasColumnName("booking_date");
            entity.Property(e => e.BookingTime).HasColumnName("booking_time");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.PropertyId).HasColumnName("property_id");
            entity.Property(e => e.RenterId).HasColumnName("renter_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("pending")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Property).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.PropertyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_bookings_property");

            entity.HasOne(d => d.Renter).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.RenterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_bookings_renter");
        });

        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.HasKey(e => e.FavoriteId).HasName("PK__Favorite__46ACF4CBF5086EC0");

            entity.HasIndex(e => new { e.UserId, e.PropertyId }, "uq_favorites").IsUnique();

            entity.Property(e => e.FavoriteId).HasColumnName("favorite_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.PropertyId).HasColumnName("property_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Property).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.PropertyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_favorites_property");

            entity.HasOne(d => d.User).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_favorites_user");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.InvoiceId).HasName("PK__Invoices__F58DFD49314AAE46");

            entity.Property(e => e.InvoiceId).HasColumnName("invoice_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DueDate).HasColumnName("due_date");
            entity.Property(e => e.ElectricAmount)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("electric_amount");
            entity.Property(e => e.ElectricNewReading).HasColumnName("electric_new_reading");
            entity.Property(e => e.ElectricOldReading).HasColumnName("electric_old_reading");
            entity.Property(e => e.ElectricUsage).HasColumnName("electric_usage");
            entity.Property(e => e.InternetFee)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("internet_fee");
            entity.Property(e => e.InvoiceMonth)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("invoice_month");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.OtherFees).HasColumnName("other_fees");
            entity.Property(e => e.PaymentDate).HasColumnName("payment_date");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(30)
                .HasColumnName("payment_method");
            entity.Property(e => e.PaymentProofUrl)
                .HasMaxLength(500)
                .HasColumnName("payment_proof_url");
            entity.Property(e => e.RentalId).HasColumnName("rental_id");
            entity.Property(e => e.RoomRent)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("room_rent");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("pending")
                .HasColumnName("status");
            entity.Property(e => e.TotalAmount)
                .HasColumnType("decimal(14, 2)")
                .HasColumnName("total_amount");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.WaterAmount)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("water_amount");
            entity.Property(e => e.WaterNewReading).HasColumnName("water_new_reading");
            entity.Property(e => e.WaterOldReading).HasColumnName("water_old_reading");
            entity.Property(e => e.WaterUsage).HasColumnName("water_usage");

            entity.HasOne(d => d.Rental).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.RentalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_invoices_rental");
        });

        modelBuilder.Entity<LandlordSubscription>(entity =>
        {
            entity.HasKey(e => e.SubscriptionId).HasName("PK__Landlord__863A7EC1D068B802");

            entity.HasIndex(e => e.LandlordId, "ix_subscriptions_landlord");

            entity.HasIndex(e => e.Status, "ix_subscriptions_status");

            entity.Property(e => e.SubscriptionId).HasColumnName("subscription_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.EndAt)
                .HasColumnType("datetime")
                .HasColumnName("end_at");
            entity.Property(e => e.LandlordId).HasColumnName("landlord_id");
            entity.Property(e => e.PlanId).HasColumnName("plan_id");
            entity.Property(e => e.QuotaRemaining).HasColumnName("quota_remaining");
            entity.Property(e => e.StartAt)
                .HasColumnType("datetime")
                .HasColumnName("start_at");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("active")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Landlord).WithMany(p => p.LandlordSubscriptions)
                .HasForeignKey(d => d.LandlordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_subscriptions_landlord");

            entity.HasOne(d => d.Plan).WithMany(p => p.LandlordSubscriptions)
                .HasForeignKey(d => d.PlanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_subscriptions_plan");
        });

        modelBuilder.Entity<Listing>(entity =>
        {
            entity.HasKey(e => e.ListingId).HasName("PK__Listings__89D81774EF122552");

            entity.HasIndex(e => e.LandlordId, "ix_listings_landlord");

            entity.HasIndex(e => e.PropertyId, "ix_listings_property");

            entity.HasIndex(e => e.Status, "ix_listings_status");

            entity.Property(e => e.ListingId).HasColumnName("listing_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.ExpiresAt)
                .HasColumnType("datetime")
                .HasColumnName("expires_at");
            entity.Property(e => e.LandlordId).HasColumnName("landlord_id");
            entity.Property(e => e.LikeCount).HasColumnName("like_count");
            entity.Property(e => e.PropertyId).HasColumnName("property_id");
            entity.Property(e => e.PublishedAt)
                .HasColumnType("datetime")
                .HasColumnName("published_at");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("draft")
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.ViewCount).HasColumnName("view_count");

            entity.HasOne(d => d.Landlord).WithMany(p => p.Listings)
                .HasForeignKey(d => d.LandlordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_listings_landlord");

            entity.HasOne(d => d.Property).WithMany(p => p.Listings)
                .HasForeignKey(d => d.PropertyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_listings_property");
        });

        modelBuilder.Entity<ListingFavorite>(entity =>
        {
            entity.HasKey(e => e.ListingFavoriteId).HasName("PK__ListingF__55A0774524E8A0C1");

            entity.HasIndex(e => e.RenterId, "ix_listingfavorites_renter");

            entity.HasIndex(e => new { e.ListingId, e.RenterId }, "uq_listingfavorites").IsUnique();

            entity.Property(e => e.ListingFavoriteId).HasColumnName("listing_favorite_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.ListingId).HasColumnName("listing_id");
            entity.Property(e => e.RenterId).HasColumnName("renter_id");

            entity.HasOne(d => d.Listing).WithMany(p => p.ListingFavorites)
                .HasForeignKey(d => d.ListingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_listingfavorites_listing");

            entity.HasOne(d => d.Renter).WithMany(p => p.ListingFavorites)
                .HasForeignKey(d => d.RenterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_listingfavorites_renter");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("PK__Messages__0BBF6EE64A3BF7CF");

            entity.Property(e => e.MessageId).HasColumnName("message_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.IsRead)
                .HasDefaultValue(false)
                .HasColumnName("is_read");
            entity.Property(e => e.PropertyId).HasColumnName("property_id");
            entity.Property(e => e.ReceiverId).HasColumnName("receiver_id");
            entity.Property(e => e.SenderId).HasColumnName("sender_id");

            entity.HasOne(d => d.Property).WithMany(p => p.Messages)
                .HasForeignKey(d => d.PropertyId)
                .HasConstraintName("fk_messages_property");

            entity.HasOne(d => d.Receiver).WithMany(p => p.MessageReceivers)
                .HasForeignKey(d => d.ReceiverId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_messages_receiver");

            entity.HasOne(d => d.Sender).WithMany(p => p.MessageSenders)
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_messages_sender");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__E059842FD5297161");

            entity.Property(e => e.NotificationId).HasColumnName("notification_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.IsRead)
                .HasDefaultValue(false)
                .HasColumnName("is_read");
            entity.Property(e => e.LinkUrl)
                .HasMaxLength(500)
                .HasColumnName("link_url");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.Type)
                .HasMaxLength(20)
                .HasColumnName("type");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_notifications_user");
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Password__3214EC070066BFCA");

            entity.HasIndex(e => e.ExpiresAt, "IX_PasswordResetTokens_ExpiresAt");

            entity.HasIndex(e => e.Token, "IX_PasswordResetTokens_Token");

            entity.HasIndex(e => e.UserId, "IX_PasswordResetTokens_UserId");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Token).HasMaxLength(10);

            entity.HasOne(d => d.User).WithMany(p => p.PasswordResetTokens)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_PasswordResetTokens_Users");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payments__ED1FC9EACC3DBF5D");

            entity.HasIndex(e => e.PayerUserId, "ix_payments_payer");

            entity.HasIndex(e => e.ProviderOrderCode, "ix_payments_provider_order_code");

            entity.HasIndex(e => new { e.PaymentType, e.Status }, "ix_payments_type_status");

            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(14, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.InvoiceId).HasColumnName("invoice_id");
            entity.Property(e => e.LandlordId).HasColumnName("landlord_id");
            entity.Property(e => e.PaidAt)
                .HasColumnType("datetime")
                .HasColumnName("paid_at");
            entity.Property(e => e.PayUrl)
                .HasMaxLength(1000)
                .HasColumnName("pay_url");
            entity.Property(e => e.PayerUserId).HasColumnName("payer_user_id");
            entity.Property(e => e.PaymentType)
                .HasMaxLength(20)
                .HasColumnName("payment_type");
            entity.Property(e => e.Provider)
                .HasMaxLength(20)
                .HasDefaultValue("payos")
                .HasColumnName("provider");
            entity.Property(e => e.ProviderOrderCode)
                .HasMaxLength(100)
                .HasColumnName("provider_order_code");
            entity.Property(e => e.RawWebhook).HasColumnName("raw_webhook");
            entity.Property(e => e.RentalId).HasColumnName("rental_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("created")
                .HasColumnName("status");
            entity.Property(e => e.SubscriptionId).HasColumnName("subscription_id");

            entity.HasOne(d => d.Invoice).WithMany(p => p.Payments)
                .HasForeignKey(d => d.InvoiceId)
                .HasConstraintName("fk_payments_invoice");

            entity.HasOne(d => d.Landlord).WithMany(p => p.PaymentLandlords)
                .HasForeignKey(d => d.LandlordId)
                .HasConstraintName("fk_payments_landlord");

            entity.HasOne(d => d.PayerUser).WithMany(p => p.PaymentPayerUsers)
                .HasForeignKey(d => d.PayerUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_payments_payer");

            entity.HasOne(d => d.Rental).WithMany(p => p.Payments)
                .HasForeignKey(d => d.RentalId)
                .HasConstraintName("fk_payments_rental");

            entity.HasOne(d => d.Subscription).WithMany(p => p.Payments)
                .HasForeignKey(d => d.SubscriptionId)
                .HasConstraintName("fk_payments_subscription");
        });

        modelBuilder.Entity<Plan>(entity =>
        {
            entity.HasKey(e => e.PlanId).HasName("PK__Plans__BE9F8F1DDAD13903");

            entity.Property(e => e.PlanId).HasColumnName("plan_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DurationDays).HasColumnName("duration_days");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.PlanName)
                .HasMaxLength(100)
                .HasColumnName("plan_name");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("price");
            entity.Property(e => e.PriorityLevel).HasColumnName("priority_level");
            entity.Property(e => e.QuotaActiveListings)
                .HasDefaultValue(1)
                .HasColumnName("quota_active_listings");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<Property>(entity =>
        {
            entity.HasKey(e => e.PropertyId).HasName("PK__Properti__735BA463438E9ED8");

            entity.Property(e => e.PropertyId).HasColumnName("property_id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.Area)
                .HasColumnType("decimal(6, 2)")
                .HasColumnName("area");
            entity.Property(e => e.AvailableFrom).HasColumnName("available_from");
            entity.Property(e => e.City)
                .HasMaxLength(100)
                .HasColumnName("city");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Deposit)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("deposit");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.District)
                .HasMaxLength(100)
                .HasColumnName("district");
            entity.Property(e => e.LandlordId).HasColumnName("landlord_id");
            entity.Property(e => e.MaxOccupants).HasColumnName("max_occupants");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("price");
            entity.Property(e => e.PropertyType)
                .HasMaxLength(30)
                .HasColumnName("property_type");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("available")
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.ViewCount)
                .HasDefaultValue(0)
                .HasColumnName("view_count");
            entity.Property(e => e.Ward)
                .HasMaxLength(100)
                .HasColumnName("ward");

            entity.HasOne(d => d.Landlord).WithMany(p => p.Properties)
                .HasForeignKey(d => d.LandlordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_properties_landlord");

            entity.HasMany(d => d.Amenities).WithMany(p => p.Properties)
                .UsingEntity<Dictionary<string, object>>(
                    "PropertyAmenity",
                    r => r.HasOne<Amenity>().WithMany()
                        .HasForeignKey("AmenityId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("fk_pa_amenity"),
                    l => l.HasOne<Property>().WithMany()
                        .HasForeignKey("PropertyId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("fk_pa_property"),
                    j =>
                    {
                        j.HasKey("PropertyId", "AmenityId").HasName("pk_property_amenities");
                        j.ToTable("PropertyAmenities");
                        j.IndexerProperty<long>("PropertyId").HasColumnName("property_id");
                        j.IndexerProperty<long>("AmenityId").HasColumnName("amenity_id");
                    });
        });

        modelBuilder.Entity<PropertyImage>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("PK__Property__DC9AC9558EEAEA5A");

            entity.Property(e => e.ImageId).HasColumnName("image_id");
            entity.Property(e => e.DisplayOrder)
                .HasDefaultValue(0)
                .HasColumnName("display_order");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .HasColumnName("image_url");
            entity.Property(e => e.IsPrimary)
                .HasDefaultValue(false)
                .HasColumnName("is_primary");
            entity.Property(e => e.PropertyId).HasColumnName("property_id");
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("uploaded_at");

            entity.HasOne(d => d.Property).WithMany(p => p.PropertyImages)
                .HasForeignKey(d => d.PropertyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_images_property");
        });

        modelBuilder.Entity<RentSchedule>(entity =>
        {
            entity.HasKey(e => e.ScheduleId).HasName("PK__RentSche__C46A8A6F1FDFE5B5");

            entity.HasIndex(e => e.Status, "ix_rentschedules_status");

            entity.HasIndex(e => new { e.RentalId, e.PeriodMonth }, "uq_rentschedules").IsUnique();

            entity.Property(e => e.ScheduleId).HasColumnName("schedule_id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(14, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DueDate).HasColumnName("due_date");
            entity.Property(e => e.PaidAt)
                .HasColumnType("datetime")
                .HasColumnName("paid_at");
            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.PeriodMonth)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("period_month");
            entity.Property(e => e.RentalId).HasColumnName("rental_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("pending")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Payment).WithMany(p => p.RentSchedules)
                .HasForeignKey(d => d.PaymentId)
                .HasConstraintName("fk_rentschedules_payment");

            entity.HasOne(d => d.Rental).WithMany(p => p.RentSchedules)
                .HasForeignKey(d => d.RentalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_rentschedules_rental");
        });

        modelBuilder.Entity<Rental>(entity =>
        {
            entity.HasKey(e => e.RentalId).HasName("PK__Rentals__67DB611BA22B0456");

            entity.Property(e => e.RentalId).HasColumnName("rental_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DepositAmount)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("deposit_amount");
            entity.Property(e => e.ElectricPrice)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("electric_price");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.InternetFee)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("internet_fee");
            entity.Property(e => e.LandlordId).HasColumnName("landlord_id");
            entity.Property(e => e.MonthlyRent)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("monthly_rent");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.OtherFees).HasColumnName("other_fees");
            entity.Property(e => e.PaymentDueDate).HasColumnName("payment_due_date");
            entity.Property(e => e.PropertyId).HasColumnName("property_id");
            entity.Property(e => e.RenterId).HasColumnName("renter_id");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("active")
                .HasColumnName("status");
            entity.Property(e => e.TerminationDate).HasColumnName("termination_date");
            entity.Property(e => e.TerminationReason).HasColumnName("termination_reason");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.WaterPrice)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("water_price");

            entity.HasOne(d => d.Landlord).WithMany(p => p.RentalLandlords)
                .HasForeignKey(d => d.LandlordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_rentals_landlord");

            entity.HasOne(d => d.Property).WithMany(p => p.Rentals)
                .HasForeignKey(d => d.PropertyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_rentals_property");

            entity.HasOne(d => d.Renter).WithMany(p => p.RentalRenters)
                .HasForeignKey(d => d.RenterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_rentals_renter");
        });

        modelBuilder.Entity<RentalOccupant>(entity =>
        {
            entity.HasKey(e => e.OccupantId).HasName("PK__RentalOc__6F2B18EB1B4516F1");

            entity.HasIndex(e => e.RentalId, "ix_occupants_rental");

            entity.Property(e => e.OccupantId).HasColumnName("occupant_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .HasColumnName("full_name");
            entity.Property(e => e.IdNumber)
                .HasMaxLength(50)
                .HasColumnName("id_number");
            entity.Property(e => e.MoveInDate).HasColumnName("move_in_date");
            entity.Property(e => e.MoveOutExpected).HasColumnName("move_out_expected");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.RentalId).HasColumnName("rental_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("active")
                .HasColumnName("status");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Rental).WithMany(p => p.RentalOccupants)
                .HasForeignKey(d => d.RentalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_occupants_rental");

            entity.HasOne(d => d.User).WithMany(p => p.RentalOccupants)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("fk_occupants_user");
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("PK__Reports__779B7C5801B35724");

            entity.Property(e => e.ReportId).HasColumnName("report_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.PropertyId).HasColumnName("property_id");
            entity.Property(e => e.Reason)
                .HasMaxLength(255)
                .HasColumnName("reason");
            entity.Property(e => e.ReporterId).HasColumnName("reporter_id");
            entity.Property(e => e.ResolvedAt)
                .HasColumnType("datetime")
                .HasColumnName("resolved_at");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("pending")
                .HasColumnName("status");

            entity.HasOne(d => d.Property).WithMany(p => p.Reports)
                .HasForeignKey(d => d.PropertyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_reports_property");

            entity.HasOne(d => d.Reporter).WithMany(p => p.Reports)
                .HasForeignKey(d => d.ReporterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_reports_user");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__Reviews__60883D904EBD2212");

            entity.Property(e => e.ReviewId).HasColumnName("review_id");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Images).HasColumnName("images");
            entity.Property(e => e.PropertyId).HasColumnName("property_id");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Property).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.PropertyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_reviews_property");

            entity.HasOne(d => d.User).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_reviews_user");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__B9BE370F1D9A0ABF");

            entity.HasIndex(e => e.Email, "UQ__Users__AB6E616490D4A6DA").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.AvatarUrl)
                .HasMaxLength(500)
                .HasColumnName("avatar_url");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .HasColumnName("full_name");
            entity.Property(e => e.IsVerified)
                .HasDefaultValue(false)
                .HasColumnName("is_verified");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("active")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserType)
                .HasMaxLength(20)
                .HasColumnName("user_type");
        });

        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.HasKey(e => e.WalletId).HasName("PK__Wallets__0EE6F041F22D1DFE");

            entity.HasIndex(e => e.LandlordId, "UQ__Wallets__0AA6026FDE85EA05").IsUnique();

            entity.Property(e => e.WalletId).HasColumnName("wallet_id");
            entity.Property(e => e.AvailableBalance)
                .HasColumnType("decimal(14, 2)")
                .HasColumnName("available_balance");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Currency)
                .HasMaxLength(10)
                .HasDefaultValue("VND")
                .HasColumnName("currency");
            entity.Property(e => e.LandlordId).HasColumnName("landlord_id");
            entity.Property(e => e.LockedBalance)
                .HasColumnType("decimal(14, 2)")
                .HasColumnName("locked_balance");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Landlord).WithOne(p => p.Wallet)
                .HasForeignKey<Wallet>(d => d.LandlordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_wallets_landlord");
        });

        modelBuilder.Entity<WalletTransaction>(entity =>
        {
            entity.HasKey(e => e.WalletTxnId).HasName("PK__WalletTr__3C5FF6AC57126483");

            entity.HasIndex(e => new { e.RelatedType, e.RelatedId }, "ix_wallettx_related");

            entity.HasIndex(e => e.WalletId, "ix_wallettx_wallet");

            entity.Property(e => e.WalletTxnId).HasColumnName("wallet_txn_id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(14, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Direction)
                .HasMaxLength(10)
                .HasColumnName("direction");
            entity.Property(e => e.Note)
                .HasMaxLength(500)
                .HasColumnName("note");
            entity.Property(e => e.RelatedId).HasColumnName("related_id");
            entity.Property(e => e.RelatedType)
                .HasMaxLength(20)
                .HasColumnName("related_type");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("posted")
                .HasColumnName("status");
            entity.Property(e => e.WalletId).HasColumnName("wallet_id");

            entity.HasOne(d => d.Wallet).WithMany(p => p.WalletTransactions)
                .HasForeignKey(d => d.WalletId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_wallettx_wallet");
        });

        modelBuilder.Entity<WithdrawRequest>(entity =>
        {
            entity.HasKey(e => e.WithdrawId).HasName("PK__Withdraw__2F1C7929664B9018");

            entity.HasIndex(e => e.LandlordId, "ix_withdraw_landlord");

            entity.HasIndex(e => e.Status, "ix_withdraw_status");

            entity.Property(e => e.WithdrawId).HasColumnName("withdraw_id");
            entity.Property(e => e.AccountHolder)
                .HasMaxLength(100)
                .HasColumnName("account_holder");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(14, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.BankAccount)
                .HasMaxLength(50)
                .HasColumnName("bank_account");
            entity.Property(e => e.BankName)
                .HasMaxLength(100)
                .HasColumnName("bank_name");
            entity.Property(e => e.LandlordId).HasColumnName("landlord_id");
            entity.Property(e => e.Note)
                .HasMaxLength(500)
                .HasColumnName("note");
            entity.Property(e => e.ProcessedAt)
                .HasColumnType("datetime")
                .HasColumnName("processed_at");
            entity.Property(e => e.ProcessedByAdminId).HasColumnName("processed_by_admin_id");
            entity.Property(e => e.RequestedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("requested_at");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("pending")
                .HasColumnName("status");
            entity.Property(e => e.WalletId).HasColumnName("wallet_id");

            entity.HasOne(d => d.Landlord).WithMany(p => p.WithdrawRequestLandlords)
                .HasForeignKey(d => d.LandlordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_withdraw_landlord");

            entity.HasOne(d => d.ProcessedByAdmin).WithMany(p => p.WithdrawRequestProcessedByAdmins)
                .HasForeignKey(d => d.ProcessedByAdminId)
                .HasConstraintName("fk_withdraw_admin");

            entity.HasOne(d => d.Wallet).WithMany(p => p.WithdrawRequests)
                .HasForeignKey(d => d.WalletId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_withdraw_wallet");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
