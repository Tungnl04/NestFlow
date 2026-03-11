
-- Migration Script: Add Buildings and Floors Management System
-- Date: 2026-03-07
-- Description: Thêm hệ thống quản lý nhà trọ, tầng và cập nhật phòng
-- =====================================================================

USE [NestFlowSystem]
GO

PRINT N'Starting migration: Buildings and Floors Management System...';
GO

-- =====================================================================
-- STEP 1: CREATE BUILDINGS TABLE
-- =====================================================================
PRINT N'Creating Buildings table...';
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Buildings')
BEGIN
    CREATE TABLE [dbo].[Buildings](
        [building_id] [bigint] IDENTITY(1,1) NOT NULL,
        [landlord_id] [bigint] NOT NULL,
        [building_name] [nvarchar](255) NOT NULL,
        [address] [nvarchar](255) NULL,
        [ward] [nvarchar](100) NULL,
        [district] [nvarchar](100) NULL,
        [city] [nvarchar](100) NULL,
        [total_floors] [int] NULL DEFAULT 0,
        [total_rooms] [int] NULL DEFAULT 0,
        [description] [nvarchar](max) NULL,
        [is_setup_completed] [bit] NOT NULL DEFAULT 0, -- Đánh dấu đã setup tầng/phòng chưa
        [created_at] [datetime] NOT NULL DEFAULT GETDATE(),
        [updated_at] [datetime] NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_Buildings] PRIMARY KEY CLUSTERED ([building_id] ASC),
        CONSTRAINT [FK_Buildings_Landlord] FOREIGN KEY([landlord_id])
            REFERENCES [dbo].[Users] ([user_id])
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];
    
    -- Create index
    CREATE NONCLUSTERED INDEX [IX_Buildings_Landlord] 
        ON [dbo].[Buildings]([landlord_id] ASC);
    
    PRINT N'✓ Buildings table created successfully';
END
ELSE
BEGIN
    PRINT N'⚠ Buildings table already exists, skipping...';
END
GO

-- =====================================================================
-- STEP 2: CREATE FLOORS TABLE
-- =====================================================================
PRINT N'Creating Floors table...';
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Floors')
BEGIN
    CREATE TABLE [dbo].[Floors](
        [floor_id] [bigint] IDENTITY(1,1) NOT NULL,
        [building_id] [bigint] NOT NULL,
        [floor_number] [int] NOT NULL, -- 1, 2, 3, 4...
        [floor_name] [nvarchar](50) NOT NULL, -- "Tầng 1", "Tầng 2"...
        [rooms_count] [int] NOT NULL DEFAULT 0,
        [display_order] [int] NOT NULL DEFAULT 0, -- Thứ tự hiển thị
        [created_at] [datetime] NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_Floors] PRIMARY KEY CLUSTERED ([floor_id] ASC),
        CONSTRAINT [FK_Floors_Building] FOREIGN KEY([building_id])
            REFERENCES [dbo].[Buildings] ([building_id]) ON DELETE CASCADE,
        CONSTRAINT [UQ_Floors_BuildingFloorNumber] UNIQUE ([building_id], [floor_number])
    ) ON [PRIMARY];
    
    -- Create indexes
    CREATE NONCLUSTERED INDEX [IX_Floors_Building] 
        ON [dbo].[Floors]([building_id] ASC);
    CREATE NONCLUSTERED INDEX [IX_Floors_DisplayOrder] 
        ON [dbo].[Floors]([building_id] ASC, [display_order] ASC);
    
    PRINT N'✓ Floors table created successfully';
END
ELSE
BEGIN
    PRINT N'⚠ Floors table already exists, skipping...';
END
GO

-- =====================================================================
-- STEP 3: ALTER PROPERTIES TABLE - ADD NEW COLUMNS
-- =====================================================================
PRINT N'Altering Properties table...';
GO

-- Add building_id column
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID(N'[dbo].[Properties]') 
               AND name = 'building_id')
BEGIN
    ALTER TABLE [dbo].[Properties] 
        ADD [building_id] [bigint] NULL;
    PRINT N'✓ Added building_id column';
END

-- Add floor_id column
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID(N'[dbo].[Properties]') 
               AND name = 'floor_id')
