USE NestFlowSystem
GO
-- Tạo bảng PasswordResetTokens
CREATE TABLE PasswordResetTokens (
    Id BIGINT PRIMARY KEY IDENTITY(1,1),
    UserId BIGINT NOT NULL,
    Token NVARCHAR(10) NOT NULL,
    ExpiresAt DATETIME2 NOT NULL,
    IsUsed BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_PasswordResetTokens_Users FOREIGN KEY (UserId) 
        REFERENCES Users(user_Id) ON DELETE CASCADE
);

-- Tạo index để tăng tốc độ truy vấn
CREATE INDEX IX_PasswordResetTokens_UserId ON PasswordResetTokens(UserId);
CREATE INDEX IX_PasswordResetTokens_Token ON PasswordResetTokens(Token);
CREATE INDEX IX_PasswordResetTokens_ExpiresAt ON PasswordResetTokens(ExpiresAt);