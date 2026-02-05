-- Kiểm tra và tạo bảng Wallets
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Wallets]') AND type in (N'U'))
BEGIN
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

    -- Tạo indexes
    CREATE NONCLUSTERED INDEX [ix_wallets_landlord] ON [dbo].[Wallets]([landlord_id] ASC)

    -- Tạo foreign key
    ALTER TABLE [dbo].[Wallets] WITH CHECK ADD CONSTRAINT [fk_wallets_landlord] 
        FOREIGN KEY([landlord_id]) REFERENCES [dbo].[Users] ([user_id])

    PRINT 'Bảng Wallets đã được tạo thành công!'
END
ELSE
BEGIN
    PRINT 'Bảng Wallets đã tồn tại!'
END
GO

-- Kiểm tra và tạo bảng WalletTransactions
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WalletTransactions]') AND type in (N'U'))
BEGIN
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
        CONSTRAINT [PK__WalletTr__3214EC07] PRIMARY KEY CLUSTERED ([wallet_txn_id] ASC)
    )

    -- Tạo indexes
    CREATE NONCLUSTERED INDEX [ix_wallettxn_wallet] ON [dbo].[WalletTransactions]([wallet_id] ASC)
    CREATE NONCLUSTERED INDEX [ix_wallettxn_related] ON [dbo].[WalletTransactions]([related_type] ASC, [related_id] ASC)
    CREATE NONCLUSTERED INDEX [ix_wallettxn_status] ON [dbo].[WalletTransactions]([status] ASC)

    -- Tạo foreign key
    ALTER TABLE [dbo].[WalletTransactions] WITH CHECK ADD CONSTRAINT [fk_wallettxn_wallet] 
        FOREIGN KEY([wallet_id]) REFERENCES [dbo].[Wallets] ([wallet_id])

    PRINT 'Bảng WalletTransactions đã được tạo thành công!'
END
ELSE
BEGIN
    PRINT 'Bảng WalletTransactions đã tồn tại!'
END
GO

PRINT 'Hoàn tất tạo bảng Wallet system!'
