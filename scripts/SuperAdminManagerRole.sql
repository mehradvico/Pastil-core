-- ادمین اصلی + نقش «مدیر» (۲۰۲۶-۱۱-۱۵). تکرارپذیر؛ هیچ ساختار جدولی تغییر نمی‌کند، فقط دو داده.
-- توضیح کامل و ترتیب دیپلوی: backend/Docs/SUPER_ADMIN_FA.md
--
-- گام ۰ (فقط خواندن): نقش RoleId = 2 را ببینید؛ Label همان «main-admin» می‌ماند (کد روی Id و Label تکیه دارد)، فقط نام نمایشی عوض می‌شود.
SELECT [Id], [Name], [Label], [Slug] FROM [Roles] WHERE [Id] = 2;
SELECT [Id], [FirstName], [LastName], [Mobile], [RoleId], [Locked], [Deleted], [TwoFactorEnabled],
       CASE WHEN [Password] IS NULL OR [Password] = N'' THEN 0 ELSE 1 END AS HasPassword
FROM [Users] WHERE [Mobile] = N'09339750767';
GO

-- گام ۱: نام نمایشی نقش «مدیر اصلی» ← «ادمین» (فقط مهراد «ادمین اصلی» نوشته می‌شود) (همه‌ی ادمین‌های دیگر این نقش را دارند؛ «ادمین اصلی» یک نفر است و با نقش مشخص نمی‌شود).
UPDATE [Roles] SET [Name] = N'ادمین' WHERE [Id] = 2 AND [Name] <> N'ادمین';
GO

-- گام ۲ (توصیه‌شده برای روز اول دیپلوی): نقش «مدیر» دیگر bypass ندارد و تابع RolePermission است. برای اینکه مدیرهای فعلی
-- لحظه‌ی دیپلوی قفل نشوند، همه‌ی مجوزهای فعلی ناحیه‌ی Admin به نقش ۲ داده می‌شود؛ بعد ادمین اصلی از پنل (نقش‌ها) آنچه لازم نیست
-- را برمی‌دارد. اگر می‌خواهید مدیرها از صفر شروع کنند این گام را اجرا نکنید.
-- (کنترلرهای فقط-ادمین-اصلی مثل نقش‌ها/کیف پول/خاطرات با policy جدا بسته‌اند و با این ردیف‌ها باز نمی‌شوند.)
INSERT INTO [PermissionRole] ([PermissionsId], [RolesId])
SELECT p.[Id], 2
FROM [Permissions] p
WHERE p.[Deleted] = 0
  AND p.[Area] IS NOT NULL AND LOWER(p.[Area]) = N'admin'
  AND NOT EXISTS (SELECT 1 FROM [PermissionRole] pr WHERE pr.[PermissionsId] = p.[Id] AND pr.[RolesId] = 2);
SELECT @@ROWCOUNT AS GrantedPermissionsToManagerRole;
GO