BEGIN
    ALTER TABLE [dbo].[Properties] 
        ADD [floor_id] [bigint] NULL;
    PRINT N'✓ Added floor_id column';
END

-- Add room_number column
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID(N'[dbo].[Properties]') 
               AND name = 'room_number')
BEGIN
    ALTER TABLE [dbo].[Properties] 
        ADD [room_number] [nvarchar](20) NULL; -- P101, P201, P302...
    PRINT N'✓ Added room_number column';
END

-- Update max_occupants if it doesn't exist or is NULL
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID(N'[dbo].[Properties]') 
               AND name = 'max_occupants')
BEGIN
    -- Column already exists from original schema, just ensure it has default
    ALTER TABLE [dbo].[Properties] 
        ADD CONSTRAINT [DF_Properties_MaxOccupants] DEFAULT 2 FOR [max_occupants];
    PRINT N'✓ Added default for max_occupants';
END

-- Add current_occupants_count column (số người đang ở)
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID(N'[dbo].[Properties]') 
               AND name = 'current_occupants_count')
BEGIN
    ALTER TABLE [dbo].[Properties] 
        ADD [current_occupants_count] [int] NOT NULL DEFAULT 0;
    PRINT N'✓ Added current_occupants_count column';
END

-- Add current_rental_id column (thuê hiện tại)
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID(N'[dbo].[Properties]') 
               AND name = 'current_rental_id')
BEGIN
    ALTER TABLE [dbo].[Properties] 
        ADD [current_rental_id] [bigint] NULL;
    PRINT N'✓ Added current_rental_id column';
END

GO

-- =====================================================================
-- STEP 4: ADD FOREIGN KEY CONSTRAINTS FOR PROPERTIES
-- =====================================================================
PRINT N'Adding foreign key constraints to Properties...';
GO

-- FK to Buildings
IF NOT EXISTS (SELECT * FROM sys.foreign_keys 
               WHERE name = 'FK_Properties_Building')
BEGIN
    ALTER TABLE [dbo].[Properties] WITH CHECK 
        ADD CONSTRAINT [FK_Properties_Building] FOREIGN KEY([building_id])
        REFERENCES [dbo].[Buildings] ([building_id]);
    PRINT N'✓ Added FK_Properties_Building';
END

-- FK to Floors
IF NOT EXISTS (SELECT * FROM sys.foreign_keys 
               WHERE name = 'FK_Properties_Floor')
BEGIN
    ALTER TABLE [dbo].[Properties] WITH CHECK 
        ADD CONSTRAINT [FK_Properties_Floor] FOREIGN KEY([floor_id])
        REFERENCES [dbo].[Floors] ([floor_id]);
    PRINT N'✓ Added FK_Properties_Floor';
END

-- FK to current rental
IF NOT EXISTS (SELECT * FROM sys.foreign_keys 
               WHERE name = 'FK_Properties_CurrentRental')
BEGIN
    ALTER TABLE [dbo].[Properties] WITH CHECK 
        ADD CONSTRAINT [FK_Properties_CurrentRental] FOREIGN KEY([current_rental_id])
        REFERENCES [dbo].[Rentals] ([rental_id]);
    PRINT N'✓ Added FK_Properties_CurrentRental';
END

GO

-- =====================================================================
-- STEP 5: CREATE INDEXES ON PROPERTIES NEW COLUMNS
-- =====================================================================
PRINT N'Creating indexes on Properties...';
GO

