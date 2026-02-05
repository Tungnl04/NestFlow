-- Database Seed Script for NestFlow
-- Uses snake_case column names and valid constraints.

BEGIN TRANSACTION;

-- 1. Insert Landlords (Users)
-- Valid user_type: 'renter', 'landlord', 'admin'
-- Valid status: 'active', 'inactive', 'banned'
IF NOT EXISTS (SELECT 1 FROM Users WHERE email = 'landlord1@nestflow.com')
BEGIN
    INSERT INTO Users (email, password_hash, full_name, phone, user_type, status, avatar_url, created_at, updated_at)
    VALUES 
    ('landlord1@nestflow.com', '123456', N'Nguyễn Văn A', '0987654321', 'landlord', 'active', 'https://ui-avatars.com/api/?name=Nguyen+Van+A&background=random', GETDATE(), GETDATE()),
    ('landlord2@nestflow.com', '123456', N'Trần Thị B', '0987654322', 'landlord', 'active', 'https://ui-avatars.com/api/?name=Tran+Thi+B&background=random', GETDATE(), GETDATE()),
    ('landlord3@nestflow.com', '123456', N'Lê Văn C', '0987654323', 'landlord', 'active', 'https://ui-avatars.com/api/?name=Le+Van+C&background=random', GETDATE(), GETDATE());
END

-- Get Landlord IDs
DECLARE @L1 BIGINT = (SELECT user_id FROM Users WHERE email = 'landlord1@nestflow.com');
DECLARE @L2 BIGINT = (SELECT user_id FROM Users WHERE email = 'landlord2@nestflow.com');
DECLARE @L3 BIGINT = (SELECT user_id FROM Users WHERE email = 'landlord3@nestflow.com');

