-- =====================================================================
-- Insert Sample Data for Rentals Table
-- =====================================================================

USE [NestFlowSystem]
GO

-- Kiểm tra xem có user và property nào không
DECLARE @LandlordId BIGINT;
DECLARE @RenterId BIGINT;
DECLARE @PropertyId BIGINT;

-- Lấy landlord đầu tiên (user type = Landlord)
SELECT TOP 1 @LandlordId = user_id FROM Users WHERE user_type = 'Landlord';

-- Lấy renter đầu tiên (user type = Renter)
SELECT TOP 1 @RenterId = user_id FROM Users WHERE user_type = 'Renter';

-- Lấy property đầu tiên
SELECT TOP 1 @PropertyId = property_id FROM Properties WHERE landlord_id = @LandlordId;

-- Kiểm tra nếu không có dữ liệu cần thiết
IF @LandlordId IS NULL OR @RenterId IS NULL OR @PropertyId IS NULL
BEGIN
    PRINT N'❌ Không tìm thấy dữ liệu cần thiết (Landlord, Renter, hoặc Property)';
    PRINT N'Vui lòng đảm bảo có ít nhất:';
    PRINT N'  - 1 User với user_type = ''Landlord''';
    PRINT N'  - 1 User với user_type = ''Renter''';
    PRINT N'  - 1 Property thuộc về Landlord';
    RETURN;
END

PRINT N'✓ Tìm thấy dữ liệu:';
PRINT N'  LandlordId: ' + CAST(@LandlordId AS NVARCHAR(10));
PRINT N'  RenterId: ' + CAST(@RenterId AS NVARCHAR(10));
PRINT N'  PropertyId: ' + CAST(@PropertyId AS NVARCHAR(10));

-- Xóa dữ liệu cũ nếu có (để test)
-- DELETE FROM Rentals WHERE rental_id > 0;
-- DBCC CHECKIDENT ('[Rentals]', RESEED, 0);

-- Insert sample rentals
DECLARE @StartDate DATE = DATEADD(MONTH, -3, GETDATE()); -- Bắt đầu từ 3 tháng trước
DECLARE @EndDate DATE = DATEADD(MONTH, 9, GETDATE()); -- Kết thúc sau 9 tháng (tổng 1 năm)
DECLARE @MonthlyRent DECIMAL(12,2);

-- Lấy giá thuê từ property
SELECT @MonthlyRent = price FROM Properties WHERE property_id = @PropertyId;

-- Insert rental
INSERT INTO [dbo].[Rentals] 
    ([landlord_id], [property_id], [renter_id], [start_date], [end_date], 
     [monthly_rent], [deposit_amount], [status], [created_at], [updated_at])
VALUES
    (@LandlordId, @PropertyId, @RenterId, @StartDate, @EndDate,
     @MonthlyRent, @MonthlyRent * 2, 'active', GETDATE(), GETDATE());

DECLARE @RentalId BIGINT = SCOPE_IDENTITY();

PRINT N'✓ Đã tạo Rental với ID: ' + CAST(@RentalId AS NVARCHAR(10));

-- Verify data
SELECT 
    r.rental_id,
    r.landlord_id,
    l.full_name AS landlord_name,
    r.property_id,
    p.title AS property_title,
    r.renter_id,
    u.full_name AS renter_name,
    r.start_date,
    r.end_date,
    r.monthly_rent,
    r.status
FROM Rentals r
INNER JOIN Users l ON r.landlord_id = l.user_id
INNER JOIN Properties p ON r.property_id = p.property_id
INNER JOIN Users u ON r.renter_id = u.user_id
WHERE r.rental_id = @RentalId;

PRINT N'✓ Rentals data inserted successfully!';
GO
