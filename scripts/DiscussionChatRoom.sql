-- پرسش و پاسخ کالا به‌صورت چت‌روم (۲۰۲۶-۱۱-۱۳): DiscussionAnswers.AdminConfirm / ReplyToAnswerId / StoreId — معادل migration DiscussionChatRoom
-- چند بار اجرا شدن امن است. ترتیب: اول این اسکریپت، بعد دیپلوی Api.
-- پاسخ‌های قبلی: Active=1 ⇒ AdminConfirm=1 (نمایش داده می‌شود)، Active=0 ⇒ AdminConfirm=0 (ردشده/پنهان؛ ادمین می‌تواند دوباره فعال کند).
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO
SET LOCK_TIMEOUT 15000;
SET XACT_ABORT ON;
BEGIN TRAN;

IF COL_LENGTH('DiscussionAnswers', 'AdminConfirm') IS NULL
BEGIN
    ALTER TABLE [DiscussionAnswers] ADD [AdminConfirm] bit NULL;
    EXEC(N'UPDATE [DiscussionAnswers] SET [AdminConfirm] = CASE WHEN [Active] = 1 THEN 1 ELSE 0 END;');
END;
IF COL_LENGTH('DiscussionAnswers', 'ReplyToAnswerId') IS NULL
    ALTER TABLE [DiscussionAnswers] ADD [ReplyToAnswerId] bigint NULL;
IF COL_LENGTH('DiscussionAnswers', 'StoreId') IS NULL
BEGIN
    ALTER TABLE [DiscussionAnswers] ADD [StoreId] bigint NULL;
    EXEC(N'CREATE INDEX [IX_DiscussionAnswers_StoreId] ON [DiscussionAnswers] ([StoreId]);');
    EXEC(N'ALTER TABLE [DiscussionAnswers] ADD CONSTRAINT [FK_DiscussionAnswers_Stores_StoreId] FOREIGN KEY ([StoreId]) REFERENCES [Stores] ([Id]);');
END;

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20261113090000_DiscussionChatRoom')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20261113090000_DiscussionChatRoom', N'9.0.0');

COMMIT;
GO
-- بررسی:
-- SELECT COL_LENGTH('DiscussionAnswers','AdminConfirm') a, COL_LENGTH('DiscussionAnswers','ReplyToAnswerId') r, COL_LENGTH('DiscussionAnswers','StoreId') s;
-- SELECT Active, AdminConfirm, COUNT(*) FROM DiscussionAnswers WHERE Deleted = 0 GROUP BY Active, AdminConfirm;