-- 2. Insert Properties
-- Valid property_type: 'phong_tro', 'chung_cu', 'nha_nguyen_can'
-- Valid status: 'available', 'rented', 'unavailable'
IF NOT EXISTS (SELECT 1 FROM Properties)
BEGIN
    INSERT INTO Properties (landlord_id, title, description, property_type, address, ward, district, city, area, price, deposit, max_occupants, available_from, status, view_count, created_at, updated_at)
    VALUES 
    -- Batch 1 (Landlord 1)
    (@L1, N'Phòng trọ khép kín full đồ tại Thạch Hòa', N'Phòng đẹp, đầy đủ tiện nghi, giờ giấc tự do, an ninh tốt.', 'phong_tro', N'Số 10, Ngõ 5', N'Thạch Hòa', N'Thạch Thất', N'Hà Nội', 25, 2500000, 2500000, 2, GETDATE(), 'available', 120, GETDATE(), GETDATE()),
    (@L1, N'Chung cư mini cao cấp tại Tân Xã', N'Nhà mới xây, có thang máy, bảo vệ 24/7. Nội thất sang trọng.', 'chung_cu', N'Số 22, Phố Tân Xã', N'Tân Xã', N'Thạch Thất', N'Hà Nội', 40, 5000000, 5000000, 3, GETDATE(), 'available', 345, GETDATE(), GETDATE()),
    (@L1, N'Phòng trọ giá rẻ sinh viên tại Bình Yên', N'Phòng thoáng mát, gần chợ, điện nước giá dân.', 'phong_tro', N'Xóm 2', N'Bình Yên', N'Thạch Thất', N'Hà Nội', 18, 1800000, 1800000, 2, GETDATE(), 'available', 98, GETDATE(), GETDATE()),
    (@L1, N'Nhà nguyên căn 3 tầng tại Hạ Bằng', N'Nhà riêng biệt, phù hợp nhóm bạn 5-6 người hoặc gia đình.', 'nha_nguyen_can', N'Thôn 3', N'Hạ Bằng', N'Thạch Thất', N'Hà Nội', 100, 8000000, 8000000, 6, GETDATE(), 'available', 450, GETDATE(), GETDATE()),
    (@L1, N'Homestay chill sân vườn tại Đồng Trúc', N'Không gian xanh, yên tĩnh, thích hợp nghỉ dưỡng cuối tuần.', 'phong_tro', N'Xóm Trại', N'Đồng Trúc', N'Thạch Thất', N'Hà Nội', 35, 3500000, 3500000, 2, GETDATE(), 'available', 210, GETDATE(), GETDATE()),
    (@L1, N'Phòng trọ gác xép tại Thạch Hòa', N'Có gác xép rộng, bếp riêng, vệ sinh khép kín.', 'phong_tro', N'Ngõ 8', N'Thạch Hòa', N'Thạch Thất', N'Hà Nội', 22, 2200000, 2200000, 2, GETDATE(), 'available', 155, GETDATE(), GETDATE()),
    
    -- Batch 2 (Landlord 2)
    (@L2, N'Căn hộ Studio full nội thất tại Tân Xã', N'Thiết kế hiện đại, ban công thoáng, full đồ chỉ việc xách vali về ở.', 'chung_cu', N'Khu công nghệ cao', N'Tân Xã', N'Thạch Thất', N'Hà Nội', 30, 4200000, 4200000, 2, GETDATE(), 'available', 380, GETDATE(), GETDATE()),
    (@L2, N'Phòng trọ gần đại học FPT', N'Đi bộ 10p sang trường, bao quanh nhiều tiện ích ăn uống.', 'phong_tro', N'Tái định cư', N'Thạch Hòa', N'Thạch Thất', N'Hà Nội', 20, 3000000, 3000000, 1, GETDATE(), 'available', 600, GETDATE(), GETDATE()),
    (@L2, N'Nhà trọ an ninh tốt tại Bình Yên', N'Camera 24/24, cổng vân tay, giờ giấc thoải mái.', 'phong_tro', N'Thôn Cánh Chủ', N'Bình Yên', N'Thạch Thất', N'Hà Nội', 25, 2600000, 2600000, 2, GETDATE(), 'available', 130, GETDATE(), GETDATE()),
    (@L2, N'Phòng ở ghép nam/nữ tại Tân Xã', N'Giường tầng cao cấp, điều hòa, tủ lạnh, máy giặt chung.', 'phong_tro', N'Số 50', N'Tân Xã', N'Thạch Thất', N'Hà Nội', 40, 1500000, 1500000, 4, GETDATE(), 'available', 90, GETDATE(), GETDATE()),
    (@L2, N'Chung cư mini 2 ngủ tại Thạch Hòa', N'2 phòng ngủ riêng biệt, phòng khách rộng, ban công phơi đồ.', 'chung_cu', N'Khu tái định cư ĐHQG', N'Thạch Hòa', N'Thạch Thất', N'Hà Nội', 55, 6000000, 6000000, 4, GETDATE(), 'available', 410, GETDATE(), GETDATE()),
    (@L2, N'Nhà cấp 4 sân vườn tại Đồng Trúc', N'Rộng rãi thoáng mát, có chỗ trồng rau, nuôi thú cưng.', 'nha_nguyen_can', N'Xóm Đồng', N'Đồng Trúc', N'Thạch Thất', N'Hà Nội', 80, 4000000, 4000000, 3, GETDATE(), 'available', 180, GETDATE(), GETDATE()),
    
    -- Batch 3 (Landlord 3)
    (@L3, N'Phòng trọ giá rẻ tại Hạ Bằng', N'Phòng đơn giản, sạch sẽ, giá mềm cho sinh viên tiết kiệm.', 'phong_tro', N'Thôn 9', N'Hạ Bằng', N'Thạch Thất', N'Hà Nội', 15, 1200000, 1200000, 1, GETDATE(), 'available', 75, GETDATE(), GETDATE()),
    (@L3, N'Căn hộ dịch vụ cao cấp tại Tân Xã', N'Dọn phòng 2 lần/tuần, giặt là miễn phí, gym, bể bơi.', 'chung_cu', N'Tòa nhà A1', N'Tân Xã', N'Thạch Thất', N'Hà Nội', 45, 7500000, 7500000, 2, GETDATE(), 'available', 290, GETDATE(), GETDATE()),
    (@L3, N'Phòng trọ mới xây tại Thạch Hòa', N'Mới 100%, thiết bị vệ sinh Inax, gạch men cao cấp.', 'phong_tro', N'Ngã 3 Hòa Lạc', N'Thạch Hòa', N'Thạch Thất', N'Hà Nội', 28, 3200000, 3200000, 2, GETDATE(), 'available', 240, GETDATE(), GETDATE()),
    (@L3, N'Nhà nguyên căn 2 tầng tại Bình Yên', N'Mặt đường lớn, tiện kinh doanh kết hợp ở.', 'nha_nguyen_can', N'Mặt đường 420', N'Bình Yên', N'Thạch Thất', N'Hà Nội', 120, 10000000, 10000000, 6, GETDATE(), 'available', 520, GETDATE(), GETDATE()),
    (@L3, N'Phòng trọ có gác lửng tại Tân Xã', N'Gác lửng cao không đụng đầu, thang gỗ chắc chắn.', 'phong_tro', N'Ngõ 15', N'Tân Xã', N'Thạch Thất', N'Hà Nội', 25, 2800000, 2800000, 2, GETDATE(), 'available', 165, GETDATE(), GETDATE()),
    (@L3, N'Homestay container độc lạ tại Đồng Trúc', N'Trải nghiệm sống độc đáo, không gian sáng tạo.', 'phong_tro', N'Khu sinh thái', N'Đồng Trúc', N'Thạch Thất', N'Hà Nội', 20, 2500000, 2500000, 2, GETDATE(), 'available', 195, GETDATE(), GETDATE()),
    (@L3, N'Chung cư mini full option tại Thạch Hòa', N'Tủ lạnh, máy giặt, tivi, sofa, giường nệm cao su.', 'chung_cu', N'Khu CNC 2', N'Thạch Hòa', N'Thạch Thất', N'Hà Nội', 35, 4800000, 4800000, 2, GETDATE(), 'available', 310, GETDATE(), GETDATE()),
    (@L3, N'Phòng trọ ban công rộng tại Hạ Bằng', N'Ban công view thoáng, trồng cây thoải mái.', 'phong_tro', N'Xóm 4', N'Hạ Bằng', N'Thạch Thất', N'Hà Nội', 26, 2400000, 2400000, 2, GETDATE(), 'available', 145, GETDATE(), GETDATE());
