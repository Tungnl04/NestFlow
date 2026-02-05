-- Tạo bảng Payments nếu chưa tồn tại
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Payments]') AND type in (N'U'))
BEGIN
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
        CONSTRAINT [PK__Payments__ED1FC9EACC3DBF5D] PRIMARY KEY CLUSTERED ([payment_id] ASC)
    )

    -- Tạo indexes
    CREATE NONCLUSTERED INDEX [ix_payments_payer] ON [dbo].[Payments]([payer_user_id] ASC)
    CREATE NONCLUSTERED INDEX [ix_payments_provider_order_code] ON [dbo].[Payments]([provider_order_code] ASC)
    CREATE NONCLUSTERED INDEX [ix_payments_type_status] ON [dbo].[Payments]([payment_type] ASC, [status] ASC)

    -- Tạo foreign keys
    ALTER TABLE [dbo].[Payments] WITH CHECK ADD CONSTRAINT [fk_payments_payer] 
        FOREIGN KEY([payer_user_id]) REFERENCES [dbo].[Users] ([user_id])
    
    ALTER TABLE [dbo].[Payments] WITH CHECK ADD CONSTRAINT [fk_payments_landlord] 
        FOREIGN KEY([landlord_id]) REFERENCES [dbo].[Users] ([user_id])


