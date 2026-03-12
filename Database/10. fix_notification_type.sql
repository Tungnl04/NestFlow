-- FILE: 10. fix_notification_type.sql
-- Description: Update CK_Notifications_Type constraint to allow common notification types like info, warning, error.

PRINT N'Updating Notifications type constraint...';
GO

IF EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_Notifications_Type' AND parent_object_id = OBJECT_ID('dbo.Notifications'))
BEGIN
    ALTER TABLE [dbo].[Notifications] DROP CONSTRAINT [CK_Notifications_Type];
    PRINT N'Dropped existing CK_Notifications_Type constraint.';
END
GO

ALTER TABLE [dbo].[Notifications] WITH CHECK ADD CONSTRAINT [CK_Notifications_Type] 
CHECK ([type] IN ('review', 'message', 'payment', 'booking', 'info', 'success', 'warning', 'error', 'danger'));
GO

PRINT N'Added new CK_Notifications_Type constraint successfully.';
GO
