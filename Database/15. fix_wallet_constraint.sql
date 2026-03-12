-- Sửa constraint để cho phép booking_commission

USE [NestFlowSystem]
GO

-- Xóa constraint cũ
ALTER TABLE [dbo].[WalletTransactions] 
DROP CONSTRAINT [CK_WalletTransactions_RelatedType]
GO

-- Tạo constraint mới với booking_commission
ALTER TABLE [dbo].[WalletTransactions]
ADD CONSTRAINT [CK_WalletTransactions_RelatedType] 
CHECK ([related_type] IN ('deposit','invoice','subscription','withdraw','adjustment','booking','withdraw_request','booking_commission'))
GO

PRINT 'Constraint updated successfully! Now supports booking_commission'