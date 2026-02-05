-- Add Commission System to Properties

PRINT '=== ADDING COMMISSION SYSTEM ===';
PRINT '';

-- 1. Thêm cột CommissionRate vào Properties
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Properties') AND name = 'commission_rate')
BEGIN
    PRINT 'Adding commission_rate column...';
    ALTER TABLE Properties
    ADD commission_rate DECIMAL(5,2) NULL DEFAULT 50.00;
    PRINT '✅ Added commission_rate column (default 50%)';
END
ELSE
BEGIN
    PRINT '⚠️ commission_rate column already exists';
END

PRINT '';

-- 2. Thêm cột UserDiscount vào Properties (discount cho user từ commission)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Properties') AND name = 'user_discount')
BEGIN
    PRINT 'Adding user_discount column...';
    ALTER TABLE Properties
    ADD user_discount DECIMAL(18,2) NULL DEFAULT 500000.00;
    PRINT '✅ Added user_discount column (default 500,000 VNĐ)';
END
ELSE
BEGIN
    PRINT '⚠️ user_discount column already exists';
END

PRINT '';

-- 3. Update existing properties với giá trị mặc định
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Properties') AND name = 'commission_rate')
BEGIN
    PRINT 'Updating existing properties...';
    UPDATE Properties
    SET commission_rate = 50.00,
        user_discount = 500000.00
    WHERE commission_rate IS NULL OR user_discount IS NULL;
    
    PRINT '✅ Updated existing properties';
END
ELSE
BEGIN
    PRINT '⚠️ Skipping update - columns not created yet';
END

PRINT '';

-- 4. Thêm cột vào Payments để track commission
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Payments') AND name = 'platform_commission')
BEGIN
    PRINT 'Adding platform_commission column to Payments...';
    ALTER TABLE Payments
    ADD platform_commission DECIMAL(18,2) NULL DEFAULT 0.00;
    PRINT '✅ Added platform_commission column';
END
ELSE
BEGIN
    PRINT '⚠️ platform_commission column already exists';
END

PRINT '';

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Payments') AND name = 'landlord_amount')
BEGIN
    PRINT 'Adding landlord_amount column to Payments...';
    ALTER TABLE Payments
    ADD landlord_amount DECIMAL(18,2) NULL DEFAULT 0.00;
    PRINT '✅ Added landlord_amount column';
END
ELSE
BEGIN
    PRINT '⚠️ landlord_amount column already exists';
END

PRINT '';

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Payments') AND name = 'user_discount_applied')
BEGIN
    PRINT 'Adding user_discount_applied column to Payments...';
    ALTER TABLE Payments
    ADD user_discount_applied DECIMAL(18,2) NULL DEFAULT 0.00;
    PRINT '✅ Added user_discount_applied column';
END
ELSE
BEGIN
    PRINT '⚠️ user_discount_applied column already exists';
END

PRINT '';
PRINT '=== COMMISSION SYSTEM SETUP COMPLETE ===';
PRINT '';

-- 5. Hiển thị kết quả
PRINT 'Sample calculation:';
PRINT '-------------------';
PRINT 'Deposit gốc:        2,000,000 VNĐ';
PRINT 'Commission rate:    50%';
PRINT 'Commission amount:  1,000,000 VNĐ';
PRINT 'User discount:      500,000 VNĐ (từ commission)';
PRINT '';
PRINT 'User trả:           1,500,000 VNĐ';
PRINT 'Landlord nhận:      1,000,000 VNĐ';
PRINT 'Platform giữ:       500,000 VNĐ';
PRINT '';

-- 6. Verify schema
SELECT 
    'Properties' AS TableName,
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Properties'
  AND COLUMN_NAME IN ('commission_rate', 'user_discount')
ORDER BY ORDINAL_POSITION;

PRINT '';

SELECT 
    'Payments' AS TableName,
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Payments'
  AND COLUMN_NAME IN ('platform_commission', 'landlord_amount', 'user_discount_applied')
ORDER BY ORDINAL_POSITION;

PRINT '';
PRINT '✅ DONE! Now update your C# models and code.';
