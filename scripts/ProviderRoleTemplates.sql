-- نقش‌های آماده‌ی کارمندان ارائه‌دهنده (۲۰۲۶-۱۱-۰۷)
-- بخش ۱: تغییر اسکیما (ProviderUserRoles.ProviderId نال‌پذیر + ستون‌های Scope/Priority + ایندکس‌ها) — معادل migration ProviderRoleTemplates
-- بخش ۲: درج نقش‌های آماده زیر هر نمایندگی (Clinic/Store/Groomer/Coach/School/Pansion). پت‌رسان نقش ندارد.
-- چند بار اجرا شدن امن است (هر دو بخش idempotent). ترتیب: اول این اسکریپت، بعد دیپلوی Api، بعد Permission Sync در پنل.
-- به ردیف‌های موجود دست نمی‌زند؛ نقش‌های خودِ ارائه‌دهنده‌ها همان‌طور می‌مانند.
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20261107065357_ProviderRoleTemplates')
BEGIN
    IF EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_ProviderUserRoles_ProviderId_Name' AND object_id = OBJECT_ID(N'[ProviderUserRoles]'))
        DROP INDEX [IX_ProviderUserRoles_ProviderId_Name] ON [ProviderUserRoles];
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = N'FK_ProviderUserRoles_Providers_ProviderId')
        ALTER TABLE [ProviderUserRoles] DROP CONSTRAINT [FK_ProviderUserRoles_Providers_ProviderId];

    ALTER TABLE [ProviderUserRoles] ALTER COLUMN [ProviderId] bigint NULL;

    ALTER TABLE [ProviderUserRoles] ADD CONSTRAINT [FK_ProviderUserRoles_Providers_ProviderId]
        FOREIGN KEY ([ProviderId]) REFERENCES [Providers] ([Id]) ON DELETE NO ACTION;

    IF COL_LENGTH(N'ProviderUserRoles', N'Priority') IS NULL
        ALTER TABLE [ProviderUserRoles] ADD [Priority] int NOT NULL DEFAULT 0;
    IF COL_LENGTH(N'ProviderUserRoles', N'Scope') IS NULL
        ALTER TABLE [ProviderUserRoles] ADD [Scope] nvarchar(20) NULL;

    EXEC(N'CREATE UNIQUE INDEX [IX_ProviderUserRoles_ProviderId_Name] ON [ProviderUserRoles] ([ProviderId], [Name]) WHERE [Deleted] = 0 AND [ProviderId] IS NOT NULL');
    EXEC(N'CREATE UNIQUE INDEX [IX_ProviderUserRoles_Template_Scope_Name] ON [ProviderUserRoles] ([Scope], [Name]) WHERE [Deleted] = 0 AND [ProviderId] IS NULL');
    EXEC(N'ALTER TABLE [ProviderUserRoles] ADD CONSTRAINT [CK_ProviderUserRoles_TemplateScope] CHECK (([ProviderId] IS NULL AND [Scope] IS NOT NULL) OR ([ProviderId] IS NOT NULL AND [Scope] IS NULL))');

    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261107065357_ProviderRoleTemplates', N'9.0.0');
END;
COMMIT;
GO

-- ------------------------------------------------------------------ بخش ۲: نقش‌های آماده
BEGIN TRANSACTION;

