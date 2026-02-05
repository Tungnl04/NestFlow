-- Script: Fix vấn đề "Lỗi xử lý ví" khi approve booking

PRINT '=== KIỂM TRA VÀ FIX BOOKING WALLET ISSUE ===';
PRINT '';

-- 1. Kiểm tra booking cần approve
PRINT '1. Kiểm tra booking Pending...';
SELECT 
    b.BookingId,
    b.PropertyId,
    p.Title AS PropertyTitle,
    p.LandlordId,
    b.RenterId,
    b.Status,
    b.BookingDate,
    b.CreatedAt
FROM Bookings b
INNER JOIN Properties p ON b.PropertyId = p.PropertyId
WHERE b.Status = 'Pending'
ORDER BY b.CreatedAt DESC;

PRINT '';

-- 2. Kiểm tra payment tương ứng
PRINT '2. Kiểm tra payment...';
SELECT 
    p.PaymentId,
    p.PayerUserId AS RenterId,
    p.LandlordId,
    p.Amount,
    p.PaymentType,
    p.Status,
    p.CreatedAt
FROM Payments p
WHERE p.PaymentType = 'Deposit'
  AND p.Status = 'Completed'
ORDER BY p.CreatedAt DESC;

PRINT '';

-- 3. Kiểm tra wallet của landlord
PRINT '3. Kiểm tra wallet landlord...';
SELECT 
    w.WalletId,
    w.LandlordId,
    u.FullName,
    w.AvailableBalance,
    w.LockedBalance,
    (w.AvailableBalance + w.LockedBalance) AS TotalBalance
FROM Wallets w
INNER JOIN Users u ON w.LandlordId = u.UserId
WHERE u.UserType = 'Landlord';

PRINT '';

-- 4. Kiểm tra transactions
PRINT '4. Kiểm tra wallet transactions...';
SELECT 
    wt.WalletTxnId,
    wt.WalletId,
    wt.Direction,
    wt.Amount,
    wt.RelatedType,
    wt.RelatedId,
    wt.Status,
    wt.Note,
    wt.CreatedAt
FROM WalletTransactions wt
ORDER BY wt.CreatedAt DESC;

PRINT '';
PRINT '=== PHÂN TÍCH VẤN ĐỀ ===';
PRINT '';

-- Tìm booking cần fix
DECLARE @BookingId BIGINT;
DECLARE @LandlordId BIGINT;
DECLARE @RenterId BIGINT;
DECLARE @Amount DECIMAL(18,2);

-- Lấy booking pending đầu tiên
SELECT TOP 1 
    @BookingId = b.BookingId,
    @LandlordId = p.LandlordId,
    @RenterId = b.RenterId
FROM Bookings b
INNER JOIN Properties p ON b.PropertyId = p.PropertyId
WHERE b.Status = 'Pending'
ORDER BY b.CreatedAt DESC;

IF @BookingId IS NULL
BEGIN
    PRINT '❌ Không có booking Pending nào!';
    RETURN;
END

PRINT 'Booking cần approve:';
PRINT '  BookingId: ' + CAST(@BookingId AS VARCHAR);
PRINT '  LandlordId: ' + CAST(@LandlordId AS VARCHAR);
PRINT '  RenterId: ' + CAST(@RenterId AS VARCHAR);
PRINT '';

-- Tìm payment
SELECT TOP 1 @Amount = Amount
FROM Payments
WHERE PaymentType = 'Deposit'
  AND Status = 'Completed'
  AND PayerUserId = @RenterId
  AND LandlordId = @LandlordId
ORDER BY CreatedAt DESC;

IF @Amount IS NULL
BEGIN
    PRINT '❌ Không tìm thấy payment!';
    PRINT '→ Cần tạo payment test';
    RETURN;
END

PRINT 'Payment found:';
PRINT '  Amount: ' + CAST(@Amount AS VARCHAR) + ' VNĐ';
PRINT '';

-- Kiểm tra wallet
DECLARE @WalletId BIGINT;
DECLARE @LockedBalance DECIMAL(18,2);

SELECT 
    @WalletId = WalletId,
    @LockedBalance = LockedBalance
FROM Wallets
WHERE LandlordId = @LandlordId;

IF @WalletId IS NULL
BEGIN
    PRINT '❌ Landlord chưa có ví!';
    PRINT '→ Đang tạo ví...';
    
    INSERT INTO Wallets (LandlordId, AvailableBalance, LockedBalance, Currency, CreatedAt, UpdatedAt)
    VALUES (@LandlordId, 0, 0, 'VND', GETDATE(), GETDATE());
    
    SELECT @WalletId = SCOPE_IDENTITY();
    SET @LockedBalance = 0;
    
    PRINT '✅ Đã tạo ví: WalletId = ' + CAST(@WalletId AS VARCHAR);
END

PRINT 'Wallet info:';
PRINT '  WalletId: ' + CAST(@WalletId AS VARCHAR);
PRINT '  LockedBalance: ' + CAST(@LockedBalance AS VARCHAR) + ' VNĐ';
PRINT '';

-- Kiểm tra locked balance
IF @LockedBalance < @Amount
BEGIN
    PRINT '❌ VẤN ĐỀ: LockedBalance không đủ!';
    PRINT '  Cần: ' + CAST(@Amount AS VARCHAR) + ' VNĐ';
    PRINT '  Có: ' + CAST(@LockedBalance AS VARCHAR) + ' VNĐ';
    PRINT '  Thiếu: ' + CAST(@Amount - @LockedBalance AS VARCHAR) + ' VNĐ';
    PRINT '';
    PRINT '→ Đang fix bằng cách thêm locked balance...';
    
    -- Thêm locked balance
    UPDATE Wallets
    SET LockedBalance = @Amount,
        UpdatedAt = GETDATE()
    WHERE WalletId = @WalletId;
    
    -- Tạo transaction log
    INSERT INTO WalletTransactions (WalletId, Direction, Amount, RelatedType, RelatedId, Status, Note, CreatedAt)
    VALUES (@WalletId, 'in', @Amount, 'booking', @BookingId, 'locked', 'Fix: Thêm locked balance cho booking #' + CAST(@BookingId AS VARCHAR), GETDATE());
    
    PRINT '✅ Đã thêm ' + CAST(@Amount AS VARCHAR) + ' VNĐ vào LockedBalance';
END
ELSE
BEGIN
    PRINT '✅ LockedBalance đủ để approve!';
END

PRINT '';
PRINT '=== KẾT QUẢ SAU KHI FIX ===';
PRINT '';

-- Hiển thị kết quả
SELECT 
    'Wallet After Fix' AS [Type],
    w.WalletId,
    w.LandlordId,
    w.AvailableBalance,
    w.LockedBalance,
    (w.AvailableBalance + w.LockedBalance) AS TotalBalance
FROM Wallets w
WHERE w.LandlordId = @LandlordId;

PRINT '';
PRINT '✅ HOÀN TẤT!';
PRINT '';
PRINT 'Bây giờ bạn có thể:';
PRINT '1. Reload trang /Landlord/Bookings';
PRINT '2. Click "Chấp nhận" booking';
PRINT '3. Sẽ thành công!';
PRINT '';
PRINT 'Nếu vẫn lỗi, xem log trong Visual Studio Output';
