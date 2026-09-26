/*
  Ticket code contract audit and repair

  IMPORTANT
  - Run once with @ApplyRepair = 0 to inspect the result.
  - Change only @ApplyRepair to 1 to insert missing canonical records.
  - The script aborts if an expected Id or Label already belongs to another
    record; it never overwrites or renumbers existing data.

  The current support form must send TicketCategoryId = 55 for "عمومی".
*/

SET NOCOUNT ON;

DECLARE @ApplyRepair bit = 0;

DECLARE @ExpectedGroups TABLE
(
    Id bigint NOT NULL PRIMARY KEY,
    Label nvarchar(200) NOT NULL,
    Name nvarchar(200) NOT NULL
);

INSERT INTO @ExpectedGroups (Id, Label, Name)
VALUES
    (13, N'Ticket_Status',     N'وضعیت تیکت'),
    (14, N'Ticket_Importance', N'اهمیت تیکت'),
    (15, N'Ticket_Category',   N'دسته‌بندی تیکت');

DECLARE @ExpectedCodes TABLE
(
    Id bigint NOT NULL PRIMARY KEY,
    Label nvarchar(200) NOT NULL,
    Value nvarchar(200) NOT NULL,
    CodeGroupId bigint NOT NULL,
    Priority int NOT NULL,
    Name nvarchar(200) NOT NULL
);

INSERT INTO @ExpectedCodes (Id, Label, Value, CodeGroupId, Priority, Name)
VALUES
    (49, N'TicketStatus_Waiting',          N'TicketStatus_Waiting',          13, 1, N'در انتظار پاسخ'),
    (50, N'TicketStatus_Answered',         N'TicketStatus_Answered',         13, 2, N'پاسخ داده شده'),
    (51, N'TicketStatus_Close',            N'TicketStatus_Close',            13, 3, N'بسته شده'),
    (52, N'TicketImportance_Normal',       N'TicketImportance_Normal',       14, 1, N'عادی'),
    (53, N'TicketImportance_Important',    N'TicketImportance_Important',    14, 2, N'مهم'),
    (54, N'TicketImportance_VeryImportant',N'TicketImportance_VeryImportant',14, 3, N'خیلی مهم'),
    (55, N'TicketCategory_General',        N'TicketCategory_General',        15, 1, N'عمومی'),
    (56, N'TicketCategory_TechnicalSupport', N'TicketCategory_TechnicalSupport', 15, 2, N'پشتیبانی فنی'),
    (57, N'TicketCategory_Financial',      N'TicketCategory_Financial',      15, 3, N'مالی'),
    (58, N'TicketCategory_Account',        N'TicketCategory_Account',        15, 4, N'حساب کاربری'),
    (59, N'TicketCategory_Product',        N'TicketCategory_Product',        15, 5, N'محصول'),
    (60, N'TicketCategory_Feedback',       N'TicketCategory_Feedback',       15, 6, N'پیشنهاد و انتقاد'),
    (61, N'TicketCategory_Other',          N'TicketCategory_Other',          15, 7, N'سایر');

-- Audit output. Every row must be OK before the support form can use it.
SELECT
    e.Id AS ExpectedId,
    e.Label AS ExpectedLabel,
    e.CodeGroupId AS ExpectedCodeGroupId,
    c.Id AS ActualId,
    c.Label AS ActualLabel,
    c.CodeGroupId AS ActualCodeGroupId,
    c.Active,
    CASE
        WHEN c.Id IS NULL THEN N'MISSING'
        WHEN c.Label <> e.Label THEN N'ID_CONFLICT'
        WHEN c.CodeGroupId <> e.CodeGroupId THEN N'GROUP_CONFLICT'
        WHEN c.Active = 0 THEN N'INACTIVE'
        ELSE N'OK'
    END AS AuditStatus
FROM @ExpectedCodes e
LEFT JOIN dbo.Codes c ON c.Id = e.Id
ORDER BY e.Id;

-- These are the IDs currently hard-coded in the support form. They are not
-- part of the canonical ticket category contract and should normally be empty.
SELECT
    c.Id,
    c.Label,
    c.Name,
    c.CodeGroupId,
    cg.Label AS CodeGroupLabel,
    c.Active
FROM dbo.Codes c
LEFT JOIN dbo.CodeGroups cg ON cg.Id = c.CodeGroupId
WHERE c.Id IN (10139, 10140, 10141, 10142)
ORDER BY c.Id;

IF @ApplyRepair = 0
    RETURN;

BEGIN TRY
    BEGIN TRANSACTION;

    -- A conflicting key means it is unsafe to guess or modify existing data.
    IF EXISTS
    (
        SELECT 1
        FROM @ExpectedGroups source
        INNER JOIN dbo.CodeGroups target ON target.Id = source.Id
        WHERE target.Label <> source.Label
    )
    BEGIN
        THROW 51000, 'A required CodeGroup ID is assigned to another label.', 1;
    END;

    IF EXISTS
    (
        SELECT 1
        FROM @ExpectedGroups source
        INNER JOIN dbo.CodeGroups target ON target.Label = source.Label
        WHERE target.Id <> source.Id
    )
    BEGIN
        THROW 51001, 'A required CodeGroup label is assigned to another ID.', 1;
    END;

    SET IDENTITY_INSERT dbo.CodeGroups ON;

    INSERT INTO dbo.CodeGroups (Id, Label, Name)
    SELECT source.Id, source.Label, source.Name
    FROM @ExpectedGroups source
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.CodeGroups target
        WHERE target.Id = source.Id
    );

    SET IDENTITY_INSERT dbo.CodeGroups OFF;

    IF EXISTS
    (
        SELECT 1
        FROM @ExpectedCodes source
        INNER JOIN dbo.Codes target ON target.Id = source.Id
        WHERE target.Label <> source.Label
           OR target.CodeGroupId <> source.CodeGroupId
    )
    BEGIN
        THROW 51002, 'A required Code ID is assigned to another ticket code.', 1;
    END;

    IF EXISTS
    (
        SELECT 1
        FROM @ExpectedCodes source
        INNER JOIN dbo.Codes target ON target.Label = source.Label
        WHERE target.Id <> source.Id
    )
    BEGIN
        THROW 51003, 'A required Code label is assigned to another ID.', 1;
    END;

    SET IDENTITY_INSERT dbo.Codes ON;

    INSERT INTO dbo.Codes (Id, Label, Value, CodeGroupId, Priority, Active, Name)
    SELECT source.Id, source.Label, source.Value, source.CodeGroupId,
           source.Priority, 1, source.Name
    FROM @ExpectedCodes source
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.Codes target
        WHERE target.Id = source.Id
    );

    SET IDENTITY_INSERT dbo.Codes OFF;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;

-- Verification after repair.
SELECT
    e.Id AS ExpectedId,
    c.Id AS ActualId,
    c.Label,
    c.CodeGroupId,
    c.Active
FROM @ExpectedCodes e
LEFT JOIN dbo.Codes c ON c.Id = e.Id
ORDER BY e.Id;
