-- =======================================================
-- ADDITIONAL TABLES FOR FULL PROJECT FLOW (NO CHANGES MADE)
-- Target DB: NestFlowSystem (SQL Server / T-SQL)
-- =======================================================
USE NestFlowSystem;
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

-- =======================================================
-- A) LISTINGS / POSTS (Landlord marketing posts)
-- =======================================================
IF OBJECT_ID('dbo.Listings','U') IS NULL
BEGIN
    CREATE TABLE dbo.Listings (
        listing_id BIGINT IDENTITY(1,1) PRIMARY KEY,
        property_id BIGINT NOT NULL,
        landlord_id BIGINT NOT NULL,

        title NVARCHAR(255) NOT NULL,
        content NVARCHAR(MAX) NULL,

        status NVARCHAR(20) NOT NULL DEFAULT 'draft'
            CHECK (status IN ('draft','active','inactive','expired')),

        published_at DATETIME NULL,
        expires_at DATETIME NULL,

        view_count INT NOT NULL DEFAULT 0,
        like_count INT NOT NULL DEFAULT 0,

        created_at DATETIME NOT NULL DEFAULT GETDATE(),
        updated_at DATETIME NOT NULL DEFAULT GETDATE(),

        CONSTRAINT fk_listings_property FOREIGN KEY (property_id)
            REFERENCES dbo.Properties(property_id),
        CONSTRAINT fk_listings_landlord FOREIGN KEY (landlord_id)
            REFERENCES dbo.Users(user_id)
    );

    CREATE INDEX ix_listings_landlord ON dbo.Listings(landlord_id);
    CREATE INDEX ix_listings_property ON dbo.Listings(property_id);
    CREATE INDEX ix_listings_status ON dbo.Listings(status);
END
GO

-- Optional: like per listing (if you want strict like_count from data)
IF OBJECT_ID('dbo.ListingFavorites','U') IS NULL
BEGIN
    CREATE TABLE dbo.ListingFavorites (
        listing_favorite_id BIGINT IDENTITY(1,1) PRIMARY KEY,
        listing_id BIGINT NOT NULL,
        renter_id BIGINT NOT NULL,
        created_at DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT uq_listingfavorites UNIQUE (listing_id, renter_id),
        CONSTRAINT fk_listingfavorites_listing FOREIGN KEY (listing_id)
            REFERENCES dbo.Listings(listing_id),
        CONSTRAINT fk_listingfavorites_renter FOREIGN KEY (renter_id)
            REFERENCES dbo.Users(user_id)
    );

    CREATE INDEX ix_listingfavorites_renter ON dbo.ListingFavorites(renter_id);
END
GO

