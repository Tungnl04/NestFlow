-- =====================================================================
-- Insert Sample Data for Plans Table
-- =====================================================================

USE [NestFlowSystem]
GO

-- Xóa dữ liệu cũ nếu có
DELETE FROM [dbo].[Plans];
DBCC CHECKIDENT ('[dbo].[Plans]', RESEED, 0);
GO

-- Insert Plans
SET IDENTITY_INSERT [dbo].[Plans] ON;
GO

INSERT INTO [dbo].[Plans] 
    ([plan_id], [plan_name], [price], [duration_days], [quota_active_listings], [priority_level], [description], [is_active], [created_at], [updated_at])
VALUES
    -- Gói Thường (ID: 1)
    (1, N'Gói Thường', 54000.00, 30, 5, 1, 
     N'Đăng tin trên trang web, hiển thị 30 ngày. Giá theo ngày: 2.000đ/ngày (tối thiểu 3 ngày)', 
     1, GETDATE(), GETDATE()),
    
    -- Gói VIP1 (ID: 2)
    (2, N'Gói VIP1', 510000.00, 30, 10, 2, 
     N'TOP tìm kiếm, màu sắc tiêu đề nổi bật, hiển thị 30 ngày. Giá theo ngày: 20.000đ/ngày (tối thiểu 3 ngày)', 
     1, GETDATE(), GETDATE()),
    
    -- Gói VIP2 (ID: 3)
    (3, N'Gói VIP2', 840000.00, 30, 20, 3, 
     N'TOP tìm kiếm, hình ảnh NỔI BẬT, nút gọi điện trực tiếp, ưu tiên tư vấn (VIP3). Giá theo ngày: 35.000đ/ngày (tối thiểu 3 ngày)', 
     1, GETDATE(), GETDATE()),
    
    -- Sử dụng quản lý (ID: 4)
    (4, N'Sử dụng quản lý', 1400000.00, 30, 50, 4, 
     N'Đăng tin (tương đương VIP3) + Chương trình quản lý phòng trọ đầy đủ', 
     1, GETDATE(), GETDATE()),
    
    -- Chỉ sử dụng quản lý (ID: 5)
    (5, N'Chỉ sử dụng quản lý', 560000.00, 30, 0, 0, 
     N'Chỉ sử dụng chương trình quản lý phòng trọ, không đăng tin', 
     1, GETDATE(), GETDATE());

SET IDENTITY_INSERT [dbo].[Plans] OFF;
GO

-- Verify data
SELECT * FROM [dbo].[Plans];
GO

PRINT N'✓ Plans data inserted successfully!';
GO
