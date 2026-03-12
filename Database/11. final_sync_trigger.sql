USE [NestFlowSystem]
GO

-- Xóa trigger cũ
IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'tr_Properties_SyncPriceDeposit')
    DROP TRIGGER tr_Properties_SyncPriceDeposit
GO

-- Tạo trigger mới
CREATE TRIGGER tr_Properties_SyncPriceDeposit
ON [dbo].[Properties]
INSTEAD OF UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE p
    SET 
        landlord_id = i.landlord_id,
        title = i.title,
        description = i.description,
        property_type = i.property_type,
        address = i.address,
        ward = i.ward,
        district = i.district,
        city = i.city,
        area = i.area,
        -- Đồng bộ Price và Deposit
        price = CASE 
            WHEN i.price != d.price THEN i.price  -- Nếu Price thay đổi
            WHEN i.deposit != d.deposit THEN i.deposit  -- Nếu Deposit thay đổi
            ELSE i.price 
        END,
        deposit = CASE 
            WHEN i.price != d.price THEN i.price  -- Nếu Price thay đổi
            WHEN i.deposit != d.deposit THEN i.deposit  -- Nếu Deposit thay đổi
            ELSE i.deposit 
        END,
        max_occupants = i.max_occupants,
        available_from = i.available_from,
        status = i.status,
        view_count = i.view_count,
        created_at = i.created_at,
        updated_at = GETDATE(),
        commission_rate = i.commission_rate,
        user_discount = i.user_discount,
        building_id = i.building_id,
        floor_id = i.floor_id,
        room_number = i.room_number,
        current_occupants_count = i.current_occupants_count,
        current_rental_id = i.current_rental_id
    FROM [dbo].[Properties] p
    INNER JOIN inserted i ON p.property_id = i.property_id
    INNER JOIN deleted d ON p.property_id = d.property_id
END
GO

-- Test
UPDATE Properties SET price = 300000 WHERE property_id = 131
SELECT property_id, price, deposit FROM Properties WHERE property_id = 131