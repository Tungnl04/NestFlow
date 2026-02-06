-- =====================================================================
-- NestFlowSystem Complete Database Migration Script
-- Database: NestFlowSystem
-- Generated: 2026-02-06
-- Description: Complete database schema with all tables including new additions
-- =====================================================================

CREATE DATABASE NestFlowSystem

USE [NestFlowSystem]
GO

-- =====================================================================
-- SECTION 1: DROP EXISTING OBJECTS (IF EXISTS)
-- =====================================================================
PRINT N'Dropping existing foreign key constraints...';
GO

-- Drop all foreign key constraints
IF OBJECT_ID('dbo.FK_PasswordResetTokens_Users', 'F') IS NOT NULL
    ALTER TABLE [dbo].[PasswordResetTokens] DROP CONSTRAINT [FK_PasswordResetTokens_Users];
IF OBJECT_ID('dbo.fk_properties_landlord', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Properties] DROP CONSTRAINT [fk_properties_landlord];
IF OBJECT_ID('dbo.fk_images_property', 'F') IS NOT NULL
    ALTER TABLE [dbo].[PropertyImages] DROP CONSTRAINT [fk_images_property];
IF OBJECT_ID('dbo.fk_pa_property', 'F') IS NOT NULL
    ALTER TABLE [dbo].[PropertyAmenities] DROP CONSTRAINT [fk_pa_property];
IF OBJECT_ID('dbo.fk_pa_amenity', 'F') IS NOT NULL
    ALTER TABLE [dbo].[PropertyAmenities] DROP CONSTRAINT [fk_pa_amenity];
IF OBJECT_ID('dbo.fk_listings_property', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Listings] DROP CONSTRAINT [fk_listings_property];
IF OBJECT_ID('dbo.fk_listings_landlord', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Listings] DROP CONSTRAINT [fk_listings_landlord];
IF OBJECT_ID('dbo.fk_listingfavorites_listing', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ListingFavorites] DROP CONSTRAINT [fk_listingfavorites_listing];
IF OBJECT_ID('dbo.fk_listingfavorites_renter', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ListingFavorites] DROP CONSTRAINT [fk_listingfavorites_renter];
IF OBJECT_ID('dbo.fk_bookings_property', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Bookings] DROP CONSTRAINT [fk_bookings_property];
IF OBJECT_ID('dbo.fk_bookings_renter', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Bookings] DROP CONSTRAINT [fk_bookings_renter];
IF OBJECT_ID('dbo.fk_favorites_property', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Favorites] DROP CONSTRAINT [fk_favorites_property];
IF OBJECT_ID('dbo.fk_favorites_user', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Favorites] DROP CONSTRAINT [fk_favorites_user];
IF OBJECT_ID('dbo.fk_rentals_landlord', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Rentals] DROP CONSTRAINT [fk_rentals_landlord];
IF OBJECT_ID('dbo.fk_rentals_property', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Rentals] DROP CONSTRAINT [fk_rentals_property];
IF OBJECT_ID('dbo.fk_rentals_renter', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Rentals] DROP CONSTRAINT [fk_rentals_renter];
IF OBJECT_ID('dbo.fk_occupants_rental', 'F') IS NOT NULL
    ALTER TABLE [dbo].[RentalOccupants] DROP CONSTRAINT [fk_occupants_rental];
IF OBJECT_ID('dbo.fk_occupants_user', 'F') IS NOT NULL
    ALTER TABLE [dbo].[RentalOccupants] DROP CONSTRAINT [fk_occupants_user];
IF OBJECT_ID('dbo.fk_rentschedules_rental', 'F') IS NOT NULL
    ALTER TABLE [dbo].[RentSchedules] DROP CONSTRAINT [fk_rentschedules_rental];
IF OBJECT_ID('dbo.fk_rentschedules_payment', 'F') IS NOT NULL
    ALTER TABLE [dbo].[RentSchedules] DROP CONSTRAINT [fk_rentschedules_payment];
IF OBJECT_ID('dbo.fk_invoices_rental', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Invoices] DROP CONSTRAINT [fk_invoices_rental];
IF OBJECT_ID('dbo.fk_messages_property', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Messages] DROP CONSTRAINT [fk_messages_property];
IF OBJECT_ID('dbo.fk_messages_receiver', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Messages] DROP CONSTRAINT [fk_messages_receiver];
IF OBJECT_ID('dbo.fk_messages_sender', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Messages] DROP CONSTRAINT [fk_messages_sender];
IF OBJECT_ID('dbo.fk_notifications_user', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Notifications] DROP CONSTRAINT [fk_notifications_user];
IF OBJECT_ID('dbo.fk_subscriptions_landlord', 'F') IS NOT NULL
    ALTER TABLE [dbo].[LandlordSubscriptions] DROP CONSTRAINT [fk_subscriptions_landlord];
IF OBJECT_ID('dbo.fk_subscriptions_plan', 'F') IS NOT NULL
    ALTER TABLE [dbo].[LandlordSubscriptions] DROP CONSTRAINT [fk_subscriptions_plan];
IF OBJECT_ID('dbo.fk_payments_landlord', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Payments] DROP CONSTRAINT [fk_payments_landlord];
IF OBJECT_ID('dbo.fk_payments_payer', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Payments] DROP CONSTRAINT [fk_payments_payer];
IF OBJECT_ID('dbo.fk_payments_rental', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Payments] DROP CONSTRAINT [fk_payments_rental];
IF OBJECT_ID('dbo.fk_payments_invoice', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Payments] DROP CONSTRAINT [fk_payments_invoice];
IF OBJECT_ID('dbo.fk_payments_subscription', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Payments] DROP CONSTRAINT [fk_payments_subscription];
IF OBJECT_ID('dbo.fk_reports_property', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Reports] DROP CONSTRAINT [fk_reports_property];
IF OBJECT_ID('dbo.fk_reports_user', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Reports] DROP CONSTRAINT [fk_reports_user];
IF OBJECT_ID('dbo.fk_reviews_property', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Reviews] DROP CONSTRAINT [fk_reviews_property];
IF OBJECT_ID('dbo.fk_reviews_user', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Reviews] DROP CONSTRAINT [fk_reviews_user];
IF OBJECT_ID('dbo.fk_wallets_landlord', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Wallets] DROP CONSTRAINT [fk_wallets_landlord];
IF OBJECT_ID('dbo.fk_wallettxn_wallet', 'F') IS NOT NULL
    ALTER TABLE [dbo].[WalletTransactions] DROP CONSTRAINT [fk_wallettxn_wallet];
IF OBJECT_ID('dbo.fk_withdraw_admin', 'F') IS NOT NULL
    ALTER TABLE [dbo].[WithdrawRequests] DROP CONSTRAINT [fk_withdraw_admin];
IF OBJECT_ID('dbo.fk_withdraw_landlord', 'F') IS NOT NULL
    ALTER TABLE [dbo].[WithdrawRequests] DROP CONSTRAINT [fk_withdraw_landlord];
IF OBJECT_ID('dbo.fk_withdraw_wallet', 'F') IS NOT NULL
    ALTER TABLE [dbo].[WithdrawRequests] DROP CONSTRAINT [fk_withdraw_wallet];
GO

PRINT N'Dropping existing tables...';
GO

DROP TABLE IF EXISTS [dbo].[RentSchedules];
DROP TABLE IF EXISTS [dbo].[RentalOccupants];
DROP TABLE IF EXISTS [dbo].[WithdrawRequests];
DROP TABLE IF EXISTS [dbo].[WalletTransactions];
DROP TABLE IF EXISTS [dbo].[Wallets];
DROP TABLE IF EXISTS [dbo].[Reviews];
DROP TABLE IF EXISTS [dbo].[Reports];
DROP TABLE IF EXISTS [dbo].[Payments];
DROP TABLE IF EXISTS [dbo].[LandlordSubscriptions];
DROP TABLE IF EXISTS [dbo].[Plans];
DROP TABLE IF EXISTS [dbo].[Notifications];
DROP TABLE IF EXISTS [dbo].[Messages];
DROP TABLE IF EXISTS [dbo].[Invoices];
DROP TABLE IF EXISTS [dbo].[Rentals];
DROP TABLE IF EXISTS [dbo].[Favorites];
DROP TABLE IF EXISTS [dbo].[Bookings];
DROP TABLE IF EXISTS [dbo].[ListingFavorites];
DROP TABLE IF EXISTS [dbo].[Listings];
DROP TABLE IF EXISTS [dbo].[PropertyAmenities];
DROP TABLE IF EXISTS [dbo].[PropertyImages];
DROP TABLE IF EXISTS [dbo].[Properties];
DROP TABLE IF EXISTS [dbo].[Amenities];
DROP TABLE IF EXISTS [dbo].[PasswordResetTokens];
DROP TABLE IF EXISTS [dbo].[Users];
GO

-- =====================================================================
-- SECTION 2: CREATE TABLES (In Dependency Order)
-- =====================================================================
PRINT N'Creating base tables...';
GO