END

-- 3. Insert Property Images
DECLARE @Img1 NVARCHAR(MAX) = 'https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?w=800&q=80';
DECLARE @Img2 NVARCHAR(MAX) = 'https://images.unsplash.com/photo-1502672260266-1c1ef2d93688?w=800&q=80';
DECLARE @Img3 NVARCHAR(MAX) = 'https://images.unsplash.com/photo-1560448204-e02f11c3d0e2?w=800&q=80';
DECLARE @Img4 NVARCHAR(MAX) = 'https://images.unsplash.com/photo-1522771739844-6a9f6d5f14af?w=800&q=80';
DECLARE @Img5 NVARCHAR(MAX) = 'https://images.unsplash.com/photo-1493809842364-78817add7ffb?w=800&q=80';

IF NOT EXISTS (SELECT 1 FROM PropertyImages)
BEGIN
    INSERT INTO PropertyImages (property_id, image_url, is_primary, display_order)
    SELECT property_id, @Img1, 1, 0 FROM Properties WHERE title LIKE N'%Phòng trọ%';
    
    INSERT INTO PropertyImages (property_id, image_url, is_primary, display_order)
    SELECT property_id, @Img2, 0, 1 FROM Properties WHERE title LIKE N'%Phòng trọ%';

    INSERT INTO PropertyImages (property_id, image_url, is_primary, display_order)
    SELECT property_id, @Img3, 1, 0 FROM Properties WHERE title LIKE N'%Chung cư%';

    INSERT INTO PropertyImages (property_id, image_url, is_primary, display_order)
    SELECT property_id, @Img4, 0, 1 FROM Properties WHERE title LIKE N'%Chung cư%';

    INSERT INTO PropertyImages (property_id, image_url, is_primary, display_order)
    SELECT property_id, @Img5, 1, 0 FROM Properties WHERE title LIKE N'%Nhà nguyên căn%';
    
    INSERT INTO PropertyImages (property_id, image_url, is_primary, display_order)
    SELECT property_id, @Img1, 0, 1 FROM Properties WHERE title LIKE N'%Nhà nguyên căn%';

    -- Add mix for others
    INSERT INTO PropertyImages (property_id, image_url, is_primary, display_order)
    SELECT property_id, @Img2, 1, 0 FROM Properties WHERE title LIKE N'%Homestay%';

    INSERT INTO PropertyImages (property_id, image_url, is_primary, display_order)
    SELECT property_id, @Img3, 1, 0 FROM Properties WHERE title LIKE N'%Căn hộ%';
END

-- 4. Insert Amenities (Standard list)
-- Valid categories: 'basic', 'furniture', 'security'
IF NOT EXISTS (SELECT 1 FROM Amenities)
BEGIN
    INSERT INTO Amenities (name, icon_url, category)
    VALUES 
    (N'Wifi miễn phí', 'fas fa-wifi', 'basic'),
    (N'Điều hòa', 'fas fa-wind', 'furniture'),
    (N'Nóng lạnh', 'fas fa-water', 'basic'),
    (N'Máy giặt', 'fas fa-tshirt', 'furniture'),
    (N'Chỗ để xe', 'fas fa-motorcycle', 'basic'),
    (N'An ninh 24/7', 'fas fa-shield-alt', 'security'),
    (N'Tủ lạnh', 'fas fa-snowflake', 'furniture'),
    (N'Camera', 'fas fa-video', 'security'),
    (N'Thang máy', 'fas fa-elevator', 'basic'),
    (N'Bếp chung', 'fas fa-fire-burner', 'furniture');
END

COMMIT TRANSACTION;
PRINT 'Database seeded successfully.';
