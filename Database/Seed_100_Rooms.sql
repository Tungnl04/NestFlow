-- =====================================================================
-- Seed Data: 100 Sample Properties for Search Testing
-- NestFlow Project
-- =====================================================================

USE [NestFlowSystem]
GO

SET NOCOUNT ON;
BEGIN TRANSACTION;

BEGIN TRY
    PRINT N'Cleaning up existing sample properties (optional - uncomment if needed)...';
    -- DELETE FROM [dbo].[Properties] WHERE [title] LIKE N'Phòng mẫu %';
    -- DELETE FROM [dbo].[Buildings] WHERE [building_name] LIKE N'Tòa nhà mẫu %';

    -- [FIX] Update Property Type Constraint to allow new types
    PRINT N'Updating CK_Properties_Type constraint...';
    IF EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_Properties_Type')
    BEGIN
        ALTER TABLE [dbo].[Properties] DROP CONSTRAINT [CK_Properties_Type];
    END
    ALTER TABLE [dbo].[Properties] WITH CHECK ADD CONSTRAINT [CK_Properties_Type] 
    CHECK ([property_type] IN ('phong_tro', 'chung_cu', 'nha_nguyen_can', 'studio', 'o_ghep'));

    -- [FIX] Update Amenities Category Constraint
    PRINT N'Updating CK_Amenities_Category constraint...';
    IF EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_Amenities_Category')
    BEGIN
        ALTER TABLE [dbo].[Amenities] DROP CONSTRAINT [CK_Amenities_Category];
    END
    ALTER TABLE [dbo].[Amenities] WITH CHECK ADD CONSTRAINT [CK_Amenities_Category] 
    CHECK ([category] IN ('basic', 'furniture', 'security', 'Standard', 'Premium'));

    -- 1. Ensure we have some Landlords
    PRINT N'Ensuring Landlords exist...';
    IF NOT EXISTS (SELECT 1 FROM Users WHERE user_type = 'Landlord')
    BEGIN
        INSERT INTO [Users] ([email], [password_hash], [full_name], [phone], [user_type], [is_verified], [status], [created_at], [updated_at])
        VALUES 
        ('landlord_seed_1@example.com', 'hashed', N'Nguyễn Văn Chủ Thầu', '0901112223', 'Landlord', 1, 'Active', GETDATE(), GETDATE()),
        ('landlord_seed_2@example.com', 'hashed', N'Trần Thị Nhà Trọ', '0904445556', 'Landlord', 1, 'Active', GETDATE(), GETDATE());
    END

    DECLARE @Landlord1 BIGINT = (SELECT TOP 1 user_id FROM Users WHERE user_type = 'Landlord' ORDER BY user_id);
    DECLARE @Landlord2 BIGINT = (SELECT TOP 1 user_id FROM Users WHERE user_type = 'Landlord' ORDER BY user_id DESC);

    -- 2. Create sample Amenities if missing
    PRINT N'Ensuring Amenities exist...';
    IF NOT EXISTS (SELECT 1 FROM Amenities)
    BEGIN
        INSERT INTO [Amenities] ([name], [icon_url], [category])
        VALUES 
        (N'Wifi', 'fas fa-wifi', 'basic'),
        (N'Điều hòa', 'fas fa-snowflake', 'furniture'),
        (N'Nóng lạnh', 'fas fa-thermometer-half', 'basic'),
        (N'Chỗ để xe', 'fas fa-motorcycle', 'basic'),
        (N'An ninh 24/7', 'fas fa-shield-alt', 'security'),
        (N'Máy giặt', 'fas fa-tshirt', 'furniture'),
        (N'Ban công', 'fas fa-wind', 'basic'),
        (N'Thang máy', 'fas fa-arrow-up', 'furniture');
    END

    -- 3. Create 5 sample Buildings
    PRINT N'Creating Buildings...';
    DECLARE @BuildingIDs TABLE (ID BIGINT);
    
    INSERT INTO [dbo].[Buildings] ([landlord_id], [building_name], [address], [ward], [district], [city], [total_floors], [total_rooms], [is_setup_completed])
    OUTPUT inserted.building_id INTO @BuildingIDs
    VALUES 
    (@Landlord1, N'NestFlow Hòa Lạc 1', N'Số 1, Ngõ 2, Thạch Hòa', N'Thạch Hòa', N'Thạch Thất', N'Hà Nội', 5, 20, 1),
    (@Landlord1, N'Chung cư mini Bình Yên', N'Xóm 3, Bình Yên', N'Bình Yên', N'Thạch Thất', N'Hà Nội', 4, 16, 1),
    (@Landlord2, N'Nhà trọ sinh viên Tân Xã', N'Khu vực Tân Xã', N'Tân Xã', N'Thạch Thất', N'Hà Nội', 3, 30, 1),
    (@Landlord2, N'Căn hộ Studio Hạ Bằng', N'Thôn 5, Hạ Bằng', N'Hạ Bằng', N'Thạch Thất', N'Hà Nội', 5, 15, 1),
    (@Landlord1, N'S-Home Tiến Xuân', N'Khu vực Tiến Xuân', N'Tiến Xuân', N'Thạch Thất', N'Hà Nội', 3, 19, 1);

    -- 4. Create Floors for each building
    PRINT N'Creating Floors...';
    INSERT INTO [dbo].[Floors] ([building_id], [floor_number], [floor_name], [rooms_count], [display_order])
    SELECT ID, 1, N'Tầng 1', 10, 1 FROM @BuildingIDs UNION ALL
    SELECT ID, 2, N'Tầng 2', 10, 2 FROM @BuildingIDs UNION ALL
    SELECT ID, 3, N'Tầng 3', 10, 3 FROM @BuildingIDs;

    -- 5. Generate 100 Properties
    PRINT N'Generating 100 Properties...';
    DECLARE @i INT = 1;
    DECLARE @BuildingId BIGINT;
    DECLARE @FloorId BIGINT;
    DECLARE @Price DECIMAL(12,2);
    DECLARE @Area DECIMAL(12,2);
    DECLARE @Type NVARCHAR(50);
    DECLARE @Status NVARCHAR(50);
    DECLARE @Ward NVARCHAR(100);

    WHILE @i <= 100
    BEGIN
        -- Randomize data
        SET @BuildingId = (SELECT TOP 1 ID FROM @BuildingIDs ORDER BY NEWID());
        SET @FloorId = (SELECT TOP 1 floor_id FROM Floors WHERE building_id = @BuildingId ORDER BY NEWID());
        
        -- Price between 1.5M and 8M
        SET @Price = (FLOOR(RAND() * 65) + 15) * 100000;
        -- Area between 15 and 50
        SET @Area = (FLOOR(RAND() * 35) + 15);
        
        -- Type distribution
        SET @Type = CASE 
            WHEN @i % 5 = 0 THEN 'phong_tro'
            WHEN @i % 5 = 1 THEN 'chung_cu'
            WHEN @i % 5 = 2 THEN 'nha_nguyen_can'
            WHEN @i % 5 = 3 THEN 'studio'
            ELSE 'o_ghep'
        END;

        SET @Ward = (SELECT ward FROM Buildings WHERE building_id = @BuildingId);
        
        -- Insert Property
        INSERT INTO [Properties] 
        ([landlord_id], [building_id], [floor_id], [room_number], [title], [description], [property_type], 
         [address], [ward], [district], [city], [price], [area], [deposit], [max_occupants], [status], 
         [created_at], [updated_at], [current_occupants_count])
        VALUES 
        (@Landlord1, @BuildingId, @FloorId, 'P' + CAST(@i AS NVARCHAR(10)), 
         N'Phòng mẫu ' + CAST(@i AS NVARCHAR(10)) + ' - ' + @Type, 
         N'Mô phỏng mô tả cho phòng thứ ' + CAST(@i AS NVARCHAR(10)) + N'. Đầy đủ tiện nghi, khu vực an ninh.',
         @Type, (SELECT address FROM Buildings WHERE building_id = @BuildingId), 
         @Ward, N'Thạch Thất', N'Hà Nội', @Price, @Area, @Price, 
         CASE WHEN @i % 3 = 0 THEN 4 ELSE 2 END, 'available', GETDATE(), GETDATE(), 0);

        DECLARE @NewPropId BIGINT = SCOPE_IDENTITY();
        DECLARE @AmenityCount INT = (FLOOR(RAND() * 4) + 2);

        -- Assign random amenities
        INSERT INTO [PropertyAmenities] ([property_id], [amenity_id])
        SELECT TOP (@AmenityCount) @NewPropId, amenity_id 
        FROM Amenities ORDER BY NEWID();

        -- Add a sample primary image for UI
        INSERT INTO [PropertyImages] ([property_id], [image_url], [is_primary], [display_order], [uploaded_at])
        VALUES (@NewPropId, 'https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=60', 1, 0, GETDATE());

        SET @i = @i + 1;
    END

    COMMIT TRANSACTION;
    PRINT N'✓ Successfully seeded 100 sample properties!';
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT N'⚠ Error during seeding: ' + ERROR_MESSAGE();
END CATCH
GO