-- =======================================================
-- B) SUBSCRIPTION PLANS (Posting packages)
-- =======================================================
IF OBJECT_ID('dbo.Plans','U') IS NULL
BEGIN
    CREATE TABLE dbo.Plans (
        plan_id BIGINT IDENTITY(1,1) PRIMARY KEY,
        plan_name NVARCHAR(100) NOT NULL,
        price DECIMAL(12,2) NOT NULL DEFAULT 0,
        duration_days INT NOT NULL,                  -- e.g., 30/90
        quota_active_listings INT NOT NULL DEFAULT 1, -- number of listings that can be active
        priority_level INT NOT NULL DEFAULT 0,        -- higher = more priority (optional)
        description NVARCHAR(MAX) NULL,
        is_active BIT NOT NULL DEFAULT 1,
        created_at DATETIME NOT NULL DEFAULT GETDATE(),
        updated_at DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

IF OBJECT_ID('dbo.LandlordSubscriptions','U') IS NULL
BEGIN
    CREATE TABLE dbo.LandlordSubscriptions (
        subscription_id BIGINT IDENTITY(1,1) PRIMARY KEY,
        landlord_id BIGINT NOT NULL,
        plan_id BIGINT NOT NULL,

        start_at DATETIME NOT NULL,
        end_at DATETIME NOT NULL,

        status NVARCHAR(20) NOT NULL DEFAULT 'active'
            CHECK (status IN ('active','expired','cancelled')),

        quota_remaining INT NOT NULL DEFAULT 0,

        created_at DATETIME NOT NULL DEFAULT GETDATE(),
        updated_at DATETIME NOT NULL DEFAULT GETDATE(),

        CONSTRAINT fk_subscriptions_landlord FOREIGN KEY (landlord_id)
            REFERENCES dbo.Users(user_id),
        CONSTRAINT fk_subscriptions_plan FOREIGN KEY (plan_id)
            REFERENCES dbo.Plans(plan_id)
    );

    CREATE INDEX ix_subscriptions_landlord ON dbo.LandlordSubscriptions(landlord_id);
    CREATE INDEX ix_subscriptions_status ON dbo.LandlordSubscriptions(status);
END
GO

-- =======================================================
-- C) PAYMENTS (PayOS tracking for deposit / invoice / subscription)
-- =======================================================
IF OBJECT_ID('dbo.Payments','U') IS NULL
BEGIN
    CREATE TABLE dbo.Payments (
        payment_id BIGINT IDENTITY(1,1) PRIMARY KEY,

        payer_user_id BIGINT NOT NULL,               -- who paid (usually renter; could be landlord for subscription)
        landlord_id BIGINT NULL,                     -- target landlord (optional)

        rental_id BIGINT NULL,
        invoice_id BIGINT NULL,
        subscription_id BIGINT NULL,

        payment_type NVARCHAR(20) NOT NULL
            CHECK (payment_type IN ('deposit','invoice','subscription')),

        amount DECIMAL(14,2) NOT NULL,

        provider NVARCHAR(20) NOT NULL DEFAULT 'payos'
            CHECK (provider IN ('payos')),

        provider_order_code NVARCHAR(100) NULL,      -- PayOS orderCode / reference
        pay_url NVARCHAR(1000) NULL,

        status NVARCHAR(20) NOT NULL DEFAULT 'created'
            CHECK (status IN ('created','paid','failed','cancelled')),

        created_at DATETIME NOT NULL DEFAULT GETDATE(),
        paid_at DATETIME NULL,
        raw_webhook NVARCHAR(MAX) NULL,              -- store webhook payload (optional)

        CONSTRAINT fk_payments_payer FOREIGN KEY (payer_user_id)
            REFERENCES dbo.Users(user_id),
        CONSTRAINT fk_payments_landlord FOREIGN KEY (landlord_id)
            REFERENCES dbo.Users(user_id),
        CONSTRAINT fk_payments_rental FOREIGN KEY (rental_id)
            REFERENCES dbo.Rentals(rental_id),
        CONSTRAINT fk_payments_invoice FOREIGN KEY (invoice_id)
            REFERENCES dbo.Invoices(invoice_id),
        CONSTRAINT fk_payments_subscription FOREIGN KEY (subscription_id)
            REFERENCES dbo.LandlordSubscriptions(subscription_id)
    );

    CREATE INDEX ix_payments_payer ON dbo.Payments(payer_user_id);
    CREATE INDEX ix_payments_type_status ON dbo.Payments(payment_type, status);
    CREATE INDEX ix_payments_provider_order_code ON dbo.Payments(provider_order_code);
END
GO

-- =======================================================
-- D) WALLET + WITHDRAW (Landlord)
-- =======================================================
IF OBJECT_ID('dbo.Wallets','U') IS NULL
BEGIN
    CREATE TABLE dbo.Wallets (
        wallet_id BIGINT IDENTITY(1,1) PRIMARY KEY,
        landlord_id BIGINT NOT NULL UNIQUE,

        locked_balance DECIMAL(14,2) NOT NULL DEFAULT 0,
        available_balance DECIMAL(14,2) NOT NULL DEFAULT 0,
        currency NVARCHAR(10) NOT NULL DEFAULT 'VND',

        created_at DATETIME NOT NULL DEFAULT GETDATE(),
        updated_at DATETIME NOT NULL DEFAULT GETDATE(),

        CONSTRAINT fk_wallets_landlord FOREIGN KEY (landlord_id)
            REFERENCES dbo.Users(user_id)
    );

	CREATE TABLE [dbo].[Wallets](
        [wallet_id] [bigint] IDENTITY(1,1) NOT NULL,
        [landlord_id] [bigint] NOT NULL,
        [locked_balance] [decimal](14, 2) NOT NULL DEFAULT 0,
        [available_balance] [decimal](14, 2) NOT NULL DEFAULT 0,
        [currency] [nvarchar](10) NOT NULL DEFAULT 'VND',
        [created_at] [datetime] NOT NULL DEFAULT (getdate()),
        [updated_at] [datetime] NOT NULL DEFAULT (getdate()),
        CONSTRAINT [PK__Wallets__3214EC07] PRIMARY KEY CLUSTERED ([wallet_id] ASC)
    )
END
GO

IF OBJECT_ID('dbo.WalletTransactions','U') IS NULL
BEGIN
    CREATE TABLE dbo.WalletTransactions (
        wallet_txn_id BIGINT IDENTITY(1,1) PRIMARY KEY,
        wallet_id BIGINT NOT NULL,

        direction NVARCHAR(10) NOT NULL CHECK (direction IN ('credit','debit')),
        amount DECIMAL(14,2) NOT NULL,

        related_type NVARCHAR(20) NOT NULL
            CHECK (related_type IN ('deposit','invoice','subscription','withdraw','adjustment')),
        related_id BIGINT NULL,                      -- id in related table (app-level meaning)

        status NVARCHAR(20) NOT NULL DEFAULT 'posted'
            CHECK (status IN ('posted','reversed')),

        note NVARCHAR(500) NULL,
        created_at DATETIME NOT NULL DEFAULT GETDATE(),

        CONSTRAINT fk_wallettx_wallet FOREIGN KEY (wallet_id)
            REFERENCES dbo.Wallets(wallet_id)
    );

    CREATE INDEX ix_wallettx_wallet ON dbo.WalletTransactions(wallet_id);
    CREATE INDEX ix_wallettx_related ON dbo.WalletTransactions(related_type, related_id);
