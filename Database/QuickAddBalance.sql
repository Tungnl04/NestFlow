-- QUICK FIX: Thêm số dư nhanh cho Landlord hiện tại đang đăng nhập

-- Cách 1: Nếu biết LandlordId
-- Thay [YOUR_LANDLORD_ID] bằng UserId của bạn
UPDATE Wallets
SET AvailableBalance = 500000,
    LockedBalance = 100000,
    UpdatedAt = GETDATE()
WHERE LandlordId = 1; -- THAY ĐỔI SỐ NÀY

-- Cách 2: Nếu biết Email
UPDATE Wallets
SET AvailableBalance = 500000,
    LockedBalance = 100000,
    UpdatedAt = GETDATE()
WHERE LandlordId IN (
    SELECT UserId FROM Users 
    WHERE Email = 'landlord@example.com' -- THAY ĐỔI EMAIL NÀY
    AND UserType = 'Landlord'
);

-- Kiểm tra kết quả
SELECT 
    u.UserId,
    u.FullName,
    u.Email,
    w.AvailableBalance,
    w.LockedBalance,
    (w.AvailableBalance + w.LockedBalance) AS TotalBalance
FROM Users u
INNER JOIN Wallets w ON u.UserId = w.LandlordId
WHERE u.UserType = 'Landlord';
