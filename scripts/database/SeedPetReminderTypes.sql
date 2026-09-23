USE [pastil_db];
GO

/* ReminderTypes is replaced with vaccine-only reminder types. */
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    /* Remove dependent reminder records before their referenced types. */
    DELETE FROM dbo.Reminders;
    DELETE FROM dbo.ReminderTypes;
    DBCC CHECKIDENT (N'dbo.ReminderTypes', RESEED, 0) WITH NO_INFOMSGS;

    /*
       ترتیب رکوردها، ترتیب نمایش/اهمیت پیش‌فرض است:
       سگ ← گربه ← سایر پت‌ها.

       واکسن‌های «با نظر دامپزشک» به شرایط زندگی، محل زندگی یا سفر وابسته‌اند.
       برای ماهی، خزنده، لاک‌پشت، همستر، خوکچه هندی، شینچیلا، موش فانتزی و
       جوجه‌تیغی واکسن روتینِ توصیه‌شده‌ای وجود ندارد؛ بنابراین عمداً ورودی
       گمراه‌کننده‌ای برای آن‌ها ثبت نشده است.
    */
    SET IDENTITY_INSERT dbo.ReminderTypes ON;

    INSERT INTO dbo.ReminderTypes ([Id], [Deleted], [Name])
    VALUES
        -- سگ: واکسن‌های پایه
        (1,  0, N'واکسن چندگانه سگ (دیستمپر، آدنوویروس، پاروو و پاراآنفلوانزا)'),
        (2,  0, N'واکسن هاری سگ'),
        (3,  0, N'واکسن لپتوسپیروز سگ'),

        -- سگ: واکسن‌های وابسته به ریسک
        (4,  0, N'واکسن بوردتلا / سرفه کنل سگ (با نظر دامپزشک)'),
        (5,  0, N'واکسن آنفلوانزای سگ (با نظر دامپزشک)'),
        (6,  0, N'واکسن لایم سگ (در منطقهٔ اندمیک یا سفر، با نظر دامپزشک)'),
        (7,  0, N'واکسن لیشمانیوز سگ (در منطقهٔ اندمیک، با نظر دامپزشک)'),

        -- گربه: واکسن‌های پایه
        (8,  0, N'واکسن سه‌گانه گربه (پن‌لوکوپنی، هرپس و کالیسی)'),
        (9,  0, N'واکسن هاری گربه'),
        (10, 0, N'واکسن لوسمی گربه (FeLV)'),

        -- گربه: واکسن‌های وابسته به ریسک
        (11, 0, N'واکسن کلامیدیای گربه (با نظر دامپزشک)'),
        (12, 0, N'واکسن بوردتلای گربه (با نظر دامپزشک)'),

        -- سایر پت‌ها
        (13, 0, N'واکسن بیماری خون‌ریزی‌دهندهٔ ویروسی خرگوش (RHDV1/RHDV2)'),
        (14, 0, N'واکسن میکسوماتوز خرگوش (در منطقهٔ درگیر، با نظر دامپزشک)'),
        (15, 0, N'واکسن دیستمپر فرت'),
        (16, 0, N'واکسن هاری فرت'),
        (17, 0, N'واکسن نیوکاسل / پارامیکسوویروس کبوتر و پرندگان زینتی (با نظر دامپزشک)'),
        (18, 0, N'واکسن پلیوماویروس طوطی‌سانان (با نظر دامپزشک)'),
        (19, 0, N'واکسن آبلهٔ پرندگان زینتی (با نظر دامپزشک)');

    SET IDENTITY_INSERT dbo.ReminderTypes OFF;
    DBCC CHECKIDENT (N'dbo.ReminderTypes', RESEED, 19) WITH NO_INFOMSGS;

    COMMIT TRANSACTION;

    SELECT [Id], [Name], [Deleted]
    FROM dbo.ReminderTypes
    ORDER BY [Id];
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    /* Avoid leaving IDENTITY_INSERT enabled when an insert fails. */
    BEGIN TRY
        SET IDENTITY_INSERT dbo.ReminderTypes OFF;
    END TRY
    BEGIN CATCH
    END CATCH;

    ;THROW;
END CATCH;