DECLARE @Roles TABLE ([Scope] nvarchar(20), [Priority] int, [Name] nvarchar(100), [Description] nvarchar(500));
INSERT INTO @Roles ([Scope], [Priority], [Name], [Description]) VALUES
    (N'Clinic', 1, N'مسئول کلینیک', N'دسترسی کامل به همه‌ی بخش‌های کلینیک و مشاهده‌ی مالی'),
    (N'Clinic', 2, N'پذیرش کلینیک', N'مدیریت نوبت‌ها؛ مشاهده‌ی خدمات، مشاوره و پروفایل'),
    (N'Clinic', 3, N'مسئول مشاوره آنلاین کلینیک', N'مدیریت مشاوره‌ها؛ مشاهده‌ی نوبت‌ها'),
    (N'Clinic', 4, N'حسابدار کلینیک', N'مشاهده‌ی مالی، نوبت‌ها و مشاوره‌ها'),
    (N'Clinic', 5, N'مسئول پرسنل کلینیک', N'مدیریت پرسنل؛ مشاهده‌ی نوبت‌ها'),
    (N'Clinic', 6, N'مسئول پروفایل و خدمات کلینیک', N'مدیریت پروفایل، خدمات و نظرات'),
    (N'Store', 1, N'مسئول فروشگاه', N'دسترسی کامل به همه‌ی بخش‌های فروشگاه و مشاهده‌ی مالی'),
    (N'Store', 2, N'اپراتور سفارش فروشگاه', N'مدیریت سفارش‌ها؛ مشاهده‌ی کالاها'),
    (N'Store', 3, N'مسئول کالا و انبار', N'مدیریت کالاها؛ مشاهده‌ی سفارش‌ها'),
    (N'Store', 4, N'مسئول تخفیف و فروش', N'مدیریت تخفیف‌ها؛ مشاهده‌ی کالاها و سفارش‌ها'),
    (N'Store', 5, N'حسابدار فروشگاه', N'مشاهده‌ی مالی و سفارش‌ها'),
    (N'Store', 6, N'پاسخ‌گوی نظرات فروشگاه', N'مدیریت نظرات؛ مشاهده‌ی کالاها'),
    (N'Groomer', 1, N'مسئول آرایشگاه', N'دسترسی کامل به همه‌ی بخش‌های آرایشگاه و مشاهده‌ی مالی'),
    (N'Groomer', 2, N'پذیرش آرایشگاه', N'مدیریت نوبت‌ها؛ مشاهده‌ی خدمات و پروفایل'),
    (N'Groomer', 3, N'مسئول نمونه‌کار و محتوای آرایشگاه', N'مدیریت نمونه‌کار، پروفایل و نظرات'),
    (N'Groomer', 4, N'حسابدار آرایشگاه', N'مشاهده‌ی مالی و نوبت‌ها'),
    (N'Groomer', 5, N'مسئول پرسنل آرایشگاه', N'مدیریت پرسنل؛ مشاهده‌ی نوبت‌ها'),
    (N'Coach', 1, N'مسئول مربی', N'دسترسی کامل به همه‌ی بخش‌های مربی و مشاهده‌ی مالی'),
    (N'Coach', 2, N'هماهنگ‌کننده جلسات مربی', N'مدیریت نوبت‌ها؛ مشاهده‌ی خدمات و مشاوره'),
    (N'Coach', 3, N'مسئول مشاوره آنلاین مربی', N'مدیریت مشاوره‌ها؛ مشاهده‌ی نوبت‌ها'),
    (N'Coach', 4, N'حسابدار مربی', N'مشاهده‌ی مالی و نوبت‌ها'),
    (N'Coach', 5, N'مسئول پرسنل مربی', N'مدیریت پرسنل؛ مشاهده‌ی نوبت‌ها'),
    (N'School', 1, N'مسئول مدرسه', N'دسترسی کامل به همه‌ی بخش‌های مدرسه و مشاهده‌ی مالی'),
    (N'School', 2, N'مسئول دوره‌های مدرسه', N'مدیریت دوره‌ها؛ مشاهده‌ی ثبت‌نام‌ها'),
    (N'School', 3, N'مسئول ثبت‌نام مدرسه', N'مدیریت ثبت‌نام‌ها؛ مشاهده‌ی دوره‌ها'),
    (N'School', 4, N'حسابدار مدرسه', N'مشاهده‌ی مالی و ثبت‌نام‌ها'),
    (N'Pansion', 1, N'مسئول پانسیون', N'دسترسی کامل به همه‌ی بخش‌های پانسیون و مشاهده‌ی مالی'),
    (N'Pansion', 2, N'پذیرش پانسیون', N'مدیریت رزروها؛ مشاهده‌ی پروفایل'),
    (N'Pansion', 3, N'حسابدار پانسیون', N'مشاهده‌ی مالی و رزروها'),
    (N'Pansion', 4, N'پاسخ‌گوی نظرات پانسیون', N'مدیریت نظرات؛ مشاهده‌ی رزروها');