-- =====================================================================
-- Table: Users (No dependencies)
-- =====================================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Users](
    [user_id] [bigint] IDENTITY(1,1) NOT NULL,
    [email] [nvarchar](255) NOT NULL,
    [password_hash] [nvarchar](255) NOT NULL,
    [full_name] [nvarchar](255) NULL,
    [phone] [nvarchar](20) NULL,
    [avatar_url] [nvarchar](500) NULL,
    [user_type] [nvarchar](20) NOT NULL,
    [is_verified] [bit] NULL DEFAULT ((0)),
    [status] [nvarchar](20) NULL DEFAULT ('active'),
    [created_at] [datetime] NULL DEFAULT (getdate()),
    [updated_at] [datetime] NULL DEFAULT (getdate()),
    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([user_id] ASC),
    CONSTRAINT [UQ_Users_Email] UNIQUE ([email]),
    CONSTRAINT [CK_Users_Status] CHECK ([status]='banned' OR [status]='inactive' OR [status]='active'),
    CONSTRAINT [CK_Users_Type] CHECK ([user_type]='admin' OR [user_type]='landlord' OR [user_type]='renter')
) ON [PRIMARY]
GO

-- =====================================================================
-- Table: Amenities (No dependencies)
-- =====================================================================
CREATE TABLE [dbo].[Amenities](
    [amenity_id] [bigint] IDENTITY(1,1) NOT NULL,
    [name] [nvarchar](100) NOT NULL,
    [icon_url] [nvarchar](500) NULL,
    [category] [nvarchar](20) NULL,
    CONSTRAINT [PK_Amenities] PRIMARY KEY CLUSTERED ([amenity_id] ASC),
    CONSTRAINT [UQ_Amenities_Name] UNIQUE ([name]),
    CONSTRAINT [CK_Amenities_Category] CHECK ([category]='security' OR [category]='furniture' OR [category]='basic')
) ON [PRIMARY]
GO

