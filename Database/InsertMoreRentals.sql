-- =====================================================================
-- Insert More Sample Rentals - Full Script for SSMS
-- Tạo thêm nhiều hợp đồng thuê mẫu
-- =====================================================================

USE [NestFlowSystem]
GO

PRINT N'=== BẮT ĐẦU INSERT RENTALS ===';
PRINT N'';

-- Lấy thông tin cần thiết
DECLARE @LandlordId BIGINT;
DECLARE @PropertyCount INT;
DECLARE @RenterCount INT;

SELECT TOP 1 @LandlordId = user_id FROM Users WHERE user_type = 'Landlord';
SELECT @PropertyCount = COUNT(*) FROM Properties WHERE landlord_id = @LandlordId;
SELECT @RenterCount = COUNT(*) FROM Users WHERE user_type = 'Renter';

PRINT N'Landlord ID: ' + CAST(@LandlordId AS NVARCHAR(10));
PRINT N'Số Properties: ' + CAST(@PropertyCount AS NVARCHAR(10));
PRINT N'Số Renters: ' + CAST(@RenterCount AS NVARCHAR(10));
PRINT N'';

-- Nếu không có renter, tạo thêm
IF @RenterCount < 5
BEGIN
    PRINT N'Đang tạo thêm Renters...';
    
    INSERT INTO [Users] ([email], [password_hash], [full_name], [phone], [user_type], [is_verified], [status], [created_at], [updated_at])
    VALUES
    ('renter1@example.com', '$2a$11$hashedpassword1', N'Trần Văn B', '0912345671', 'Renter', 1, 'Active', GETDATE(), GETDATE()),
    ('renter2@example.com', '$2a$11$hashedpassword2', N'Lê Thị C', '0912345672', 'Renter', 1, 'Active', GETDATE(), GETDATE()),
    ('renter3@example.com', '$2a$11$hashedpassword3', N'Phạm Văn D', '0912345673', 'Renter', 1, 'Active', GETDATE(), GETDATE()),
    ('renter4@example.com', '$2a$11$hashedpassword4', N'Hoàng Thị E', '0912345674', 'Renter', 1, 'Active', GETDATE(), GETDATE()),
    ('renter5@example.com', '$2a$11$hashedpassword5', N'Ngô Văn F', '0912345675', 'Renter', 1, 'Active', GETDATE(), GETDATE());
    
    PRINT N'✓ Đã tạo thêm 5 Renters';
    PRINT N'';
END

-- Nếu không có property, tạo thêm
IF @PropertyCount < 5
BEGIN
    PRINT N'Đang tạo thêm Properties...';
    
    INSERT INTO [Properties] 
    ([landlord_id], [title], [description], [property_type], [address], [ward], [district], [city], 
     [price], [area], [max_occupants], [deposit], [available_from], [status], [created_at], [updated_at])
    VALUES
    (@LandlordId, N'Phòng trọ cao cấp quận 1', N'Phòng đẹp, đầy đủ tiện nghi', 'Room', N'123 Nguyễn Huệ', N'Bến Nghé', N'Quận 1', N'TP.HCM', 3500000, 25, 2, 7000000, GETDATE(), 'Available', GETDATE(), GETDATE()),
    (@LandlordId, N'Căn hộ mini Bình Thạnh', N'Căn hộ mini tiện nghi', 'Apartment', N'456 Xô Viết Nghệ Tĩnh', N'Phường 25', N'Bình Thạnh', N'TP.HCM', 4500000, 30, 2, 9000000, GETDATE(), 'Available', GETDATE(), GETDATE()),
    (@LandlordId, N'Phòng trọ sinh viên Thủ Đức', N'Phòng giá rẻ cho sinh viên', 'Room', N'789 Võ Văn Ngân', N'Linh Chiểu', N'Thủ Đức', N'TP.HCM', 2000000, 20, 2, 4000000, GETDATE(), 'Available', GETDATE(), GETDATE()),
    (@LandlordId, N'Nhà nguyên căn Gò Vấp', N'Nhà 1 trệt 1 lầu', 'House', N'321 Quang Trung', N'Phường 10', N'Gò Vấp', N'TP.HCM', 8000000, 80, 4, 16000000, GETDATE(), 'Available', GETDATE(), GETDATE()),
    (@LandlordId, N'Studio Quận 7', N'Studio hiện đại, view đẹp', 'Studio', N'654 Nguyễn Thị Thập', N'Tân Phú', N'Quận 7', N'TP.HCM', 5500000, 35, 2, 11000000, GETDATE(), 'Available', GETDATE(), GETDATE());
    
    PRINT N'✓ Đã tạo thêm 5 Properties';
    PRINT N'';
