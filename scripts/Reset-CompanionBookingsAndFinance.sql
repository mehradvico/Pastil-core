/*
  Reset-CompanionBookingsAndFinance.sql
  ---------------------------------------------------------------------------
  Removes ALL bookings (reservations) and the financial records derived from
  them for ONE clinic/companion, keeping the companion, its services,
  packages, school and team. Written for CompanionId = 7 (محمد قادرپناه).

  NOTHING IS COMMITTED BY DEFAULT: @Commit = 0 runs everything inside a
  transaction and ROLLS BACK at the end. Read the plan it prints, then set
  @Commit = 1 and run again.

  BEFORE COMMITTING: take a backup (Liara panel or
  BACKUP DATABASE [pastil_db] TO DISK = ... WITH COPY_ONLY). Deleted rows
  cannot be recovered afterwards.

  What it does
    - Roots (deleted): CompanionReserves of the companion's services,
      ConsultationPurchases, CompanionInsurancePackageSales, PansionReserves,
      SchoolReserves (each switchable below) and the companion's Settlements.
    - Child rows are discovered at run time from sys.foreign_keys:
        * child column NULLABLE            -> set to NULL (link removed, row kept)
        * child column NOT NULL            -> child rows deleted (recursively)
        * ledger/financial/medical tables of OTHER people (#Keep list below)
          are never deleted: NULLABLE link -> set NULL, NOT NULL link -> the
          script stops BEFORE changing anything ("BLOCKED") so you can decide.
    - NOT touched: Payments, customers' Wallet rows (only their link to the
      deleted booking is cleared), packages, services, schools, courses.
      Customers who paid are NOT refunded by this script.
    - Multi-column FKs are ignored (none expected); if one exists the DELETE
      fails and the whole transaction rolls back.

  Row counts in the plan can double-count a table reached through two paths.
*/

SET NOCOUNT ON;

IF OBJECT_ID('tempdb..#Cfg')  IS NOT NULL DROP TABLE #Cfg;
IF OBJECT_ID('tempdb..#Log')  IS NOT NULL DROP TABLE #Log;
IF OBJECT_ID('tempdb..#Keep') IS NOT NULL DROP TABLE #Keep;
IF OBJECT_ID('tempdb..#Cascade') IS NOT NULL DROP PROCEDURE #Cascade;

CREATE TABLE #Cfg  (ExecuteIt bit NOT NULL);
INSERT #Cfg VALUES (0);
CREATE TABLE #Log  (Seq int IDENTITY(1,1), Depth int, Action varchar(12), Tbl nvarchar(300), Col sysname NULL, [Rows] int, Note nvarchar(300) NULL);
CREATE TABLE #Keep (Name sysname PRIMARY KEY);
-- tables that hold OTHER people's money/records: never delete their rows
INSERT #Keep VALUES (N'Wallets'), (N'Payments'), (N'Trips'), (N'OnlinePrescriptions'),
                    (N'PastilAiSubscriptions'), (N'ClubCoupons'), (N'ClubRewardCostTransactions');
GO