DECLARE @Perms TABLE ([Scope] nvarchar(20), [Name] nvarchar(100), [PermissionKey] nvarchar(100));
INSERT INTO @Perms ([Scope], [Name], [PermissionKey]) VALUES
    (N'Clinic', N'مسئول کلینیک', N'Clinic.Profile.Manage'),
    (N'Clinic', N'مسئول کلینیک', N'Clinic.Service.Manage'),
    (N'Clinic', N'مسئول کلینیک', N'Clinic.Reserve.Manage'),
    (N'Clinic', N'مسئول کلینیک', N'Clinic.Consultation.Manage'),
    (N'Clinic', N'مسئول کلینیک', N'Clinic.Review.Manage'),
    (N'Clinic', N'مسئول کلینیک', N'Clinic.Staff.Manage'),
    (N'Clinic', N'مسئول کلینیک', N'Clinic.Finance.View'),
    (N'Clinic', N'پذیرش کلینیک', N'Clinic.Reserve.Manage'),
    (N'Clinic', N'پذیرش کلینیک', N'Clinic.Service.View'),
    (N'Clinic', N'پذیرش کلینیک', N'Clinic.Consultation.View'),
    (N'Clinic', N'پذیرش کلینیک', N'Clinic.Profile.View'),
    (N'Clinic', N'مسئول مشاوره آنلاین کلینیک', N'Clinic.Consultation.Manage'),
    (N'Clinic', N'مسئول مشاوره آنلاین کلینیک', N'Clinic.Reserve.View'),
    (N'Clinic', N'حسابدار کلینیک', N'Clinic.Finance.View'),
    (N'Clinic', N'حسابدار کلینیک', N'Clinic.Reserve.View'),
    (N'Clinic', N'حسابدار کلینیک', N'Clinic.Consultation.View'),
    (N'Clinic', N'مسئول پرسنل کلینیک', N'Clinic.Staff.Manage'),
    (N'Clinic', N'مسئول پرسنل کلینیک', N'Clinic.Reserve.View'),
    (N'Clinic', N'مسئول پروفایل و خدمات کلینیک', N'Clinic.Profile.Manage'),
    (N'Clinic', N'مسئول پروفایل و خدمات کلینیک', N'Clinic.Service.Manage'),
    (N'Clinic', N'مسئول پروفایل و خدمات کلینیک', N'Clinic.Review.Manage'),
    (N'Store', N'مسئول فروشگاه', N'Store.Profile.Manage'),
    (N'Store', N'مسئول فروشگاه', N'Store.Product.Manage'),
    (N'Store', N'مسئول فروشگاه', N'Store.Order.Manage'),
    (N'Store', N'مسئول فروشگاه', N'Store.Discount.Manage'),
    (N'Store', N'مسئول فروشگاه', N'Store.Review.Manage'),
    (N'Store', N'مسئول فروشگاه', N'Store.Finance.View'),
    (N'Store', N'اپراتور سفارش فروشگاه', N'Store.Order.Manage'),
    (N'Store', N'اپراتور سفارش فروشگاه', N'Store.Product.View'),
    (N'Store', N'مسئول کالا و انبار', N'Store.Product.Manage'),
    (N'Store', N'مسئول کالا و انبار', N'Store.Order.View'),
    (N'Store', N'مسئول تخفیف و فروش', N'Store.Discount.Manage'),
    (N'Store', N'مسئول تخفیف و فروش', N'Store.Product.View'),
    (N'Store', N'مسئول تخفیف و فروش', N'Store.Order.View'),
    (N'Store', N'حسابدار فروشگاه', N'Store.Finance.View'),
    (N'Store', N'حسابدار فروشگاه', N'Store.Order.View'),
    (N'Store', N'پاسخ‌گوی نظرات فروشگاه', N'Store.Review.Manage'),
    (N'Store', N'پاسخ‌گوی نظرات فروشگاه', N'Store.Product.View'),
    (N'Groomer', N'مسئول آرایشگاه', N'Groomer.Profile.Manage'),
    (N'Groomer', N'مسئول آرایشگاه', N'Groomer.Service.Manage'),
    (N'Groomer', N'مسئول آرایشگاه', N'Groomer.Reserve.Manage'),
    (N'Groomer', N'مسئول آرایشگاه', N'Groomer.Portfolio.Manage'),
    (N'Groomer', N'مسئول آرایشگاه', N'Groomer.Review.Manage'),
    (N'Groomer', N'مسئول آرایشگاه', N'Groomer.Staff.Manage'),
    (N'Groomer', N'مسئول آرایشگاه', N'Groomer.Finance.View'),
    (N'Groomer', N'پذیرش آرایشگاه', N'Groomer.Reserve.Manage'),
    (N'Groomer', N'پذیرش آرایشگاه', N'Groomer.Service.View'),
    (N'Groomer', N'پذیرش آرایشگاه', N'Groomer.Profile.View'),
    (N'Groomer', N'مسئول نمونه‌کار و محتوای آرایشگاه', N'Groomer.Portfolio.Manage'),
    (N'Groomer', N'مسئول نمونه‌کار و محتوای آرایشگاه', N'Groomer.Profile.Manage'),
    (N'Groomer', N'مسئول نمونه‌کار و محتوای آرایشگاه', N'Groomer.Review.Manage'),
    (N'Groomer', N'حسابدار آرایشگاه', N'Groomer.Finance.View'),
    (N'Groomer', N'حسابدار آرایشگاه', N'Groomer.Reserve.View'),
    (N'Groomer', N'مسئول پرسنل آرایشگاه', N'Groomer.Staff.Manage'),
    (N'Groomer', N'مسئول پرسنل آرایشگاه', N'Groomer.Reserve.View'),
    (N'Coach', N'مسئول مربی', N'Coach.Profile.Manage'),
    (N'Coach', N'مسئول مربی', N'Coach.Service.Manage'),
    (N'Coach', N'مسئول مربی', N'Coach.Reserve.Manage'),
    (N'Coach', N'مسئول مربی', N'Coach.Consultation.Manage'),
    (N'Coach', N'مسئول مربی', N'Coach.Review.Manage'),
    (N'Coach', N'مسئول مربی', N'Coach.Staff.Manage'),
    (N'Coach', N'مسئول مربی', N'Coach.Finance.View'),
    (N'Coach', N'هماهنگ‌کننده جلسات مربی', N'Coach.Reserve.Manage'),
    (N'Coach', N'هماهنگ‌کننده جلسات مربی', N'Coach.Service.View'),
    (N'Coach', N'هماهنگ‌کننده جلسات مربی', N'Coach.Consultation.View'),
    (N'Coach', N'مسئول مشاوره آنلاین مربی', N'Coach.Consultation.Manage'),
    (N'Coach', N'مسئول مشاوره آنلاین مربی', N'Coach.Reserve.View'),
    (N'Coach', N'حسابدار مربی', N'Coach.Finance.View'),
    (N'Coach', N'حسابدار مربی', N'Coach.Reserve.View'),
    (N'Coach', N'مسئول پرسنل مربی', N'Coach.Staff.Manage'),
    (N'Coach', N'مسئول پرسنل مربی', N'Coach.Reserve.View'),
    (N'School', N'مسئول مدرسه', N'School.Profile.Manage'),
    (N'School', N'مسئول مدرسه', N'School.Course.Manage'),
    (N'School', N'مسئول مدرسه', N'School.Reserve.Manage'),
    (N'School', N'مسئول مدرسه', N'School.Finance.View'),
    (N'School', N'مسئول دوره‌های مدرسه', N'School.Course.Manage'),
    (N'School', N'مسئول دوره‌های مدرسه', N'School.Reserve.View'),
    (N'School', N'مسئول ثبت‌نام مدرسه', N'School.Reserve.Manage'),
    (N'School', N'مسئول ثبت‌نام مدرسه', N'School.Course.View'),
    (N'School', N'حسابدار مدرسه', N'School.Finance.View'),
    (N'School', N'حسابدار مدرسه', N'School.Reserve.View'),
    (N'Pansion', N'مسئول پانسیون', N'Pansion.Profile.Manage'),
    (N'Pansion', N'مسئول پانسیون', N'Pansion.Reserve.Manage'),
    (N'Pansion', N'مسئول پانسیون', N'Pansion.Review.Manage'),
    (N'Pansion', N'مسئول پانسیون', N'Pansion.Finance.View'),
    (N'Pansion', N'پذیرش پانسیون', N'Pansion.Reserve.Manage'),
    (N'Pansion', N'پذیرش پانسیون', N'Pansion.Profile.View'),
    (N'Pansion', N'حسابدار پانسیون', N'Pansion.Finance.View'),
    (N'Pansion', N'حسابدار پانسیون', N'Pansion.Reserve.View'),
    (N'Pansion', N'پاسخ‌گوی نظرات پانسیون', N'Pansion.Review.Manage'),
    (N'Pansion', N'پاسخ‌گوی نظرات پانسیون', N'Pansion.Reserve.View');