END

-- Lấy lại số lượng sau khi insert
SELECT @PropertyCount = COUNT(*) FROM Properties WHERE landlord_id = @LandlordId;
SELECT @RenterCount = COUNT(*) FROM Users WHERE user_type = 'Renter';

PRINT N'Chuẩn bị tạo Rentals...';
PRINT N'';

-- Tạo bảng tạm để lưu properties và renters
DECLARE @TempProperties TABLE (RowNum INT, PropertyId BIGINT, Title NVARCHAR(255), Price DECIMAL(12,2));
DECLARE @TempRenters TABLE (RowNum INT, RenterId BIGINT, RenterName NVARCHAR(255));

INSERT INTO @TempProperties
SELECT ROW_NUMBER() OVER (ORDER BY property_id), property_id, title, price
FROM Properties WHERE landlord_id = @LandlordId;

INSERT INTO @TempRenters
SELECT ROW_NUMBER() OVER (ORDER BY user_id), user_id, full_name
FROM Users WHERE user_type = 'Renter';

-- Insert rentals
DECLARE @i INT = 1;
DECLARE @MaxRentals INT = CASE WHEN @PropertyCount < @RenterCount THEN @PropertyCount ELSE @RenterCount END;
DECLARE @PropertyId BIGINT;
DECLARE @RenterId BIGINT;
DECLARE @Title NVARCHAR(255);
DECLARE @RenterName NVARCHAR(255);
DECLARE @Price DECIMAL(12,2);
DECLARE @StartDate DATE;
DECLARE @EndDate DATE;
DECLARE @InsertedCount INT = 0;

WHILE @i <= @MaxRentals
BEGIN
    -- Lấy property
    SELECT @PropertyId = PropertyId, @Title = Title, @Price = Price
    FROM @TempProperties WHERE RowNum = @i;
    
    -- Lấy renter
    SELECT @RenterId = RenterId, @RenterName = RenterName
    FROM @TempRenters WHERE RowNum = @i;
    
    -- Kiểm tra xem property đã có rental active chưa
    IF NOT EXISTS (SELECT 1 FROM Rentals WHERE property_id = @PropertyId AND status = 'active')
    BEGIN
        -- Random start date trong 3 tháng qua
        SET @StartDate = DATEADD(DAY, -(@i * 10), GETDATE());
        SET @EndDate = DATEADD(MONTH, 12, @StartDate);
        
        INSERT INTO [Rentals]
        ([landlord_id], [property_id], [renter_id], [start_date], [end_date], 
         [monthly_rent], [deposit_amount], [status], [created_at], [updated_at])
        VALUES
        (@LandlordId, @PropertyId, @RenterId, @StartDate, @EndDate,
         @Price, @Price * 2, 'active', GETDATE(), GETDATE());
        
        SET @InsertedCount = @InsertedCount + 1;
        PRINT N'✓ Rental #' + CAST(@InsertedCount AS NVARCHAR(10)) + N': ' + @Title + N' → ' + @RenterName;
    END
    
    SET @i = @i + 1;
END

PRINT N'';
PRINT N'=== KẾT QUẢ ===';
PRINT N'Đã tạo: ' + CAST(@InsertedCount AS NVARCHAR(10)) + N' rentals';
PRINT N'';

-- Hiển thị tất cả rentals
SELECT 
    r.rental_id AS [Mã HĐ],
    l.full_name AS [Chủ nhà],
    p.title AS [Phòng],
    u.full_name AS [Người thuê],
    CONVERT(VARCHAR(10), r.start_date, 103) AS [Từ ngày],
    CONVERT(VARCHAR(10), r.end_date, 103) AS [Đến ngày],
    FORMAT(r.monthly_rent, 'N0') AS [Giá thuê/tháng],
    r.status AS [Trạng thái]
FROM Rentals r
INNER JOIN Users l ON r.landlord_id = l.user_id
INNER JOIN Properties p ON r.property_id = p.property_id
INNER JOIN Users u ON r.renter_id = u.user_id
ORDER BY r.rental_id;

DECLARE @TotalRentals INT = (SELECT COUNT(*) FROM Rentals);
PRINT N'';
PRINT N'✓ Tổng số Rentals trong hệ thống: ' + CAST(@TotalRentals AS NVARCHAR(10));
PRINT N'✓ HOÀN TẤT!';
GO