CREATE PROCEDURE #Cascade @Tbl nvarchar(300), @Pred nvarchar(max), @Depth int
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Execute bit = (SELECT TOP 1 ExecuteIt FROM #Cfg);
    IF @Depth > 10 THROW 50001, N'Cascade nested too deep (FK cycle?). Nothing was changed by this call.', 1;

    DECLARE @ChildFull nvarchar(300), @ChildName sysname, @Col sysname, @RefCol sysname, @Nullable bit,
            @sql nvarchar(max), @n int, @childPred nvarchar(max), @NextDepth int, @parentName sysname;
    SET @parentName = PARSENAME(@Tbl, 1);

    DECLARE c CURSOR LOCAL FAST_FORWARD FOR
        SELECT QUOTENAME(SCHEMA_NAME(ct.schema_id)) + N'.' + QUOTENAME(ct.name), ct.name, cc.name, rc.name, cc.is_nullable
        FROM sys.foreign_keys f
        JOIN sys.foreign_key_columns k ON k.constraint_object_id = f.object_id
        JOIN sys.tables  ct ON ct.object_id = f.parent_object_id
        JOIN sys.columns cc ON cc.object_id = k.parent_object_id AND cc.column_id = k.parent_column_id
        JOIN sys.columns rc ON rc.object_id = k.referenced_object_id AND rc.column_id = k.referenced_column_id
        WHERE f.referenced_object_id = OBJECT_ID(@Tbl)
          AND (SELECT COUNT(*) FROM sys.foreign_key_columns x WHERE x.constraint_object_id = f.object_id) = 1;

    OPEN c;
    FETCH NEXT FROM c INTO @ChildFull, @ChildName, @Col, @RefCol, @Nullable;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @childPred = QUOTENAME(@Col) + N' IN (SELECT ' + QUOTENAME(@RefCol) + N' FROM ' + @Tbl + N' WHERE ' + @Pred + N')';
        SET @sql = N'SELECT @n = COUNT(*) FROM ' + @ChildFull + N' WHERE ' + @childPred;
        EXEC sp_executesql @sql, N'@n int OUTPUT', @n = @n OUTPUT;

        IF @n > 0
        BEGIN
            IF @Nullable = 1
            BEGIN
                INSERT #Log VALUES (@Depth, 'SET NULL', @ChildFull, @Col, @n,
                    CASE WHEN EXISTS (SELECT 1 FROM #Keep WHERE Name = @ChildName) THEN N'kept (other people''s record)' ELSE N'link removed, row kept' END);
                IF @Execute = 1
                BEGIN
                    SET @sql = N'UPDATE ' + @ChildFull + N' SET ' + QUOTENAME(@Col) + N' = NULL WHERE ' + @childPred;
                    EXEC (@sql);
                END
            END
            ELSE IF EXISTS (SELECT 1 FROM #Keep WHERE Name = @ChildName) OR @ChildName = @parentName
            BEGIN
                INSERT #Log VALUES (@Depth, 'BLOCKED', @ChildFull, @Col, @n,
                    N'NOT NULL link to a table that must be kept (or self-reference): decide manually');
            END
            ELSE
            BEGIN
                SET @NextDepth = @Depth + 1;
                EXEC #Cascade @ChildFull, @childPred, @NextDepth;
            END
        END

        FETCH NEXT FROM c INTO @ChildFull, @ChildName, @Col, @RefCol, @Nullable;
    END
    CLOSE c; DEALLOCATE c;

    SET @sql = N'SELECT @n = COUNT(*) FROM ' + @Tbl + N' WHERE ' + @Pred;
    EXEC sp_executesql @sql, N'@n int OUTPUT', @n = @n OUTPUT;
    IF @n > 0
    BEGIN
        INSERT #Log VALUES (@Depth, 'DELETE', @Tbl, NULL, @n, NULL);
        IF @Execute = 1
        BEGIN
            SET @sql = N'DELETE FROM ' + @Tbl + N' WHERE ' + @Pred;
            EXEC (@sql);
        END
    END
END
GO

-- ===========================================================================
-- SETTINGS
-- ===========================================================================
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @CompanionId          bigint = 7;
DECLARE @Commit               bit    = 1;   -- 0 = preview + rollback, 1 = COMMIT  (set to 1 on 2026-10-01 after the preview was reviewed)
DECLARE @IncludeServiceReserves bit  = 1;   -- CompanionReserves (vet/grooming/online... services)
DECLARE @IncludeConsultations bit    = 1;   -- ConsultationPurchases (online consultation packages)
DECLARE @IncludeInsuranceSales bit   = 1;   -- CompanionInsurancePackageSales
DECLARE @IncludePansion       bit    = 1;   -- PansionReserves
DECLARE @IncludeSchoolReserves bit   = 1;   -- SchoolReserves (school stays, only bookings go)
DECLARE @IncludeSettlements   bit    = 1;   -- Settlements of this companion (payout records)

-- ===========================================================================
-- Sanity checks + identity confirmation
-- ===========================================================================
PRINT N'Database: ' + DB_NAME();
IF OBJECT_ID(N'dbo.Companions') IS NULL OR OBJECT_ID(N'dbo.CompanionReserves') IS NULL
    THROW 50010, N'Expected tables not found - wrong database?', 1;
IF NOT EXISTS (SELECT 1 FROM dbo.Companions WHERE Id = @CompanionId)
    THROW 50011, N'CompanionId not found.', 1;

SELECT c.Id AS CompanionId, c.Name AS CompanionName, c.OwnerId, u.FirstName, u.LastName
FROM dbo.Companions c LEFT JOIN dbo.Users u ON u.Id = c.OwnerId
WHERE c.Id = @CompanionId;   -- CHECK THIS IS THE RIGHT CLINIC BEFORE COMMITTING

-- ===========================================================================
-- Build the root list
-- ===========================================================================
DECLARE @Roots TABLE (Seq int IDENTITY(1,1), Label nvarchar(100), Tbl nvarchar(300), Pred nvarchar(max));
DECLARE @id nvarchar(30) = CONVERT(nvarchar(30), @CompanionId);
DECLARE @fk sysname, @fk2 sysname;

IF @IncludeServiceReserves = 1
BEGIN
    IF OBJECT_ID(N'dbo.CompanionAssistances') IS NULL OR COL_LENGTH(N'dbo.CompanionAssistances', N'CompanionId') IS NULL
        THROW 50012, N'CompanionAssistances.CompanionId not found.', 1;
    INSERT @Roots VALUES (N'Service reserves', N'[dbo].[CompanionReserves]',
        N'[CompanionAssistanceId] IN (SELECT [Id] FROM [dbo].[CompanionAssistances] WHERE [CompanionId] = ' + @id + N')');
END

IF @IncludeConsultations = 1 AND OBJECT_ID(N'dbo.ConsultationPurchases') IS NOT NULL
    INSERT @Roots VALUES (N'Consultation purchases', N'[dbo].[ConsultationPurchases]', N'[CompanionId] = ' + @id);

IF @IncludeInsuranceSales = 1 AND OBJECT_ID(N'dbo.CompanionInsurancePackageSales') IS NOT NULL
BEGIN
    SET @fk = NULL;
    SELECT TOP 1 @fk = cc.name FROM sys.foreign_keys f
      JOIN sys.foreign_key_columns k ON k.constraint_object_id = f.object_id
      JOIN sys.columns cc ON cc.object_id = k.parent_object_id AND cc.column_id = k.parent_column_id
     WHERE f.parent_object_id = OBJECT_ID(N'dbo.CompanionInsurancePackageSales')
       AND f.referenced_object_id = OBJECT_ID(N'dbo.CompanionInsurancePackages');
    IF @fk IS NOT NULL
        INSERT @Roots VALUES (N'Insurance package sales', N'[dbo].[CompanionInsurancePackageSales]',
            QUOTENAME(@fk) + N' IN (SELECT [Id] FROM [dbo].[CompanionInsurancePackages] WHERE [CompanionId] = ' + @id + N')');
    ELSE PRINT N'NOTE: could not find the FK CompanionInsurancePackageSales -> CompanionInsurancePackages; skipped.';
END

IF @IncludePansion = 1 AND OBJECT_ID(N'dbo.PansionReserves') IS NOT NULL
BEGIN
    SET @fk = NULL;
    SELECT TOP 1 @fk = cc.name FROM sys.foreign_keys f
      JOIN sys.foreign_key_columns k ON k.constraint_object_id = f.object_id
      JOIN sys.columns cc ON cc.object_id = k.parent_object_id AND cc.column_id = k.parent_column_id
     WHERE f.parent_object_id = OBJECT_ID(N'dbo.PansionReserves') AND f.referenced_object_id = OBJECT_ID(N'dbo.Pansions');
    IF @fk IS NOT NULL AND COL_LENGTH(N'dbo.Pansions', N'CompanionId') IS NOT NULL
        INSERT @Roots VALUES (N'Pansion reserves', N'[dbo].[PansionReserves]',
            QUOTENAME(@fk) + N' IN (SELECT [Id] FROM [dbo].[Pansions] WHERE [CompanionId] = ' + @id + N')');
    ELSE PRINT N'NOTE: could not link PansionReserves to Pansions.CompanionId; skipped.';
END

IF @IncludeSchoolReserves = 1 AND OBJECT_ID(N'dbo.SchoolReserves') IS NOT NULL AND COL_LENGTH(N'dbo.Schools', N'CompanionId') IS NOT NULL
BEGIN
    SET @fk = NULL;
    SELECT TOP 1 @fk = cc.name FROM sys.foreign_keys f
      JOIN sys.foreign_key_columns k ON k.constraint_object_id = f.object_id
      JOIN sys.columns cc ON cc.object_id = k.parent_object_id AND cc.column_id = k.parent_column_id
     WHERE f.parent_object_id = OBJECT_ID(N'dbo.SchoolReserves') AND f.referenced_object_id = OBJECT_ID(N'dbo.Schools');
    IF @fk IS NOT NULL
        INSERT @Roots VALUES (N'School reserves', N'[dbo].[SchoolReserves]',
            QUOTENAME(@fk) + N' IN (SELECT [Id] FROM [dbo].[Schools] WHERE [CompanionId] = ' + @id + N')');
    ELSE
    BEGIN
        -- two hops: SchoolReserves -> SchoolCourses -> Schools
        SELECT TOP 1 @fk = cc.name FROM sys.foreign_keys f
          JOIN sys.foreign_key_columns k ON k.constraint_object_id = f.object_id
          JOIN sys.columns cc ON cc.object_id = k.parent_object_id AND cc.column_id = k.parent_column_id
         WHERE f.parent_object_id = OBJECT_ID(N'dbo.SchoolReserves') AND f.referenced_object_id = OBJECT_ID(N'dbo.SchoolCourses');
        SELECT TOP 1 @fk2 = cc.name FROM sys.foreign_keys f
          JOIN sys.foreign_key_columns k ON k.constraint_object_id = f.object_id
          JOIN sys.columns cc ON cc.object_id = k.parent_object_id AND cc.column_id = k.parent_column_id
         WHERE f.parent_object_id = OBJECT_ID(N'dbo.SchoolCourses') AND f.referenced_object_id = OBJECT_ID(N'dbo.Schools');
        IF @fk IS NOT NULL AND @fk2 IS NOT NULL
            INSERT @Roots VALUES (N'School reserves', N'[dbo].[SchoolReserves]',
                QUOTENAME(@fk) + N' IN (SELECT [Id] FROM [dbo].[SchoolCourses] WHERE ' + QUOTENAME(@fk2) +
                N' IN (SELECT [Id] FROM [dbo].[Schools] WHERE [CompanionId] = ' + @id + N'))');
        ELSE PRINT N'NOTE: could not link SchoolReserves to Schools.CompanionId; skipped (tell Claude the real path).';
    END
END

IF @IncludeSettlements = 1 AND OBJECT_ID(N'dbo.Settlements') IS NOT NULL AND COL_LENGTH(N'dbo.Settlements', N'CompanionId') IS NOT NULL
    INSERT @Roots VALUES (N'Settlements (payouts)', N'[dbo].[Settlements]', N'[CompanionId] = ' + @id);

-- ===========================================================================
-- Money snapshot BEFORE
-- ===========================================================================
IF @IncludeServiceReserves = 1
BEGIN
    DECLARE @q nvarchar(max) = N'SELECT N''BEFORE: service reserves'' AS Info, COUNT(*) AS Reserves, SUM([CompanionShare]) AS CompanionShare, SUM([SiteShare]) AS SiteShare, SUM([PaymentPrice]) AS PaymentPrice FROM [dbo].[CompanionReserves] WHERE '
        + (SELECT Pred FROM @Roots WHERE Label = N'Service reserves');
    EXEC (@q);
END

-- ===========================================================================
-- PASS 1: plan (changes nothing)
-- ===========================================================================
DECLARE @tbl nvarchar(300), @pred nvarchar(max);
UPDATE #Cfg SET ExecuteIt = 0;
TRUNCATE TABLE #Log;

DECLARE r CURSOR LOCAL FAST_FORWARD FOR SELECT Tbl, Pred FROM @Roots ORDER BY Seq;
OPEN r; FETCH NEXT FROM r INTO @tbl, @pred;
WHILE @@FETCH_STATUS = 0
BEGIN
    EXEC #Cascade @tbl, @pred, 0;
    FETCH NEXT FROM r INTO @tbl, @pred;
END
CLOSE r; DEALLOCATE r;

SELECT 'PLAN' AS Pass, Seq, Depth, Action, Tbl, Col, [Rows], Note FROM #Log ORDER BY Seq;
IF EXISTS (SELECT 1 FROM #Log WHERE Action = 'BLOCKED')
    THROW 50020, N'BLOCKED rows found (see PLAN). Nothing was changed. Decide how to handle them, then re-run.', 1;

-- ===========================================================================
-- PASS 2: execute inside a transaction (rolled back unless @Commit = 1)
-- ===========================================================================
BEGIN TRANSACTION;
    UPDATE #Cfg SET ExecuteIt = 1;
    TRUNCATE TABLE #Log;

    DECLARE r2 CURSOR LOCAL FAST_FORWARD FOR SELECT Tbl, Pred FROM @Roots ORDER BY Seq;
    OPEN r2; FETCH NEXT FROM r2 INTO @tbl, @pred;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        EXEC #Cascade @tbl, @pred, 0;
        FETCH NEXT FROM r2 INTO @tbl, @pred;
    END
    CLOSE r2; DEALLOCATE r2;

    SELECT 'EXECUTED' AS Pass, Seq, Depth, Action, Tbl, Col, [Rows], Note FROM #Log ORDER BY Seq;

    IF @IncludeServiceReserves = 1
    BEGIN
        DECLARE @q2 nvarchar(max) = N'SELECT N''AFTER: service reserves left'' AS Info, COUNT(*) AS Reserves, SUM([CompanionShare]) AS CompanionShare FROM [dbo].[CompanionReserves] WHERE '
            + (SELECT Pred FROM @Roots WHERE Label = N'Service reserves');
        EXEC (@q2);
    END

    IF @Commit = 1
    BEGIN
        COMMIT TRANSACTION;
        PRINT N'COMMITTED.';
    END
    ELSE
    BEGIN
        ROLLBACK TRANSACTION;
        PRINT N'ROLLED BACK (preview only). Set @Commit = 1 to apply.';
    END
GO