IF NOT EXISTS (SELECT * FROM sys.indexes 
               WHERE name = 'IX_Properties_Building' 
               AND object_id = OBJECT_ID('dbo.Properties'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Properties_Building] 
        ON [dbo].[Properties]([building_id] ASC);
    PRINT N'✓ Created IX_Properties_Building';
END

IF NOT EXISTS (SELECT * FROM sys.indexes 
               WHERE name = 'IX_Properties_Floor' 
               AND object_id = OBJECT_ID('dbo.Properties'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Properties_Floor] 
        ON [dbo].[Properties]([floor_id] ASC);
    PRINT N'✓ Created IX_Properties_Floor';
END

IF NOT EXISTS (SELECT * FROM sys.indexes 
               WHERE name = 'IX_Properties_RoomNumber' 
               AND object_id = OBJECT_ID('dbo.Properties'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Properties_RoomNumber] 
        ON [dbo].[Properties]([building_id] ASC, [room_number] ASC);
    PRINT N'✓ Created IX_Properties_RoomNumber';
END

GO

-- =====================================================================
-- STEP 6: UPDATE EXISTING PROPERTIES STATUS VALUES
-- =====================================================================
PRINT N'Updating Properties status values...';
GO

-- Cập nhật status để phù hợp với hệ thống mới
-- available → available (giữ nguyên)
-- rented → occupied (đổi tên cho đồng nhất)
-- unavailable → inactive

UPDATE [dbo].[Properties]
SET [status] = 'occupied'
WHERE [status] = 'rented';

UPDATE [dbo].[Properties]
SET [status] = 'inactive'
WHERE [status] = 'unavailable';

-- Drop old check constraint if exists
IF EXISTS (SELECT * FROM sys.check_constraints 
           WHERE name = 'CK_Properties_Status')
BEGIN
    ALTER TABLE [dbo].[Properties] DROP CONSTRAINT [CK_Properties_Status];
    PRINT N'✓ Dropped old CK_Properties_Status';
END

-- Add new check constraint with updated values
ALTER TABLE [dbo].[Properties] WITH CHECK 
    ADD CONSTRAINT [CK_Properties_Status] 
    CHECK ([status] IN ('available', 'occupied', 'maintenance', 'inactive'));

PRINT N'✓ Updated Properties status values and constraints';
GO

-- =====================================================================
-- STEP 7: MIGRATE EXISTING PROPERTIES TO BUILDING SYSTEM
-- =====================================================================
PRINT N'Migrating existing properties to building system...';
GO

-- Tạo building mặc định cho mỗi landlord có properties
INSERT INTO [dbo].[Buildings] ([landlord_id], [building_name], [address], [ward], [district], [city], [total_floors], [total_rooms], [is_setup_completed])
SELECT DISTINCT 
    p.landlord_id,
    COALESCE(u.full_name, 'Chủ ' + CAST(p.landlord_id AS NVARCHAR)) + N' - Nhà trọ chính',
    MAX(p.address),
    MAX(p.ward),
    MAX(p.district),
    MAX(p.city),
    1, -- Mặc định 1 tầng
    COUNT(*), -- Số phòng hiện có
    0 -- Chưa setup, cần cấu hình lại
FROM [dbo].[Properties] p
LEFT JOIN [dbo].[Users] u ON p.landlord_id = u.user_id
WHERE p.building_id IS NULL -- Chỉ migrate những property chưa có building
GROUP BY p.landlord_id, u.full_name;

PRINT N'✓ Created default buildings for existing landlords';

-- Tạo tầng 1 mặc định cho mỗi building
INSERT INTO [dbo].[Floors] ([building_id], [floor_number], [floor_name], [rooms_count], [display_order])
SELECT 
    b.building_id,
    1,
    N'Tầng 1',
    COUNT(p.property_id),
    1
FROM [dbo].[Buildings] b
LEFT JOIN [dbo].[Properties] p ON p.landlord_id = b.landlord_id AND p.building_id IS NULL
GROUP BY b.building_id;

PRINT N'✓ Created default floor 1 for all buildings';

WITH RoomCTE AS
(
    SELECT 
        p.property_id,
        b.building_id,
        f.floor_id,
        ROW_NUMBER() OVER (PARTITION BY b.building_id ORDER BY p.property_id) AS rn
    FROM dbo.Properties p
    INNER JOIN dbo.Buildings b ON p.landlord_id = b.landlord_id
    INNER JOIN dbo.Floors f ON f.building_id = b.building_id AND f.floor_number = 1
    WHERE p.building_id IS NULL
)

UPDATE p
SET 
    p.building_id = c.building_id,
    p.floor_id = c.floor_id,
    p.room_number = 'P1' + RIGHT('0' + CAST(c.rn AS NVARCHAR),2)
FROM dbo.Properties p
INNER JOIN RoomCTE c ON p.property_id = c.property_id;

PRINT N'✓ Assigned existing properties to buildings and floors';

-- Cập nhật current_rental_id cho các phòng đang được thuê
UPDATE p
SET p.current_rental_id = r.rental_id
FROM [dbo].[Properties] p
INNER JOIN [dbo].[Rentals] r ON r.property_id = p.property_id
WHERE r.status = 'active'
  AND p.status = 'occupied';

PRINT N'✓ Updated current_rental_id for occupied rooms';

-- Cập nhật current_occupants_count
UPDATE p
SET p.current_occupants_count = (
    SELECT COUNT(*)
    FROM [dbo].[RentalOccupants] ro
    INNER JOIN [dbo].[Rentals] r ON ro.rental_id = r.rental_id
    WHERE r.property_id = p.property_id
      AND r.status = 'active'
      AND ro.status = 'active'
)
FROM [dbo].[Properties] p
WHERE p.status = 'occupied';
PRINT N'✓ Updated current_occupants_count for occupied rooms';

GO

-- =====================================================================
-- STEP 8: CREATE HELPER VIEWS
-- =====================================================================
PRINT N'Creating helper views...';
GO

-- View: Thông tin tổng hợp phòng
IF OBJECT_ID('dbo.vw_RoomDetails', 'V') IS NOT NULL
    DROP VIEW dbo.vw_RoomDetails;
GO

CREATE VIEW dbo.vw_RoomDetails
AS
SELECT 
    p.property_id,
    p.building_id,
    p.floor_id,
    p.room_number,
    p.title,
    p.area,
    p.price,
    p.deposit,
    p.max_occupants,
    p.current_occupants_count,
    p.status,
    b.building_name,
    f.floor_number,
    f.floor_name,
    r.rental_id as current_rental_id,
    r.renter_id,
    r.start_date as rental_start_date,
    r.end_date as rental_end_date,
    u.full_name as renter_name,
    u.phone as renter_phone,
    CASE 
        WHEN p.status = 'available' THEN N'Trống'
        WHEN p.status = 'occupied' THEN N'Đang thuê'
        WHEN p.status = 'maintenance' THEN N'Bảo trì'
        WHEN p.status = 'inactive' THEN N'Ngừng hoạt động'
        ELSE p.status
    END as status_display
FROM [dbo].[Properties] p
LEFT JOIN [dbo].[Buildings] b ON p.building_id = b.building_id
LEFT JOIN [dbo].[Floors] f ON p.floor_id = f.floor_id
LEFT JOIN [dbo].[Rentals] r ON p.current_rental_id = r.rental_id
LEFT JOIN [dbo].[Users] u ON r.renter_id = u.user_id;
GO

PRINT N'✓ Created vw_RoomDetails view';

-- View: Thống kê building
IF OBJECT_ID('dbo.vw_BuildingStatistics', 'V') IS NOT NULL
    DROP VIEW dbo.vw_BuildingStatistics;
GO

CREATE VIEW dbo.vw_BuildingStatistics
AS
SELECT 
    b.building_id,
    b.building_name,
    b.landlord_id,
    b.total_floors,
    b.total_rooms,
    COUNT(DISTINCT p.property_id) as actual_rooms_count,
    SUM(CASE WHEN p.status = 'available' THEN 1 ELSE 0 END) as available_rooms,
    SUM(CASE WHEN p.status = 'occupied' THEN 1 ELSE 0 END) as occupied_rooms,
    SUM(CASE WHEN p.status = 'maintenance' THEN 1 ELSE 0 END) as maintenance_rooms,
    SUM(CASE WHEN p.status = 'inactive' THEN 1 ELSE 0 END) as inactive_rooms,
    SUM(CASE WHEN p.status = 'occupied' THEN p.price ELSE 0 END) as monthly_revenue
FROM [dbo].[Buildings] b
LEFT JOIN [dbo].[Properties] p ON b.building_id = p.building_id
GROUP BY b.building_id, b.building_name, b.landlord_id, b.total_floors, b.total_rooms;
GO

PRINT N'✓ Created vw_BuildingStatistics view';

GO

-- =====================================================================
-- STEP 9: CREATE STORED PROCEDURES
-- =====================================================================
PRINT N'Creating stored procedures...';
GO

-- SP: Khởi tạo tầng và phòng cho building
IF OBJECT_ID('dbo.sp_InitializeBuildingFloorsAndRooms', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_InitializeBuildingFloorsAndRooms;
GO

CREATE PROCEDURE dbo.sp_InitializeBuildingFloorsAndRooms
    @BuildingId BIGINT,
    @TotalFloors INT,
    @RoomsPerFloor INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @FloorNumber INT = 1;
    DECLARE @FloorId BIGINT;
    DECLARE @RoomNumber INT;
    DECLARE @RoomNumberStr NVARCHAR(20);
    DECLARE @TotalRooms INT = @TotalFloors * @RoomsPerFloor;
    
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- Xóa các floor và room cũ nếu có
        DELETE FROM [dbo].[Properties] WHERE building_id = @BuildingId;
        DELETE FROM [dbo].[Floors] WHERE building_id = @BuildingId;
        
        -- Tạo các tầng
        WHILE @FloorNumber <= @TotalFloors
        BEGIN
            INSERT INTO [dbo].[Floors] ([building_id], [floor_number], [floor_name], [rooms_count], [display_order])
            VALUES (@BuildingId, @FloorNumber, N'Tầng ' + CAST(@FloorNumber AS NVARCHAR), @RoomsPerFloor, @FloorNumber);
            
            SET @FloorId = SCOPE_IDENTITY();
            
            -- Tạo các phòng cho tầng này
            SET @RoomNumber = 1;
            WHILE @RoomNumber <= @RoomsPerFloor
            BEGIN
                SET @RoomNumberStr = 'P' + CAST(@FloorNumber AS NVARCHAR) + RIGHT('0' + CAST(@RoomNumber AS NVARCHAR), 2);
                
                INSERT INTO [dbo].[Properties] 
                    ([landlord_id], [building_id], [floor_id], [room_number], [title], [property_type], [status], [area], [price], [deposit], [max_occupants])
                SELECT 
                    landlord_id,
                    @BuildingId,
                    @FloorId,
                    @RoomNumberStr,
                    N'Phòng ' + @RoomNumberStr,
                    'phong_tro',
                    'available',
                    25.00,
                    2000000.00,
                    2000000.00,
                    2
                FROM [dbo].[Buildings]
                WHERE building_id = @BuildingId;
                
                SET @RoomNumber = @RoomNumber + 1;
            END
            
            SET @FloorNumber = @FloorNumber + 1;
        END
        
        -- Cập nhật thông tin building
        UPDATE [dbo].[Buildings]
        SET 
            total_floors = @TotalFloors,
            total_rooms = @TotalRooms,
            is_setup_completed = 1,
            updated_at = GETDATE()
        WHERE building_id = @BuildingId;
        
        COMMIT TRANSACTION;
        
        SELECT 1 as Success, N'Khởi tạo thành công ' + CAST(@TotalFloors AS NVARCHAR) + N' tầng và ' + CAST(@TotalRooms AS NVARCHAR) + N' phòng' as Message;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        SELECT 0 as Success, ERROR_MESSAGE() as Message;
    END CATCH
END
GO

PRINT N'✓ Created sp_InitializeBuildingFloorsAndRooms';

GO

-- =====================================================================
-- MIGRATION COMPLETED
-- =====================================================================
PRINT N'';
PRINT N'========================================';
PRINT N'Migration completed successfully! ✓';
PRINT N'========================================';
PRINT N'';
PRINT N'Summary:';
PRINT N'- Buildings table created';
PRINT N'- Floors table created';
PRINT N'- Properties table updated with new columns';
PRINT N'- Existing properties migrated to building system';
PRINT N'- Helper views created';
PRINT N'- Stored procedures created';
PRINT N'';
PRINT N'Next steps:';
PRINT N'1. Run the application';
PRINT N'2. Landlords will be prompted to setup floors/rooms for each building';
PRINT N'3. System is ready to use!';
PRINT N'';
GO
--Update renter_id nullable
ALTER TABLE Rentals
ALTER COLUMN renter_id BIGINT NULL	