-- =====================================================================
-- Table: Plans (No dependencies)
-- =====================================================================
CREATE TABLE [dbo].[Plans](
    [plan_id] [bigint] IDENTITY(1,1) NOT NULL,
    [plan_name] [nvarchar](100) NOT NULL,
    [price] [decimal](12,2) NOT NULL DEFAULT 0,
    [duration_days] [int] NOT NULL,
    [quota_active_listings] [int] NOT NULL DEFAULT 1,
    [priority_level] [int] NOT NULL DEFAULT 0,
    [description] [nvarchar](max) NULL,
    [is_active] [bit] NOT NULL DEFAULT 1,
    [created_at] [datetime] NOT NULL DEFAULT (getdate()),
    [updated_at] [datetime] NOT NULL DEFAULT (getdate()),
    CONSTRAINT [PK_Plans] PRIMARY KEY CLUSTERED ([plan_id] ASC)
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

-- =====================================================================
-- Table: PasswordResetTokens (Depends on: Users)
-- =====================================================================
CREATE TABLE [dbo].[PasswordResetTokens](
    [Id] [bigint] IDENTITY(1,1) NOT NULL,
    [UserId] [bigint] NOT NULL,
    [Token] [nvarchar](10) NOT NULL,
    [ExpiresAt] [datetime2](7) NOT NULL,
    [IsUsed] [bit] NOT NULL DEFAULT ((0)),
    [CreatedAt] [datetime2](7) NOT NULL DEFAULT (getutcdate()),
    CONSTRAINT [PK_PasswordResetTokens] PRIMARY KEY CLUSTERED ([Id] ASC)
) ON [PRIMARY]
GO

-- =====================================================================
-- Table: Properties (Depends on: Users)
-- =====================================================================
CREATE TABLE [dbo].[Properties](
    [property_id] [bigint] IDENTITY(1,1) NOT NULL,
    [landlord_id] [bigint] NOT NULL,
    [title] [nvarchar](255) NOT NULL,
    [description] [nvarchar](max) NULL,
    [property_type] [nvarchar](30) NOT NULL,
    [address] [nvarchar](255) NULL,
    [ward] [nvarchar](100) NULL,
    [district] [nvarchar](100) NULL,
    [city] [nvarchar](100) NULL,
    [area] [decimal](6, 2) NULL,
    [price] [decimal](12, 2) NULL,
    [deposit] [decimal](12, 2) NULL,
    [max_occupants] [int] NULL,
    [available_from] [date] NULL,
    [status] [nvarchar](20) NULL DEFAULT ('available'),
    [view_count] [int] NULL DEFAULT ((0)),
    [created_at] [datetime] NULL DEFAULT (getdate()),
    [updated_at] [datetime] NULL DEFAULT (getdate()),
    [commission_rate] [decimal](5, 2) NULL,
    [user_discount] [decimal](18, 2) NULL,
    CONSTRAINT [PK_Properties] PRIMARY KEY CLUSTERED ([property_id] ASC),
    CONSTRAINT [CK_Properties_Type] CHECK ([property_type]='nha_nguyen_can' OR [property_type]='chung_cu' OR [property_type]='phong_tro'),
    CONSTRAINT [CK_Properties_Status] CHECK ([status]='unavailable' OR [status]='rented' OR [status]='available')
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

-- =====================================================================
-- Table: Wallets (Depends on: Users)
-- =====================================================================
CREATE TABLE [dbo].[Wallets](
    [wallet_id] [bigint] IDENTITY(1,1) NOT NULL,
    [landlord_id] [bigint] NOT NULL,
    [locked_balance] [decimal](14, 2) NOT NULL DEFAULT ((0)),
    [available_balance] [decimal](14, 2) NOT NULL DEFAULT ((0)),
    [currency] [nvarchar](10) NOT NULL DEFAULT ('VND'),
    [created_at] [datetime] NOT NULL DEFAULT (getdate()),
    [updated_at] [datetime] NOT NULL DEFAULT (getdate()),
    CONSTRAINT [PK_Wallets] PRIMARY KEY CLUSTERED ([wallet_id] ASC),
    CONSTRAINT [UQ_Wallets_Landlord] UNIQUE ([landlord_id])
) ON [PRIMARY]
GO

-- =====================================================================
-- Table: PropertyImages (Depends on: Properties)
-- =====================================================================
CREATE TABLE [dbo].[PropertyImages](
    [image_id] [bigint] IDENTITY(1,1) NOT NULL,
    [property_id] [bigint] NOT NULL,
    [image_url] [nvarchar](500) NOT NULL,
    [is_primary] [bit] NULL DEFAULT ((0)),
    [display_order] [int] NULL DEFAULT ((0)),
    [uploaded_at] [datetime] NULL DEFAULT (getdate()),
    CONSTRAINT [PK_PropertyImages] PRIMARY KEY CLUSTERED ([image_id] ASC)
) ON [PRIMARY]
GO

-- =====================================================================
-- Table: PropertyAmenities (Depends on: Properties, Amenities)
-- =====================================================================
CREATE TABLE [dbo].[PropertyAmenities](
    [property_id] [bigint] NOT NULL,
    [amenity_id] [bigint] NOT NULL,
    CONSTRAINT [PK_PropertyAmenities] PRIMARY KEY CLUSTERED ([property_id] ASC, [amenity_id] ASC)
) ON [PRIMARY]
GO

-- =====================================================================
-- Table: Listings (Depends on: Properties, Users)
-- =====================================================================
CREATE TABLE [dbo].[Listings](
    [listing_id] [bigint] IDENTITY(1,1) NOT NULL,
    [property_id] [bigint] NOT NULL,
    [landlord_id] [bigint] NOT NULL,
    [title] [nvarchar](255) NOT NULL,
    [content] [nvarchar](max) NULL,
    [status] [nvarchar](20) NOT NULL DEFAULT ('draft'),
    [published_at] [datetime] NULL,
    [expires_at] [datetime] NULL,
    [view_count] [int] NOT NULL DEFAULT 0,
    [like_count] [int] NOT NULL DEFAULT 0,
    [created_at] [datetime] NOT NULL DEFAULT (getdate()),
    [updated_at] [datetime] NOT NULL DEFAULT (getdate()),
    CONSTRAINT [PK_Listings] PRIMARY KEY CLUSTERED ([listing_id] ASC),
    CONSTRAINT [CK_Listings_Status] CHECK ([status] IN ('draft','active','inactive','expired'))
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

-- =====================================================================
-- Table: ListingFavorites (Depends on: Listings, Users)
-- =====================================================================
CREATE TABLE [dbo].[ListingFavorites](
    [listing_favorite_id] [bigint] IDENTITY(1,1) NOT NULL,
    [listing_id] [bigint] NOT NULL,
    [renter_id] [bigint] NOT NULL,
    [created_at] [datetime] NOT NULL DEFAULT (getdate()),
    CONSTRAINT [PK_ListingFavorites] PRIMARY KEY CLUSTERED ([listing_favorite_id] ASC),
    CONSTRAINT [UQ_ListingFavorites] UNIQUE ([listing_id], [renter_id])
) ON [PRIMARY]
GO

-- =====================================================================
-- Table: Bookings (Depends on: Properties, Users)
-- =====================================================================
CREATE TABLE [dbo].[Bookings](
    [booking_id] [bigint] IDENTITY(1,1) NOT NULL,
    [property_id] [bigint] NOT NULL,
    [renter_id] [bigint] NOT NULL,
    [booking_date] [date] NOT NULL,
    [booking_time] [time](7) NOT NULL,
    [status] [nvarchar](20) NULL DEFAULT ('pending'),
    [notes] [nvarchar](max) NULL,
    [created_at] [datetime] NULL DEFAULT (getdate()),
    [updated_at] [datetime] NULL DEFAULT (getdate()),
    CONSTRAINT [PK_Bookings] PRIMARY KEY CLUSTERED ([booking_id] ASC),
    CONSTRAINT [CK_Bookings_Status] CHECK ([status]='completed' OR [status]='cancelled' OR [status]='confirmed' OR [status]='pending')
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

-- =====================================================================
-- Table: Favorites (Depends on: Properties, Users)
-- =====================================================================
CREATE TABLE [dbo].[Favorites](
    [favorite_id] [bigint] IDENTITY(1,1) NOT NULL,
    [user_id] [bigint] NOT NULL,
    [property_id] [bigint] NOT NULL,
    [created_at] [datetime] NULL DEFAULT (getdate()),
    CONSTRAINT [PK_Favorites] PRIMARY KEY CLUSTERED ([favorite_id] ASC),
    CONSTRAINT [UQ_Favorites] UNIQUE NONCLUSTERED ([user_id] ASC, [property_id] ASC)
) ON [PRIMARY]
GO

-- =====================================================================
-- Table: Rentals (Depends on: Properties, Users)
-- =====================================================================
CREATE TABLE [dbo].[Rentals](
    [rental_id] [bigint] IDENTITY(1,1) NOT NULL,
    [property_id] [bigint] NOT NULL,
    [landlord_id] [bigint] NOT NULL,
    [renter_id] [bigint] NOT NULL,
    [start_date] [date] NOT NULL,
    [end_date] [date] NULL,
    [monthly_rent] [decimal](12, 2) NULL,
    [deposit_amount] [decimal](12, 2) NULL,
    [payment_due_date] [int] NULL,
    [electric_price] [decimal](10, 2) NULL,
    [water_price] [decimal](10, 2) NULL,
    [internet_fee] [decimal](10, 2) NULL,
    [other_fees] [nvarchar](max) NULL,
    [status] [nvarchar](20) NULL DEFAULT ('active'),
    [termination_date] [date] NULL,
    [termination_reason] [nvarchar](max) NULL,
    [notes] [nvarchar](max) NULL,
    [created_at] [datetime] NULL DEFAULT (getdate()),
    [updated_at] [datetime] NULL DEFAULT (getdate()),
    CONSTRAINT [PK_Rentals] PRIMARY KEY CLUSTERED ([rental_id] ASC),
    CONSTRAINT [CK_Rentals_Status] CHECK ([status]='terminated' OR [status]='expired' OR [status]='active')
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

-- =====================================================================
-- Table: RentalOccupants (Depends on: Rentals, Users)
-- =====================================================================
CREATE TABLE [dbo].[RentalOccupants](
    [occupant_id] [bigint] IDENTITY(1,1) NOT NULL,
    [rental_id] [bigint] NOT NULL,
    [user_id] [bigint] NULL,
    [full_name] [nvarchar](255) NULL,
    [phone] [nvarchar](20) NULL,
    [id_number] [nvarchar](50) NULL,
    [move_in_date] [date] NULL,
    [move_out_expected] [date] NULL,
    [status] [nvarchar](20) NOT NULL DEFAULT ('active'),
    [created_at] [datetime] NOT NULL DEFAULT (getdate()),
    CONSTRAINT [PK_RentalOccupants] PRIMARY KEY CLUSTERED ([occupant_id] ASC),
    CONSTRAINT [CK_RentalOccupants_Status] CHECK ([status] IN ('active','left'))
) ON [PRIMARY]
GO

-- =====================================================================
-- Table: LandlordSubscriptions (Depends on: Users, Plans)
-- =====================================================================
CREATE TABLE [dbo].[LandlordSubscriptions](
    [subscription_id] [bigint] IDENTITY(1,1) NOT NULL,
    [landlord_id] [bigint] NOT NULL,
    [plan_id] [bigint] NOT NULL,
    [start_at] [datetime] NOT NULL,
    [end_at] [datetime] NOT NULL,
    [status] [nvarchar](20) NOT NULL DEFAULT ('active'),
    [quota_remaining] [int] NOT NULL DEFAULT 0,
    [created_at] [datetime] NOT NULL DEFAULT (getdate()),
    [updated_at] [datetime] NOT NULL DEFAULT (getdate()),
    CONSTRAINT [PK_LandlordSubscriptions] PRIMARY KEY CLUSTERED ([subscription_id] ASC),
    CONSTRAINT [CK_LandlordSubscriptions_Status] CHECK ([status] IN ('active','expired','cancelled'))
) ON [PRIMARY]
GO

-- =====================================================================
-- Table: Invoices (Depends on: Rentals)
-- =====================================================================
CREATE TABLE [dbo].[Invoices](
    [invoice_id] [bigint] IDENTITY(1,1) NOT NULL,
    [rental_id] [bigint] NOT NULL,
    [invoice_month] [char](7) NULL,
    [room_rent] [decimal](12, 2) NULL,
    [electric_old_reading] [int] NULL,
    [electric_new_reading] [int] NULL,
    [electric_usage] [int] NULL,
    [electric_amount] [decimal](12, 2) NULL,
    [water_old_reading] [int] NULL,
    [water_new_reading] [int] NULL,
    [water_usage] [int] NULL,
    [water_amount] [decimal](12, 2) NULL,
    [internet_fee] [decimal](10, 2) NULL,
    [other_fees] [nvarchar](max) NULL,
    [total_amount] [decimal](14, 2) NULL,
    [due_date] [date] NULL,
    [payment_date] [date] NULL,
    [payment_method] [nvarchar](30) NULL,
    [payment_proof_url] [nvarchar](500) NULL,
    [status] [nvarchar](20) NULL DEFAULT ('pending'),
    [notes] [nvarchar](max) NULL,
    [created_at] [datetime] NULL DEFAULT (getdate()),
    [updated_at] [datetime] NULL DEFAULT (getdate()),
    CONSTRAINT [PK_Invoices] PRIMARY KEY CLUSTERED ([invoice_id] ASC),
    CONSTRAINT [CK_Invoices_PaymentMethod] CHECK ([payment_method]='zalopay' OR [payment_method]='momo' OR [payment_method]='bank_transfer' OR [payment_method]='cash'),
    CONSTRAINT [CK_Invoices_Status] CHECK ([status]='cancelled' OR [status]='overdue' OR [status]='paid' OR [status]='pending')
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

-- =====================================================================
-- Table: Messages (Depends on: Properties, Users)
-- =====================================================================
CREATE TABLE [dbo].[Messages](
    [message_id] [bigint] IDENTITY(1,1) NOT NULL,
    [sender_id] [bigint] NOT NULL,
    [receiver_id] [bigint] NOT NULL,
    [property_id] [bigint] NULL,
    [content] [nvarchar](max) NOT NULL,
    [is_read] [bit] NULL DEFAULT ((0)),
    [created_at] [datetime] NULL DEFAULT (getdate()),
    CONSTRAINT [PK_Messages] PRIMARY KEY CLUSTERED ([message_id] ASC)
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

-- =====================================================================
-- Table: Notifications (Depends on: Users)
-- =====================================================================
CREATE TABLE [dbo].[Notifications](
    [notification_id] [bigint] IDENTITY(1,1) NOT NULL,
    [user_id] [bigint] NOT NULL,
    [type] [nvarchar](20) NULL,
    [title] [nvarchar](255) NULL,
    [content] [nvarchar](max) NULL,
    [link_url] [nvarchar](500) NULL,
    [is_read] [bit] NULL DEFAULT ((0)),
    [created_at] [datetime] NULL DEFAULT (getdate()),
    CONSTRAINT [PK_Notifications] PRIMARY KEY CLUSTERED ([notification_id] ASC),
    CONSTRAINT [CK_Notifications_Type] CHECK ([type]='review' OR [type]='message' OR [type]='payment' OR [type]='booking')
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

-- =====================================================================
-- Table: Payments (Depends on: Users, Rentals, Invoices, LandlordSubscriptions)
-- MERGED: Combines old structure with new fields and relationships
-- =====================================================================
CREATE TABLE [dbo].[Payments](
    [payment_id] [bigint] IDENTITY(1,1) NOT NULL,
    [payer_user_id] [bigint] NOT NULL,
    [landlord_id] [bigint] NULL,
    [rental_id] [bigint] NULL,
    [invoice_id] [bigint] NULL,
    [subscription_id] [bigint] NULL,
    [payment_type] [nvarchar](20) NOT NULL,
    [amount] [decimal](14, 2) NOT NULL,
    [provider] [nvarchar](20) NOT NULL DEFAULT ('payos'),
    [provider_order_code] [nvarchar](100) NULL,
    [pay_url] [nvarchar](1000) NULL,
    [status] [nvarchar](20) NOT NULL DEFAULT ('created'),
    [created_at] [datetime] NOT NULL DEFAULT (getdate()),
    [paid_at] [datetime] NULL,
    [raw_webhook] [nvarchar](max) NULL,
    [platform_commission] [decimal](18, 2) NULL,
    [landlord_amount] [decimal](18, 2) NULL,
    [user_discount_applied] [decimal](18, 2) NULL,
    CONSTRAINT [PK_Payments] PRIMARY KEY CLUSTERED ([payment_id] ASC),
    CONSTRAINT [CK_Payments_Type] CHECK ([payment_type] IN ('deposit','invoice','subscription','Deposit','Invoice','Subscription')),
    CONSTRAINT [CK_Payments_Provider] CHECK ([provider] IN ('payos','PayOS')),
    CONSTRAINT [CK_Payments_Status] CHECK ([status] IN ('created','paid','failed','cancelled','Pending','Completed','Cancelled'))
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

-- =====================================================================
-- Table: RentSchedules (Depends on: Rentals, Payments)
-- =====================================================================
CREATE TABLE [dbo].[RentSchedules](
    [schedule_id] [bigint] IDENTITY(1,1) NOT NULL,
    [rental_id] [bigint] NOT NULL,
    [period_month] [char](7) NOT NULL,
    [due_date] [date] NOT NULL,
    [amount] [decimal](14, 2) NOT NULL,
    [status] [nvarchar](20) NOT NULL DEFAULT ('pending'),
    [paid_at] [datetime] NULL,
    [payment_id] [bigint] NULL,
    [created_at] [datetime] NOT NULL DEFAULT (getdate()),
    [updated_at] [datetime] NOT NULL DEFAULT (getdate()),
    CONSTRAINT [PK_RentSchedules] PRIMARY KEY CLUSTERED ([schedule_id] ASC),
    CONSTRAINT [UQ_RentSchedules] UNIQUE ([rental_id], [period_month]),
    CONSTRAINT [CK_RentSchedules_Status] CHECK ([status] IN ('pending','paid','overdue','cancelled'))
) ON [PRIMARY]
GO

-- =====================================================================
-- Table: Reports (Depends on: Properties, Users)
-- =====================================================================
CREATE TABLE [dbo].[Reports](
    [report_id] [bigint] IDENTITY(1,1) NOT NULL,
    [property_id] [bigint] NOT NULL,
    [reporter_id] [bigint] NOT NULL,
    [reason] [nvarchar](255) NULL,
    [description] [nvarchar](max) NULL,
    [status] [nvarchar](20) NULL DEFAULT ('pending'),
    [created_at] [datetime] NULL DEFAULT (getdate()),
    [resolved_at] [datetime] NULL,
    CONSTRAINT [PK_Reports] PRIMARY KEY CLUSTERED ([report_id] ASC),
    CONSTRAINT [CK_Reports_Status] CHECK ([status]='rejected' OR [status]='resolved' OR [status]='reviewing' OR [status]='pending')
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

-- =====================================================================
-- Table: Reviews (Depends on: Properties, Users)
-- =====================================================================
CREATE TABLE [dbo].[Reviews](
    [review_id] [bigint] IDENTITY(1,1) NOT NULL,
    [property_id] [bigint] NOT NULL,
    [user_id] [bigint] NOT NULL,
    [rating] [int] NULL,
    [comment] [nvarchar](max) NULL,
    [images] [nvarchar](max) NULL,
    [created_at] [datetime] NULL DEFAULT (getdate()),
    [updated_at] [datetime] NULL DEFAULT (getdate()),
    CONSTRAINT [PK_Reviews] PRIMARY KEY CLUSTERED ([review_id] ASC),
    CONSTRAINT [CK_Reviews_Rating] CHECK ([rating]>=(1) AND [rating]<=(5))
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

-- =====================================================================
-- Table: WalletTransactions (Depends on: Wallets)
-- MERGED: Updated direction values to match new spec
-- =====================================================================
CREATE TABLE [dbo].[WalletTransactions](
    [wallet_txn_id] [bigint] IDENTITY(1,1) NOT NULL,
    [wallet_id] [bigint] NOT NULL,
    [direction] [nvarchar](10) NOT NULL,
    [amount] [decimal](14, 2) NOT NULL,
    [related_type] [nvarchar](50) NOT NULL,
    [related_id] [bigint] NULL,
    [status] [nvarchar](20) NOT NULL,
    [note] [nvarchar](500) NULL,
    [created_at] [datetime] NOT NULL DEFAULT (getdate()),
    CONSTRAINT [PK_WalletTransactions] PRIMARY KEY CLUSTERED ([wallet_txn_id] ASC),
    CONSTRAINT [CK_WalletTransactions_Direction] CHECK ([direction] IN ('in','out','credit','debit')),
    CONSTRAINT [CK_WalletTransactions_RelatedType] CHECK ([related_type] IN ('deposit','invoice','subscription','withdraw','adjustment','booking','withdraw_request')),
    CONSTRAINT [CK_WalletTransactions_Status] CHECK ([status] IN ('posted','reversed','pending','locked','completed'))
) ON [PRIMARY]
GO

-- =====================================================================
-- Table: WithdrawRequests (Depends on: Wallets, Users)
-- MERGED: Updated status values to include all options
-- =====================================================================
CREATE TABLE [dbo].[WithdrawRequests](
    [withdraw_id] [bigint] IDENTITY(1,1) NOT NULL,
    [wallet_id] [bigint] NOT NULL,
    [landlord_id] [bigint] NOT NULL,
    [amount] [decimal](14, 2) NOT NULL,
    [bank_name] [nvarchar](100) NOT NULL,
    [bank_account] [nvarchar](50) NOT NULL,
    [account_holder] [nvarchar](100) NOT NULL,
    [status] [nvarchar](20) NOT NULL DEFAULT ('Pending'),
    [requested_at] [datetime] NOT NULL DEFAULT (getdate()),
    [processed_at] [datetime] NULL,
    [processed_by_admin_id] [bigint] NULL,
    [note] [nvarchar](500) NULL,
    CONSTRAINT [PK_WithdrawRequests] PRIMARY KEY CLUSTERED ([withdraw_id] ASC),
    CONSTRAINT [CK_WithdrawRequests_Status] CHECK ([status] IN ('pending','approved','rejected','completed','Pending','Approved','Rejected','Completed'))
) ON [PRIMARY]
GO

PRINT N'Tables created successfully.';
GO

-- =====================================================================
-- SECTION 3: ADD FOREIGN KEY CONSTRAINTS
-- =====================================================================
PRINT N'Adding foreign key constraints...';
GO

-- Users-related FKs
ALTER TABLE [dbo].[PasswordResetTokens] WITH CHECK 
    ADD CONSTRAINT [FK_PasswordResetTokens_Users] FOREIGN KEY([UserId])
    REFERENCES [dbo].[Users] ([user_id]) ON DELETE CASCADE;

ALTER TABLE [dbo].[Properties] WITH CHECK 
    ADD CONSTRAINT [fk_properties_landlord] FOREIGN KEY([landlord_id])
    REFERENCES [dbo].[Users] ([user_id]);

ALTER TABLE [dbo].[Notifications] WITH CHECK 
    ADD CONSTRAINT [fk_notifications_user] FOREIGN KEY([user_id])
    REFERENCES [dbo].[Users] ([user_id]);

ALTER TABLE [dbo].[Wallets] WITH CHECK 
    ADD CONSTRAINT [fk_wallets_landlord] FOREIGN KEY([landlord_id])
    REFERENCES [dbo].[Users] ([user_id]);

ALTER TABLE [dbo].[LandlordSubscriptions] WITH CHECK 
    ADD CONSTRAINT [fk_subscriptions_landlord] FOREIGN KEY([landlord_id])
    REFERENCES [dbo].[Users] ([user_id]);

ALTER TABLE [dbo].[LandlordSubscriptions] WITH CHECK 
    ADD CONSTRAINT [fk_subscriptions_plan] FOREIGN KEY([plan_id])
    REFERENCES [dbo].[Plans] ([plan_id]);

-- Properties-related FKs
ALTER TABLE [dbo].[PropertyImages] WITH CHECK 
    ADD CONSTRAINT [fk_images_property] FOREIGN KEY([property_id])
    REFERENCES [dbo].[Properties] ([property_id]);

ALTER TABLE [dbo].[PropertyAmenities] WITH CHECK 
    ADD CONSTRAINT [fk_pa_property] FOREIGN KEY([property_id])
    REFERENCES [dbo].[Properties] ([property_id]);

ALTER TABLE [dbo].[PropertyAmenities] WITH CHECK 
    ADD CONSTRAINT [fk_pa_amenity] FOREIGN KEY([amenity_id])
    REFERENCES [dbo].[Amenities] ([amenity_id]);

ALTER TABLE [dbo].[Listings] WITH CHECK 
    ADD CONSTRAINT [fk_listings_property] FOREIGN KEY([property_id])
    REFERENCES [dbo].[Properties] ([property_id]);

ALTER TABLE [dbo].[Listings] WITH CHECK 
    ADD CONSTRAINT [fk_listings_landlord] FOREIGN KEY([landlord_id])
    REFERENCES [dbo].[Users] ([user_id]);

ALTER TABLE [dbo].[ListingFavorites] WITH CHECK 
    ADD CONSTRAINT [fk_listingfavorites_listing] FOREIGN KEY([listing_id])
    REFERENCES [dbo].[Listings] ([listing_id]);

ALTER TABLE [dbo].[ListingFavorites] WITH CHECK 
    ADD CONSTRAINT [fk_listingfavorites_renter] FOREIGN KEY([renter_id])
    REFERENCES [dbo].[Users] ([user_id]);

ALTER TABLE [dbo].[Bookings] WITH CHECK 
    ADD CONSTRAINT [fk_bookings_property] FOREIGN KEY([property_id])
    REFERENCES [dbo].[Properties] ([property_id]);

ALTER TABLE [dbo].[Bookings] WITH CHECK 
    ADD CONSTRAINT [fk_bookings_renter] FOREIGN KEY([renter_id])
    REFERENCES [dbo].[Users] ([user_id]);

ALTER TABLE [dbo].[Favorites] WITH CHECK 
    ADD CONSTRAINT [fk_favorites_property] FOREIGN KEY([property_id])
    REFERENCES [dbo].[Properties] ([property_id]);

ALTER TABLE [dbo].[Favorites] WITH CHECK 
    ADD CONSTRAINT [fk_favorites_user] FOREIGN KEY([user_id])
    REFERENCES [dbo].[Users] ([user_id]);

ALTER TABLE [dbo].[Messages] WITH CHECK 
    ADD CONSTRAINT [fk_messages_property] FOREIGN KEY([property_id])
    REFERENCES [dbo].[Properties] ([property_id]);

ALTER TABLE [dbo].[Messages] WITH CHECK 
    ADD CONSTRAINT [fk_messages_sender] FOREIGN KEY([sender_id])
    REFERENCES [dbo].[Users] ([user_id]);

ALTER TABLE [dbo].[Messages] WITH CHECK 
    ADD CONSTRAINT [fk_messages_receiver] FOREIGN KEY([receiver_id])
    REFERENCES [dbo].[Users] ([user_id]);

ALTER TABLE [dbo].[Reports] WITH CHECK 
    ADD CONSTRAINT [fk_reports_property] FOREIGN KEY([property_id])
    REFERENCES [dbo].[Properties] ([property_id]);

ALTER TABLE [dbo].[Reports] WITH CHECK 
    ADD CONSTRAINT [fk_reports_user] FOREIGN KEY([reporter_id])
    REFERENCES [dbo].[Users] ([user_id]);

ALTER TABLE [dbo].[Reviews] WITH CHECK 
    ADD CONSTRAINT [fk_reviews_property] FOREIGN KEY([property_id])
    REFERENCES [dbo].[Properties] ([property_id]);

ALTER TABLE [dbo].[Reviews] WITH CHECK 
    ADD CONSTRAINT [fk_reviews_user] FOREIGN KEY([user_id])
    REFERENCES [dbo].[Users] ([user_id]);

-- Rentals-related FKs
ALTER TABLE [dbo].[Rentals] WITH CHECK 
    ADD CONSTRAINT [fk_rentals_property] FOREIGN KEY([property_id])
    REFERENCES [dbo].[Properties] ([property_id]);

ALTER TABLE [dbo].[Rentals] WITH CHECK 
    ADD CONSTRAINT [fk_rentals_landlord] FOREIGN KEY([landlord_id])
    REFERENCES [dbo].[Users] ([user_id]);

ALTER TABLE [dbo].[Rentals] WITH CHECK 
    ADD CONSTRAINT [fk_rentals_renter] FOREIGN KEY([renter_id])
    REFERENCES [dbo].[Users] ([user_id]);

ALTER TABLE [dbo].[RentalOccupants] WITH CHECK 
    ADD CONSTRAINT [fk_occupants_rental] FOREIGN KEY([rental_id])
    REFERENCES [dbo].[Rentals] ([rental_id]);

ALTER TABLE [dbo].[RentalOccupants] WITH CHECK 
    ADD CONSTRAINT [fk_occupants_user] FOREIGN KEY([user_id])
    REFERENCES [dbo].[Users] ([user_id]);

ALTER TABLE [dbo].[RentSchedules] WITH CHECK 
    ADD CONSTRAINT [fk_rentschedules_rental] FOREIGN KEY([rental_id])
    REFERENCES [dbo].[Rentals] ([rental_id]);

ALTER TABLE [dbo].[RentSchedules] WITH CHECK 
    ADD CONSTRAINT [fk_rentschedules_payment] FOREIGN KEY([payment_id])
    REFERENCES [dbo].[Payments] ([payment_id]);

ALTER TABLE [dbo].[Invoices] WITH CHECK 
    ADD CONSTRAINT [fk_invoices_rental] FOREIGN KEY([rental_id])
    REFERENCES [dbo].[Rentals] ([rental_id]);

-- Payments-related FKs
ALTER TABLE [dbo].[Payments] WITH CHECK 
    ADD CONSTRAINT [fk_payments_payer] FOREIGN KEY([payer_user_id])
    REFERENCES [dbo].[Users] ([user_id]);

ALTER TABLE [dbo].[Payments] WITH CHECK 
    ADD CONSTRAINT [fk_payments_landlord] FOREIGN KEY([landlord_id])
    REFERENCES [dbo].[Users] ([user_id]);

ALTER TABLE [dbo].[Payments] WITH CHECK 
    ADD CONSTRAINT [fk_payments_rental] FOREIGN KEY([rental_id])
    REFERENCES [dbo].[Rentals] ([rental_id]);

ALTER TABLE [dbo].[Payments] WITH CHECK 
    ADD CONSTRAINT [fk_payments_invoice] FOREIGN KEY([invoice_id])
    REFERENCES [dbo].[Invoices] ([invoice_id]);

ALTER TABLE [dbo].[Payments] WITH CHECK 
    ADD CONSTRAINT [fk_payments_subscription] FOREIGN KEY([subscription_id])
    REFERENCES [dbo].[LandlordSubscriptions] ([subscription_id]);

-- Wallets-related FKs
ALTER TABLE [dbo].[WalletTransactions] WITH CHECK 
    ADD CONSTRAINT [fk_wallettxn_wallet] FOREIGN KEY([wallet_id])
    REFERENCES [dbo].[Wallets] ([wallet_id]);

ALTER TABLE [dbo].[WithdrawRequests] WITH CHECK 
    ADD CONSTRAINT [fk_withdraw_wallet] FOREIGN KEY([wallet_id])
    REFERENCES [dbo].[Wallets] ([wallet_id]);

ALTER TABLE [dbo].[WithdrawRequests] WITH CHECK 
    ADD CONSTRAINT [fk_withdraw_landlord] FOREIGN KEY([landlord_id])
    REFERENCES [dbo].[Users] ([user_id]);

ALTER TABLE [dbo].[WithdrawRequests] WITH CHECK 
    ADD CONSTRAINT [fk_withdraw_admin] FOREIGN KEY([processed_by_admin_id])
    REFERENCES [dbo].[Users] ([user_id]);

PRINT N'Foreign key constraints added successfully.';
GO

-- =====================================================================
-- SECTION 4: CREATE INDEXES
-- =====================================================================
PRINT N'Creating indexes...';
GO

-- PasswordResetTokens indexes
CREATE NONCLUSTERED INDEX [IX_PasswordResetTokens_UserId] 
    ON [dbo].[PasswordResetTokens]([UserId] ASC);
CREATE NONCLUSTERED INDEX [IX_PasswordResetTokens_Token] 
    ON [dbo].[PasswordResetTokens]([Token] ASC);
CREATE NONCLUSTERED INDEX [IX_PasswordResetTokens_ExpiresAt] 
    ON [dbo].[PasswordResetTokens]([ExpiresAt] ASC);

-- Listings indexes
CREATE NONCLUSTERED INDEX [ix_listings_landlord] 
    ON [dbo].[Listings]([landlord_id] ASC);
CREATE NONCLUSTERED INDEX [ix_listings_property] 
    ON [dbo].[Listings]([property_id] ASC);
CREATE NONCLUSTERED INDEX [ix_listings_status] 
    ON [dbo].[Listings]([status] ASC);

-- ListingFavorites indexes
CREATE NONCLUSTERED INDEX [ix_listingfavorites_renter] 
    ON [dbo].[ListingFavorites]([renter_id] ASC);

-- LandlordSubscriptions indexes
CREATE NONCLUSTERED INDEX [ix_subscriptions_landlord] 
    ON [dbo].[LandlordSubscriptions]([landlord_id] ASC);
CREATE NONCLUSTERED INDEX [ix_subscriptions_status] 
    ON [dbo].[LandlordSubscriptions]([status] ASC);

-- RentalOccupants indexes
CREATE NONCLUSTERED INDEX [ix_occupants_rental] 
    ON [dbo].[RentalOccupants]([rental_id] ASC);

-- RentSchedules indexes
CREATE NONCLUSTERED INDEX [ix_rentschedules_status] 
    ON [dbo].[RentSchedules]([status] ASC);

-- Payments indexes
CREATE NONCLUSTERED INDEX [ix_payments_payer] 
    ON [dbo].[Payments]([payer_user_id] ASC);
CREATE NONCLUSTERED INDEX [ix_payments_provider_order_code] 
    ON [dbo].[Payments]([provider_order_code] ASC);
CREATE NONCLUSTERED INDEX [ix_payments_type_status] 
    ON [dbo].[Payments]([payment_type] ASC, [status] ASC);

-- Wallets indexes
CREATE NONCLUSTERED INDEX [ix_wallets_landlord] 
    ON [dbo].[Wallets]([landlord_id] ASC);

-- WalletTransactions indexes
CREATE NONCLUSTERED INDEX [ix_wallettxn_wallet] 
    ON [dbo].[WalletTransactions]([wallet_id] ASC);
CREATE NONCLUSTERED INDEX [ix_wallettxn_status] 
    ON [dbo].[WalletTransactions]([status] ASC);
CREATE NONCLUSTERED INDEX [ix_wallettxn_related] 
    ON [dbo].[WalletTransactions]([related_type] ASC, [related_id] ASC);

-- WithdrawRequests indexes
CREATE NONCLUSTERED INDEX [ix_withdraw_landlord] 
    ON [dbo].[WithdrawRequests]([landlord_id] ASC);
CREATE NONCLUSTERED INDEX [ix_withdraw_status] 
    ON [dbo].[WithdrawRequests]([status] ASC);

PRINT N'Indexes created successfully.';
GO

-- =====================================================================
-- SECTION 5: SEED DATA
-- =====================================================================
PRINT N'Inserting seed data...';
GO

-- =====================================================================
-- Seed: Users
-- =====================================================================
SET IDENTITY_INSERT [dbo].[Users] ON;

INSERT [dbo].[Users] ([user_id], [email], [password_hash], [full_name], [phone], [avatar_url], [user_type], [is_verified], [status], [created_at], [updated_at]) VALUES
(1, N'landlord1@nestflow.com', N'$2a$12$x6NyUpxTrrjw3iC1QAoqU.S5ycQxvFT6XnmeP9v4bEm4QpoeGH6V2', N'Nguyễn Văn A', N'0987654321', N'https://ui-avatars.com/api/?name=Nguyen+Van+A&background=random', N'landlord', 0, N'active', CAST(N'2026-02-04T21:59:40.970' AS DateTime), CAST(N'2026-02-04T21:59:40.970' AS DateTime)),
(2, N'landlord2@nestflow.com', N'123456', N'Trần Thị B', N'0987654322', N'https://ui-avatars.com/api/?name=Tran+Thi+B&background=random', N'landlord', 0, N'active', CAST(N'2026-02-04T21:59:40.970' AS DateTime), CAST(N'2026-02-04T21:59:40.970' AS DateTime)),
(3, N'landlord3@nestflow.com', N'123456', N'Lê Văn C', N'0987654323', N'https://ui-avatars.com/api/?name=Le+Van+C&background=random', N'landlord', 0, N'active', CAST(N'2026-02-04T21:59:40.970' AS DateTime), CAST(N'2026-02-04T21:59:40.970' AS DateTime)),
(4, N'luongdanduy03@gmail.com', N'$2a$12$x6NyUpxTrrjw3iC1QAoqU.S5ycQxvFT6XnmeP9v4bEm4QpoeGH6V2', N'Lường Đan Duy', N'0867787339', NULL, N'renter', 0, N'active', CAST(N'2026-02-04T15:58:59.780' AS DateTime), CAST(N'2026-02-04T15:58:59.780' AS DateTime)),
(5, N'tien@gmail.com', N'$2a$11$kI.Ue8lNFdaqxQNpOX8c7.xnYNvPukH6hDpy0qJypWasQkMWwioq6', N'Hoang Tien', N'01294392324', NULL, N'landlord', 0, N'active', CAST(N'2026-02-04T17:36:25.533' AS DateTime), CAST(N'2026-02-04T17:36:25.533' AS DateTime));

SET IDENTITY_INSERT [dbo].[Users] OFF;
GO

-- =====================================================================
-- Seed: Amenities
-- =====================================================================
SET IDENTITY_INSERT [dbo].[Amenities] ON;

INSERT [dbo].[Amenities] ([amenity_id], [name], [icon_url], [category]) VALUES
(1, N'Wifi miễn phí', N'fas fa-wifi', N'basic'),
(2, N'Điều hòa', N'fas fa-wind', N'furniture'),
(3, N'Nóng lạnh', N'fas fa-water', N'basic'),
(4, N'Máy giặt', N'fas fa-tshirt', N'furniture'),
(5, N'Chỗ để xe', N'fas fa-motorcycle', N'basic'),
(6, N'An ninh 24/7', N'fas fa-shield-alt', N'security'),
(7, N'Tủ lạnh', N'fas fa-snowflake', N'furniture'),
(8, N'Camera', N'fas fa-video', N'security'),
(9, N'Thang máy', N'fas fa-elevator', N'basic'),
(10, N'Bếp chung', N'fas fa-fire-burner', N'furniture');

SET IDENTITY_INSERT [dbo].[Amenities] OFF;
GO

-- =====================================================================
-- Seed: Plans (Subscription packages)
-- =====================================================================
SET IDENTITY_INSERT [dbo].[Plans] ON;

INSERT [dbo].[Plans] ([plan_id], [plan_name], [price], [duration_days], [quota_active_listings], [priority_level], [description], [is_active], [created_at], [updated_at]) VALUES
(1, N'Gói Miễn Phí', CAST(0.00 AS Decimal(12, 2)), 30, 1, 0, N'Đăng 1 tin miễn phí trong 30 ngày', 1, GETDATE(), GETDATE()),
(2, N'Gói Cơ Bản', CAST(100000.00 AS Decimal(12, 2)), 30, 5, 1, N'Đăng tối đa 5 tin trong 30 ngày', 1, GETDATE(), GETDATE()),
(3, N'Gói Nâng Cao', CAST(300000.00 AS Decimal(12, 2)), 90, 20, 2, N'Đăng tối đa 20 tin trong 90 ngày với ưu tiên hiển thị', 1, GETDATE(), GETDATE()),
(4, N'Gói VIP', CAST(500000.00 AS Decimal(12, 2)), 90, 50, 3, N'Đăng không giới hạn trong 90 ngày, ưu tiên cao nhất', 1, GETDATE(), GETDATE());

SET IDENTITY_INSERT [dbo].[Plans] OFF;
GO

-- =====================================================================
-- Seed: PasswordResetTokens
-- =====================================================================
SET IDENTITY_INSERT [dbo].[PasswordResetTokens] ON;

INSERT [dbo].[PasswordResetTokens] ([Id], [UserId], [Token], [ExpiresAt], [IsUsed], [CreatedAt]) VALUES
(1, 4, N'206282', CAST(N'2026-02-04T16:30:54.5417198' AS DateTime2), 0, CAST(N'2026-02-04T16:15:54.5417992' AS DateTime2)),
(2, 4, N'404772', CAST(N'2026-02-04T16:30:55.3524173' AS DateTime2), 0, CAST(N'2026-02-04T16:15:55.3524184' AS DateTime2));

SET IDENTITY_INSERT [dbo].[PasswordResetTokens] OFF;
GO

-- =====================================================================
-- Seed: Properties
-- =====================================================================
SET IDENTITY_INSERT [dbo].[Properties] ON;

INSERT [dbo].[Properties] ([property_id], [landlord_id], [title], [description], [property_type], [address], [ward], [district], [city], [area], [price], [deposit], [max_occupants], [available_from], [status], [view_count], [created_at], [updated_at], [commission_rate], [user_discount]) VALUES
(1, 1, N'Phòng trọ khép kín full đồ tại Thạch Hòa', N'Phòng đẹp, đầy đủ tiện nghi, giá giấc tự do, an ninh tốt.', N'phong_tro', N'Số 10, Ngõ 5', N'Thạch Hòa', N'Thạch Thất', N'Hà Nội', CAST(25.00 AS Decimal(6, 2)), CAST(2500000.00 AS Decimal(12, 2)), CAST(2500000.00 AS Decimal(12, 2)), 2, CAST(N'2026-02-04' AS Date), N'available', 120, CAST(N'2026-02-04T21:59:40.973' AS DateTime), CAST(N'2026-02-04T21:59:40.973' AS DateTime), CAST(50.00 AS Decimal(5, 2)), CAST(500000.00 AS Decimal(18, 2))),
(2, 1, N'Chung cư mini cao cấp tại Tân Xã', N'Nhà mới xây, có thang máy, bảo vệ 24/7. Nội thất sang trọng.', N'chung_cu', N'Số 22, Phố Tân Xã', N'Tân Xã', N'Thạch Thất', N'Hà Nội', CAST(40.00 AS Decimal(6, 2)), CAST(5000000.00 AS Decimal(12, 2)), CAST(5000000.00 AS Decimal(12, 2)), 3, CAST(N'2026-02-04' AS Date), N'available', 345, CAST(N'2026-02-04T21:59:40.973' AS DateTime), CAST(N'2026-02-04T21:59:40.973' AS DateTime), CAST(50.00 AS Decimal(5, 2)), CAST(500000.00 AS Decimal(18, 2))),
(3, 1, N'Phòng trọ giá rẻ sinh viên tại Bình Yên', N'Phòng thoáng mát, gần chợ, trường học. Phù hợp sinh viên.', N'phong_tro', N'Ngách 23', N'Bình Yên', N'Thạch Thất', N'Hà Nội', CAST(20.00 AS Decimal(6, 2)), CAST(1300000.00 AS Decimal(12, 2)), CAST(1300000.00 AS Decimal(12, 2)), 2, CAST(N'2026-02-04' AS Date), N'available', 290, CAST(N'2026-02-04T21:59:40.973' AS DateTime), CAST(N'2026-02-04T21:59:40.973' AS DateTime), CAST(50.00 AS Decimal(5, 2)), CAST(500000.00 AS Decimal(18, 2))),
(4, 2, N'Nhà nguyên căn 2 tầng tại Quảng An', N'Nhà riêng biệt, đầy đủ nội thất, có sân vườn. Yên tĩnh.', N'nha_nguyen_can', N'Phố Quảng An', N'Quảng An', N'Tây Hồ', N'Hà Nội', CAST(80.00 AS Decimal(6, 2)), CAST(8000000.00 AS Decimal(12, 2)), CAST(8000000.00 AS Decimal(12, 2)), 5, CAST(N'2026-02-04' AS Date), N'available', 410, CAST(N'2026-02-04T21:59:40.973' AS DateTime), CAST(N'2026-02-04T21:59:40.973' AS DateTime), CAST(50.00 AS Decimal(5, 2)), CAST(500000.00 AS Decimal(18, 2))),
(5, 2, N'Phòng trọ view hồ Tây đẹp', N'View thoáng, sáng, gần hồ Tây. Tiện ích xung quanh đầy đủ.', N'phong_tro', N'Phố Yên Phụ', N'Yên Phụ', N'Tây Hồ', N'Hà Nội', CAST(28.00 AS Decimal(6, 2)), CAST(3500000.00 AS Decimal(12, 2)), CAST(3500000.00 AS Decimal(12, 2)), 2, CAST(N'2026-02-04' AS Date), N'available', 275, CAST(N'2026-02-04T21:59:40.973' AS DateTime), CAST(N'2026-02-04T21:59:40.973' AS DateTime), CAST(50.00 AS Decimal(5, 2)), CAST(500000.00 AS Decimal(18, 2))),
(6, 2, N'Chung cư cao cấp Xuân La', N'Chung cư mới, nhiều tiện ích: gym, hồ bơi, siêu thị.', N'chung_cu', N'Đường Xuân La', N'Xuân La', N'Tây Hồ', N'Hà Nội', CAST(60.00 AS Decimal(6, 2)), CAST(7500000.00 AS Decimal(12, 2)), CAST(7500000.00 AS Decimal(12, 2)), 3, CAST(N'2026-02-04' AS Date), N'available', 390, CAST(N'2026-02-04T21:59:40.973' AS DateTime), CAST(N'2026-02-04T21:59:40.973' AS DateTime), CAST(50.00 AS Decimal(5, 2)), CAST(500000.00 AS Decimal(18, 2))),
(7, 2, N'Phòng trọ giá tốt tại Nhật Tân', N'Gần bãi đậu xe, chợ, trường học. Tiện lợi đi lại.', N'phong_tro', N'Ngõ Nhật Tân', N'Nhật Tân', N'Tây Hồ', N'Hà Nội', CAST(22.00 AS Decimal(6, 2)), CAST(1800000.00 AS Decimal(12, 2)), CAST(1800000.00 AS Decimal(12, 2)), 2, CAST(N'2026-02-04' AS Date), N'available', 215, CAST(N'2026-02-04T21:59:40.973' AS DateTime), CAST(N'2026-02-04T21:59:40.973' AS DateTime), CAST(50.00 AS Decimal(5, 2)), CAST(500000.00 AS Decimal(18, 2))),
(8, 2, N'Nhà nguyên căn 3 tầng tại Bình Yên', N'Mặt đường lớn, tiện kinh doanh kết hợp ở.', N'nha_nguyen_can', N'Mặt đường 420', N'Bình Yên', N'Thạch Thất', N'Hà Nội', CAST(120.00 AS Decimal(6, 2)), CAST(10000000.00 AS Decimal(12, 2)), CAST(10000000.00 AS Decimal(12, 2)), 6, CAST(N'2026-02-04' AS Date), N'available', 520, CAST(N'2026-02-04T21:59:40.973' AS DateTime), CAST(N'2026-02-04T21:59:40.973' AS DateTime), CAST(50.00 AS Decimal(5, 2)), CAST(500000.00 AS Decimal(18, 2))),
(17, 3, N'Phòng trọ có gác lửng tại Tân Xã', N'Gác lửng cao không ấm đầu, thang gỗ chắc chắn.', N'phong_tro', N'Ngõ 15', N'Tân Xã', N'Thạch Thất', N'Hà Nội', CAST(25.00 AS Decimal(6, 2)), CAST(2800000.00 AS Decimal(12, 2)), CAST(2800000.00 AS Decimal(12, 2)), 2, CAST(N'2026-02-04' AS Date), N'available', 165, CAST(N'2026-02-04T21:59:40.973' AS DateTime), CAST(N'2026-02-04T21:59:40.973' AS DateTime), CAST(50.00 AS Decimal(5, 2)), CAST(500000.00 AS Decimal(18, 2))),
(18, 3, N'Homestay container độc lạ tại Đông Trúc', N'Trải nghiệm sống độc đáo, không gian sáng tạo.', N'phong_tro', N'Khu sinh thái', N'Đông Trúc', N'Thạch Thất', N'Hà Nội', CAST(20.00 AS Decimal(6, 2)), CAST(2500000.00 AS Decimal(12, 2)), CAST(2500000.00 AS Decimal(12, 2)), 2, CAST(N'2026-02-04' AS Date), N'available', 195, CAST(N'2026-02-04T21:59:40.973' AS DateTime), CAST(N'2026-02-04T21:59:40.973' AS DateTime), CAST(50.00 AS Decimal(5, 2)), CAST(500000.00 AS Decimal(18, 2))),
(19, 3, N'Chung cư mini full option tại Thạch Hòa', N'Tủ lạnh, máy giặt, tivi, sofa, giường nệm cao su.', N'chung_cu', N'Khu CNC 2', N'Thạch Hòa', N'Thạch Thất', N'Hà Nội', CAST(35.00 AS Decimal(6, 2)), CAST(4800000.00 AS Decimal(12, 2)), CAST(4800000.00 AS Decimal(12, 2)), 2, CAST(N'2026-02-04' AS Date), N'available', 310, CAST(N'2026-02-04T21:59:40.973' AS DateTime), CAST(N'2026-02-04T21:59:40.973' AS DateTime), CAST(50.00 AS Decimal(5, 2)), CAST(500000.00 AS Decimal(18, 2))),
(20, 3, N'Phòng trọ ban công rộng tại Hạ Bằng', N'Ban công view thoáng, trồng cây thoải mái.', N'phong_tro', N'Xóm 4', N'Hạ Bằng', N'Thạch Thất', N'Hà Nội', CAST(26.00 AS Decimal(6, 2)), CAST(2400000.00 AS Decimal(12, 2)), CAST(2400000.00 AS Decimal(12, 2)), 2, CAST(N'2026-02-04' AS Date), N'available', 145, CAST(N'2026-02-04T21:59:40.973' AS DateTime), CAST(N'2026-02-04T21:59:40.973' AS DateTime), CAST(50.00 AS Decimal(5, 2)), CAST(500000.00 AS Decimal(18, 2)));

SET IDENTITY_INSERT [dbo].[Properties] OFF;
GO

-- =====================================================================
-- Seed: PropertyImages (Sample data)
-- =====================================================================
SET IDENTITY_INSERT [dbo].[PropertyImages] ON;

INSERT [dbo].[PropertyImages] ([image_id], [property_id], [image_url], [is_primary], [display_order], [uploaded_at]) VALUES
(1, 1, N'https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?w=800&q=80', 1, 0, CAST(N'2026-02-04T21:59:40.987' AS DateTime)),
(2, 3, N'https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?w=800&q=80', 1, 0, CAST(N'2026-02-04T21:59:40.987' AS DateTime)),
(9, 1, N'https://images.unsplash.com/photo-1502672260266-1c1ef2d93688?w=800&q=80', 0, 1, CAST(N'2026-02-04T21:59:40.993' AS DateTime)),
(17, 2, N'https://images.unsplash.com/photo-1560448204-e02f11c3d0e2?w=800&q=80', 1, 0, CAST(N'2026-02-04T21:59:41.000' AS DateTime)),
(20, 2, N'https://images.unsplash.com/photo-1522771739844-6a9f6d5f14af?w=800&q=80', 0, 1, CAST(N'2026-02-04T21:59:41.003' AS DateTime));

SET IDENTITY_INSERT [dbo].[PropertyImages] OFF;
GO

-- =====================================================================
-- Seed: Bookings
-- =====================================================================
SET IDENTITY_INSERT [dbo].[Bookings] ON;

INSERT [dbo].[Bookings] ([booking_id], [property_id], [renter_id], [booking_date], [booking_time], [status], [notes], [created_at], [updated_at]) VALUES
(1, 3, 4, CAST(N'2026-02-12' AS Date), CAST(N'23:21:00' AS Time), N'Pending', N'afsdf', CAST(N'2026-02-04T23:21:37.737' AS DateTime), CAST(N'2026-02-04T23:21:37.737' AS DateTime)),
(2, 3, 4, CAST(N'2026-02-11' AS Date), CAST(N'23:24:00' AS Time), N'Pending', N'fasdf', CAST(N'2026-02-04T23:24:49.540' AS DateTime), CAST(N'2026-02-04T23:24:49.540' AS DateTime)),
(3, 3, 4, CAST(N'2026-02-04' AS Date), CAST(N'23:26:00' AS Time), N'Pending', N'sfadsfa', CAST(N'2026-02-04T23:26:58.093' AS DateTime), CAST(N'2026-02-04T23:26:58.093' AS DateTime)),
(4, 3, 4, CAST(N'2026-02-05' AS Date), CAST(N'00:52:00' AS Time), N'Pending', N'', CAST(N'2026-02-05T00:52:22.370' AS DateTime), CAST(N'2026-02-05T00:52:22.370' AS DateTime)),
(5, 3, 4, CAST(N'2026-02-05' AS Date), CAST(N'00:53:00' AS Time), N'Cancelled', N'', CAST(N'2026-02-05T00:53:43.670' AS DateTime), CAST(N'2026-02-05T00:53:50.900' AS DateTime)),
(6, 3, 4, CAST(N'2026-02-05' AS Date), CAST(N'00:55:00' AS Time), N'Confirmed', N'', CAST(N'2026-02-05T00:55:38.937' AS DateTime), CAST(N'2026-02-05T00:56:30.660' AS DateTime)),
(7, 3, 4, CAST(N'2026-02-10' AS Date), CAST(N'22:27:00' AS Time), N'Confirmed', N'Đặt phòng bởi Đan Duy', CAST(N'2026-02-05T22:28:14.323' AS DateTime), CAST(N'2026-02-05T22:28:38.777' AS DateTime));

SET IDENTITY_INSERT [dbo].[Bookings] OFF;
GO

-- =====================================================================
-- Seed: Payments
-- =====================================================================
SET IDENTITY_INSERT [dbo].[Payments] ON;

INSERT [dbo].[Payments] ([payment_id], [payer_user_id], [landlord_id], [rental_id], [invoice_id], [subscription_id], [payment_type], [amount], [provider], [provider_order_code], [pay_url], [status], [created_at], [paid_at], [raw_webhook], [platform_commission], [landlord_amount], [user_discount_applied]) VALUES
(1, 4, 1, NULL, NULL, NULL, N'Deposit', CAST(1800000.00 AS Decimal(14, 2)), N'PayOS', N'371137', N'https://pay.payos.vn/web/6f53b79a4a4840168a8eef5dcd218861', N'Pending', CAST(N'2026-02-04T23:38:43.560' AS DateTime), NULL, NULL, NULL, NULL, NULL),
(2, 4, 1, NULL, NULL, NULL, N'Deposit', CAST(1800000.00 AS Decimal(14, 2)), N'PayOS', N'753887', N'https://pay.payos.vn/web/3f8daf377b384bb2a9303cb80ea25f0b', N'Pending', CAST(N'2026-02-05T00:19:08.550' AS DateTime), NULL, NULL, NULL, NULL, NULL),
(3, 4, 1, NULL, NULL, NULL, N'Deposit', CAST(1300000.00 AS Decimal(14, 2)), N'PayOS', N'779792', N'https://pay.payos.vn/web/cf0c33c8d7294b729cf04d4235e6492f', N'Pending', CAST(N'2026-02-05T00:52:23.657' AS DateTime), NULL, NULL, NULL, NULL, NULL),
(4, 4, 1, NULL, NULL, NULL, N'Deposit', CAST(1300000.00 AS Decimal(14, 2)), N'PayOS', N'684021', N'https://pay.payos.vn/web/b75abc8460534153b34010b1efcb56bc', N'Cancelled', CAST(N'2026-02-05T00:53:44.437' AS DateTime), NULL, NULL, NULL, NULL, NULL),
(5, 4, 1, NULL, NULL, NULL, N'Deposit', CAST(1300000.00 AS Decimal(14, 2)), N'PayOS', N'952960', N'https://pay.payos.vn/web/3dadaaeefbeb4ec6b9c287726ca3c6bd', N'Completed', CAST(N'2026-02-05T00:55:39.477' AS DateTime), CAST(N'2026-02-05T00:56:30.677' AS DateTime), NULL, NULL, NULL, NULL),
(6, 4, 1, NULL, NULL, NULL, N'Deposit', CAST(1300000.00 AS Decimal(14, 2)), N'PayOS', N'451013', N'https://pay.payos.vn/web/0644897eedbb40ea8a6673ec1f05823f', N'Completed', CAST(N'2026-02-05T22:28:15.730' AS DateTime), CAST(N'2026-02-05T22:28:38.797' AS DateTime), NULL, NULL, NULL, NULL);

SET IDENTITY_INSERT [dbo].[Payments] OFF;
GO

-- =====================================================================
-- Seed: Wallets
-- =====================================================================
SET IDENTITY_INSERT [dbo].[Wallets] ON;

INSERT [dbo].[Wallets] ([wallet_id], [landlord_id], [locked_balance], [available_balance], [currency], [created_at], [updated_at]) VALUES
(1, 5, CAST(0.00 AS Decimal(14, 2)), CAST(470000.00 AS Decimal(14, 2)), N'VND', CAST(N'2026-02-05T13:14:38.710' AS DateTime), CAST(N'2026-02-05T21:19:51.687' AS DateTime)),
(2, 1, CAST(1800000.00 AS Decimal(14, 2)), CAST(0.00 AS Decimal(14, 2)), N'VND', CAST(N'2026-02-05T14:40:52.527' AS DateTime), CAST(N'2026-02-05T22:28:38.900' AS DateTime)),
(3, 4, CAST(0.00 AS Decimal(14, 2)), CAST(0.00 AS Decimal(14, 2)), N'VND', CAST(N'2026-02-05T22:27:52.697' AS DateTime), CAST(N'2026-02-05T22:27:52.697' AS DateTime));

SET IDENTITY_INSERT [dbo].[Wallets] OFF;
GO

-- =====================================================================
-- Seed: WalletTransactions
-- =====================================================================
SET IDENTITY_INSERT [dbo].[WalletTransactions] ON;

INSERT [dbo].[WalletTransactions] ([wallet_txn_id], [wallet_id], [direction], [amount], [related_type], [related_id], [status], [note], [created_at]) VALUES
(1, 1, N'out', CAST(30000.00 AS Decimal(14, 2)), N'withdraw_request', 0, N'pending', N'Yêu cầu rút tiền - BIDV 4110425662', CAST(N'2026-02-05T21:19:51.687' AS DateTime)),
(2, 2, N'in', CAST(1800000.00 AS Decimal(14, 2)), N'booking', 7, N'locked', N'Đặt cọc booking #7 - Phòng trọ giá rẻ sinh viên tại Bình Yên', CAST(N'2026-02-05T22:28:38.907' AS DateTime));

SET IDENTITY_INSERT [dbo].[WalletTransactions] OFF;
GO

-- =====================================================================
-- Seed: WithdrawRequests
-- =====================================================================
SET IDENTITY_INSERT [dbo].[WithdrawRequests] ON;

INSERT [dbo].[WithdrawRequests] ([withdraw_id], [wallet_id], [landlord_id], [amount], [bank_name], [bank_account], [account_holder], [status], [requested_at], [processed_at], [processed_by_admin_id], [note]) VALUES
(1, 1, 5, CAST(30000.00 AS Decimal(14, 2)), N'BIDV', N'4110425662', N'LUONG DAN DUY', N'Pending', CAST(N'2026-02-05T21:19:51.590' AS DateTime), NULL, NULL, N'rut tien');

SET IDENTITY_INSERT [dbo].[WithdrawRequests] OFF;
GO

PRINT N'Seed data inserted successfully.';
GO

