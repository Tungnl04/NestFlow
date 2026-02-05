-- Create WithdrawRequests table if not exists
USE NestFlowSystem;
GO

-- Check if table exists
IF OBJECT_ID('dbo.WithdrawRequests', 'U') IS NULL
BEGIN
    PRINT 'Creating WithdrawRequests table...';
    
    CREATE TABLE dbo.WithdrawRequests (
        withdraw_id BIGINT IDENTITY(1,1) PRIMARY KEY,
        wallet_id BIGINT NOT NULL,
        landlord_id BIGINT NOT NULL,
        
        amount DECIMAL(14,2) NOT NULL,
        
        bank_name NVARCHAR(100) NOT NULL,
        bank_account NVARCHAR(50) NOT NULL,
        account_holder NVARCHAR(100) NOT NULL,
        
        status NVARCHAR(20) NOT NULL DEFAULT 'Pending'
            CHECK (status IN ('Pending','Approved','Rejected','Completed')),
        
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
    
    PRINT 'WithdrawRequests table created successfully!';
END
ELSE
BEGIN
    PRINT 'WithdrawRequests table already exists.';
END
GO

-- Verify table structure
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'WithdrawRequests'
ORDER BY ORDINAL_POSITION;
GO

PRINT 'Script completed!';