-- نقشی که قبلاً با همین نام در همین نمایندگی ساخته شده (مثلاً ادمین ویرایشش کرده) دوباره ساخته/بازنویسی نمی‌شود
INSERT INTO [ProviderUserRoles] ([ProviderId], [Scope], [Priority], [Name], [Description], [Deleted], [CreateDate])
SELECT NULL, r.[Scope], r.[Priority], r.[Name], r.[Description], 0, GETDATE()
FROM @Roles r
WHERE NOT EXISTS (
    SELECT 1 FROM [ProviderUserRoles] x
    WHERE x.[ProviderId] IS NULL AND x.[Deleted] = 0 AND x.[Scope] = r.[Scope] AND x.[Name] = r.[Name]);

-- دسترسی‌ها فقط برای نقش آماده‌ای که هنوز هیچ دسترسی‌ای ندارد (تازه ساخته شده) — دسترسی‌هایی که ادمین بعداً عوض کرده دست نمی‌خورد
INSERT INTO [ProviderUserRolePermissions] ([ProviderUserRoleId], [PermissionKey])
SELECT x.[Id], p.[PermissionKey]
FROM @Perms p
JOIN [ProviderUserRoles] x ON x.[ProviderId] IS NULL AND x.[Deleted] = 0 AND x.[Scope] = p.[Scope] AND x.[Name] = p.[Name]
WHERE NOT EXISTS (SELECT 1 FROM [ProviderUserRolePermissions] e WHERE e.[ProviderUserRoleId] = x.[Id]);

COMMIT;
GO

-- بررسی:
-- SELECT r.Scope, r.Priority, r.Name, COUNT(p.Id) AS Perms FROM ProviderUserRoles r LEFT JOIN ProviderUserRolePermissions p ON p.ProviderUserRoleId = r.Id
-- WHERE r.ProviderId IS NULL AND r.Deleted = 0 GROUP BY r.Scope, r.Priority, r.Name ORDER BY r.Scope, r.Priority;
