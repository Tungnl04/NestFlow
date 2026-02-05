-- Script to create wallets for existing users who don't have one yet
-- Run this after updating the registration code

USE NestFlowSystem;
GO

-- Create wallets for all users who don't have one
INSERT INTO Wallets (LandlordId, AvailableBalance, LockedBalance, Currency, CreatedAt, UpdatedAt)
SELECT 
    u.UserId,
    0 as AvailableBalance,
    0 as LockedBalance,
    'VND' as Currency,
    GETDATE() as CreatedAt,
    GETDATE() as UpdatedAt
FROM Users u
LEFT JOIN Wallets w ON u.UserId = w.LandlordId
WHERE w.WalletId IS NULL;

-- Verify wallets created
SELECT 
    u.UserId,
    u.FullName,
    u.Email,
    u.UserType,
    w.WalletId,
    w.AvailableBalance,
    w.LockedBalance
FROM Users u
LEFT JOIN Wallets w ON u.UserId = w.LandlordId
ORDER BY u.UserId;

-- Count users with and without wallets
SELECT 
    'Total Users' as Category,
    COUNT(*) as Count
FROM Users
UNION ALL
SELECT 
    'Users with Wallet' as Category,
    COUNT(*) as Count
FROM Users u
INNER JOIN Wallets w ON u.UserId = w.LandlordId
UNION ALL
SELECT 
    'Users without Wallet' as Category,
    COUNT(*) as Count
FROM Users u
LEFT JOIN Wallets w ON u.UserId = w.LandlordId
WHERE w.WalletId IS NULL;

PRINT 'Wallets created successfully for all existing users!';
