-- Check if commission columns exist

PRINT '=== CHECKING COMMISSION COLUMNS ===';
PRINT '';

-- Check Properties table
PRINT 'Properties table columns:';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Properties'
  AND COLUMN_NAME IN ('commission_rate', 'user_discount')
ORDER BY ORDINAL_POSITION;

IF @@ROWCOUNT = 0
BEGIN
    PRINT '❌ Commission columns NOT found in Properties table';
END
ELSE
BEGIN
    PRINT '✅ Commission columns found in Properties table';
END

PRINT '';

-- Check Payments table
PRINT 'Payments table columns:';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Payments'
  AND COLUMN_NAME IN ('platform_commission', 'landlord_amount', 'user_discount_applied')
ORDER BY ORDINAL_POSITION;

IF @@ROWCOUNT = 0
BEGIN
    PRINT '❌ Commission columns NOT found in Payments table';
END
ELSE
BEGIN
    PRINT '✅ Commission columns found in Payments table';
END

PRINT '';
PRINT '=== CHECK COMPLETE ===';