END
GO

IF OBJECT_ID('dbo.WithdrawRequests','U') IS NULL
BEGIN
    CREATE TABLE dbo.WithdrawRequests (
        withdraw_id BIGINT IDENTITY(1,1) PRIMARY KEY,
        wallet_id BIGINT NOT NULL,
        landlord_id BIGINT NOT NULL,

        amount DECIMAL(14,2) NOT NULL,

        bank_name NVARCHAR(100) NOT NULL,
        bank_account NVARCHAR(50) NOT NULL,
        account_holder NVARCHAR(100) NOT NULL,

        status NVARCHAR(20) NOT NULL DEFAULT 'pending'
            CHECK (status IN ('pending','approved','rejected','completed')),

        requested_at DATETIME NOT NULL DEFAULT GETDATE(),
        processed_at DATETIME NULL,

        processed_by_admin_id BIGINT NULL,
        note NVARCHAR(500) NULL,

        CONSTRAINT fk_withdraw_wallet FOREIGN KEY (wallet_id)
            REFERENCES dbo.Wallets(wallet_id),
        CONSTRAINT fk_withdraw_landlord FOREIGN KEY (landlord_id)
            REFERENCES dbo.Users(user_id),
        CONSTRAINT fk_withdraw_admin FOREIGN KEY (processed_by_admin_id)
            REFERENCES dbo.Users(user_id)
    );

    CREATE INDEX ix_withdraw_status ON dbo.WithdrawRequests(status);
    CREATE INDEX ix_withdraw_landlord ON dbo.WithdrawRequests(landlord_id);
END
GO

-- =======================================================
-- E) OCCUPANTS + RENT SCHEDULE (Manage people and due dates)
-- =======================================================
IF OBJECT_ID('dbo.RentalOccupants','U') IS NULL
BEGIN
    CREATE TABLE dbo.RentalOccupants (
        occupant_id BIGINT IDENTITY(1,1) PRIMARY KEY,
        rental_id BIGINT NOT NULL,

        user_id BIGINT NULL,                         -- if occupant has an account
        full_name NVARCHAR(255) NULL,
        phone NVARCHAR(20) NULL,
        id_number NVARCHAR(50) NULL,

        move_in_date DATE NULL,
        move_out_expected DATE NULL,

        status NVARCHAR(20) NOT NULL DEFAULT 'active'
            CHECK (status IN ('active','left')),

        created_at DATETIME NOT NULL DEFAULT GETDATE(),

        CONSTRAINT fk_occupants_rental FOREIGN KEY (rental_id)
            REFERENCES dbo.Rentals(rental_id),
        CONSTRAINT fk_occupants_user FOREIGN KEY (user_id)
            REFERENCES dbo.Users(user_id)
    );

    CREATE INDEX ix_occupants_rental ON dbo.RentalOccupants(rental_id);
END
GO

IF OBJECT_ID('dbo.RentSchedules','U') IS NULL
BEGIN
    CREATE TABLE dbo.RentSchedules (
        schedule_id BIGINT IDENTITY(1,1) PRIMARY KEY,
        rental_id BIGINT NOT NULL,

        period_month CHAR(7) NOT NULL,               -- YYYY-MM
        due_date DATE NOT NULL,

        amount DECIMAL(14,2) NOT NULL,

        status NVARCHAR(20) NOT NULL DEFAULT 'pending'
            CHECK (status IN ('pending','paid','overdue','cancelled')),

        paid_at DATETIME NULL,
        payment_id BIGINT NULL,                      -- link to Payments if paid online

        created_at DATETIME NOT NULL DEFAULT GETDATE(),
        updated_at DATETIME NOT NULL DEFAULT GETDATE(),

        CONSTRAINT uq_rentschedules UNIQUE (rental_id, period_month),
        CONSTRAINT fk_rentschedules_rental FOREIGN KEY (rental_id)
            REFERENCES dbo.Rentals(rental_id),
        CONSTRAINT fk_rentschedules_payment FOREIGN KEY (payment_id)
            REFERENCES dbo.Payments(payment_id)
    );

    CREATE INDEX ix_rentschedules_status ON dbo.RentSchedules(status);
END
GO
