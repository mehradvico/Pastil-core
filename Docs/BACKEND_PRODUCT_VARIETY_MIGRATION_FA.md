# تنوع محصول توسط فروشنده + مخزن مشترک مقدارها — مستند تیم بک‌اند (Migration و بررسی‌ها)

> **وضعیت: طرح و بررسی — هنوز هیچ کدی برای این بخش‌ها نوشته یا عوض نشده است.**
> هدف این مستند این است که بک‌اند بداند دقیقاً چه چیزهایی باید عوض شود و چه چیزهایی را نباید بشکند.
> هر بخشی که تست واقعی نشده یا حدس است با **⚠️ تأییدنشده** علامت خورده.
> مواردی که هنوز تصمیم محصول برایشان نگرفته‌ایم در بخش ۹ آمده‌اند و تا تأیید، «پیشنهاد» حساب می‌شوند.

---

## ۱. هدف محصول (تصمیم‌های نهایی)

- **نوع تنوع** (`Variety`: رنگ، وزن، تعداد…) ثابت و مال ادمین است. فروشنده `Variety` جدید نمی‌سازد و فقط از موجودها انتخاب می‌کند.
- **مقدارها** (`VarietyItem`: قرمز، سبز…) یک **مخزن مشترک** برای هر `Variety` هستند. فروشگاه اول می‌سازد، فروشگاه‌های بعدی همان را می‌بینند و اگر مقدار مورد نیازشان نبود خودشان اضافه می‌کنند.
- برای هر مقدارِ انتخاب‌شده، فروشنده قیمت و موجودی جدا می‌گذارد (`ProductItem` به‌ازای هر فروشگاه × مقدار).
- پیش‌فرض همیشه «بدون تنوع» است (یک `ProductItem` با `VarietyItemId = null`). **هیچ جریان فعلی نباید بشکند:** سبد، سفارش، محاسبه‌ی قیمت، صفحه‌ی محصول مشتری، `AiProductMatch`، `MissingProduct`.
- حداکثر دو تنوع برای هر محصول (`Variety` و `Variety2`).
- مقدار جدید بلافاصله قابل استفاده است؛ ادمین بعداً بررسی می‌کند.
- تکراری‌یابی **فقط املایی** است: فاصله‌ی اضافه، نیم‌فاصله، ی/ک عربی و فارسی، ارقام فارسی/لاتین، حروف بزرگ/کوچک. مترادف‌ها (قرمز/سرخ) کار ادمین است.
- **هیچ سقفی روی تعداد مقدار جدید نمی‌گذاریم** (نه در هر درخواست، نه روزانه). تنها محدودیت، طول نام مقدار (پیشنهاد: ۵۰ کاراکتر) است.

## ۲. وضعیت فعلی کد (خوانده‌شده، تأییدشده با خواندن کد)

| موضوع | وضعیت فعلی | مسیر |
|---|---|---|
| مدل `VarietyItem` | فقط `Id, Name, Label, VarietyId, Deleted`. `Name` از نوع `nvarchar(max)` و بدون index (فقط index روی `VarietyId`) | `Entities/Entities/VarietyItem.cs`، `Persistence/Migrations/DataBaseContextModelSnapshot.cs` |
| تکراری‌نبودن نام | فقط در کد و **سراسری** (نه per-Variety)، بدون نرمال‌سازی، ردیف‌های حذف‌شده را هم می‌شمارد. در دیتابیس تضمینی نیست | `VarietyItemService.NameIsUnique` |
| ساخت فرم آیتم | حاصل‌ضرب **همه‌ی** مقدارهای غیرحذف‌شده‌ی Variety(ها) | `ProductItemService.GetInsertOrUpdateListAsync` |
| ثبت آیتم | `InsertOrUpdateAsync` → `InsertOrUpdate`. آیتم جدید فقط وقتی ساخته می‌شود که `BasePrice > 0 && Quantity > 0`. **هیچ اعتبارسنجی‌ای روی `VarietyItemId` ندارد** (فروشنده می‌تواند هر id دلخواه بفرستد) | `ProductItemService.cs` |
| ساخت محصول توسط فروشنده | `POST api/Seller/ProductAdmin` می‌تواند `VarietyId/Variety2Id` را در `ProductDto` بگیرد (`InsertAsyncDto`). ویرایش فقط برای محصول پیش‌نویس همان فروشگاه | `Api/Areas/Seller/Controllers/ProductAdminController.cs`، `ProductService.InsertAsyncDto/UpdateDtoAsync` |
| تغییر تنوع محصول | `ChangeProductVarietiesAsync` (ادمین) — جزئیات در بخش ۳ | `ProductService.cs` |
| سبد | `UpdateCartAsync` آیتم‌هایی را که `ProductItem.SystemActive == false` باشند بی‌صدا حذف می‌کند | `CartService.cs` |
| سفارش/تخفیف | `ProductOrderItem`، `CartItem`، `Discount` و `MissingProduct` فقط به `ProductItemId` وصل‌اند، نه مستقیم به `VarietyItem`. `VarietyItem` فقط برای نمایش نام Include می‌شود | `Entities/Entities/*.cs` |
| صفحه‌ی مشتری | مقدارها از روی `ProductItem`های فعال ساخته می‌شوند (نه از مخزن). پس مقدار جدیدِ بدون آیتم برای مشتری دیده نمی‌شود | `GetVariety2Async`، `GetVarietyAsync`، `GetForProductVDto` |
| قیمت محصول | `UpdateProductPriceAsync` → stored procedure `UpdateProductItemDiscount`. **⚠️ تأییدنشده:** سورس این SP در ریپو نیست و رفتارش دقیقاً معلوم نیست | `ProductService.cs` |

### شکنندگی‌هایی که باید به آن‌ها توجه شود
1. `GetVariety2Async`: اگر محصولی `Variety` داشته باشد و یک `ProductItem` فعال با `VarietyItemId = null` هم داشته باشد، `.First().Id` روی null خطا می‌دهد (⚠️ از روی خواندن کد؛ اجرا نشده). اگر `Variety2` هم داشته باشد، هر دو مقدار باید non-null باشند.
2. `AllMap.cs:755` — نگاشت `ProductDto → Product` فقط `VarietyId` را ignore می‌کند، **`Variety2Id` را نه**؛ پس `UpdateDtoAsync` می‌تواند `Variety2Id` را بدون حذف آیتم‌ها عوض کند (خارج از محدوده‌ی این کار ولی خطر است).
3. `DeleteDto` عمومی (`CommonSrv`) برای `VarietyItem` بدون بررسی استفاده حذف نرم می‌کند.

## ۳. `ChangeProductVarietiesAsync` دقیقاً چه می‌کند

1. اگر `Variety2` داده شده و `Variety` نه، یا هر دو یکی باشند → خطا.
2. محصول را با `FindAsync` می‌خواند.
3. **فقط اگر Variety یا Variety2 عوض شده باشد:** `Product.VarietyId/Variety2Id` را ذخیره می‌کند و بعد با `ExecuteUpdateAsync` **همه‌ی** `ProductItem`های آن محصول (همه‌ی فروشگاه‌ها) را `Deleted=true, Active=false, SystemActive=false` می‌کند. ردیف‌ها می‌مانند، پس سفارش‌ها و `Discount`ها سالم می‌مانند؛ سبدها را `UpdateCartAsync` بعداً پاک می‌کند.
4. اگر چیزی عوض نشده باشد → `false` با پیام «ناموفق» (idempotent نیست، پیام گمراه‌کننده است).

**ضعف‌ها:** تراکنش ندارد؛ بعدش `UpdateProductPriceAsync` صدا زده نمی‌شود؛ ادمین و فروشنده هر دو می‌توانند صدایش بزنند چون کنترلرش فقط `[Authorize]` دارد (بخش ۵-۱۱).

---

## ۴. Migration

### ۴-۱. تغییرات مدل `VarietyItem`

| ستون | نوع | توضیح |
|---|---|---|
| `NormalizedName` | `nvarchar(200) NULL` | خروجی `SearchNormalizeHelper.NormalizeNoSpace(Name)`. ⚠️ نمی‌شود `nvarchar(max)` بود چون روی آن index ممکن نیست |
| `CreatedByStoreId` | `bigint NULL` | ادمین/داده‌ی قدیمی = null. پیشنهاد: FK به `Stores` با `NoAction` |
| `CreateDate` | `datetime2 NULL` | ردیف‌های قدیمی null می‌مانند؛ ردیف‌های جدید مقدار می‌گیرند |

**Unique index** (پیشنهاد؛ سابقه‌ی index فیلترشده در این ریپو: مایگریشن `FixSlugUniqueIndexesExcludeDeleted`):

```csharp
e.HasIndex(x => new { x.VarietyId, x.NormalizedName })
 .IsUnique()
 .HasFilter("[Deleted] = 0 AND [NormalizedName] IS NOT NULL");
```

### ۴-۲. ⚠️ قبل از هر چیز: گزارش تکراری‌ها روی داده‌ی واقعی
LocalDB این ماشین داده‌ی واقعی ندارد، پس **وجود یا نبودن تکراری در داده‌ی واقعی هنوز نامعلوم است.**
اسکریپت زیر فقط SELECT است (به‌جز یک جدول موقت `#Norm`) و روی داده‌ی نمونه‌ی ساختگی تست شده. روی دیتابیس واقعی اجرا شود و خروجی به تیم برگردد.

```sql
/* گزارش تکراری‌های VarietyItem — فقط SELECT (به‌جز جدول موقت #Norm). */
SET NOCOUNT ON;

;WITH N AS (SELECT TOP (500) ROW_NUMBER() OVER (ORDER BY (SELECT 1)) AS n FROM sys.all_objects a CROSS JOIN sys.all_objects b),
Ch AS (
    SELECT v.Id, N.n AS pos, UNICODE(SUBSTRING(v.Name, N.n, 1)) AS cp
    FROM dbo.VarietyItems v JOIN N ON N.n <= LEN(v.Name)
),
Mapped AS (
    SELECT Id, pos,
        CASE
            WHEN cp BETWEEN 1776 AND 1785 THEN cp - 1776 + 48   -- ارقام فارسی
            WHEN cp BETWEEN 1632 AND 1641 THEN cp - 1632 + 48   -- ارقام عربی
            WHEN cp = 1610 THEN 1740                             -- ي -> ی
            WHEN cp = 1603 THEN 1705                             -- ك -> ک
            WHEN cp = 1577 THEN 1607                             -- ة -> ه
            WHEN cp = 1572 THEN 1608                             -- ؤ -> و
            WHEN cp IN (1573, 1571, 1649, 1570) THEN 1575        -- إ أ ٱ آ -> ا
            WHEN cp = 1574 THEN 1610                             -- ئ -> ي (رفتار واقعی C#)
            WHEN cp = 1728 THEN 1749                             -- ۀ -> ە (U+06D5؛ تأییدشده با C# واقعی)
            ELSE cp
        END AS cp
    FROM Ch
),
Kept AS (
    SELECT Id, pos, cp FROM Mapped
    WHERE cp BETWEEN 48 AND 57 OR cp BETWEEN 65 AND 90 OR cp BETWEEN 97 AND 122
       OR cp BETWEEN 1569 AND 1594 OR cp BETWEEN 1600 AND 1610 OR cp IN (1646, 1647, 1749, 1791)
       OR cp BETWEEN 1649 AND 1747 OR cp BETWEEN 1774 AND 1775 OR cp BETWEEN 1786 AND 1788
)
SELECT v.Id, v.VarietyId, v.Name, v.Deleted,
       LOWER(ISNULL((SELECT STRING_AGG(NCHAR(k.cp), '') WITHIN GROUP (ORDER BY k.pos) FROM Kept k WHERE k.Id = v.Id), N'')) AS NormName
INTO #Norm
FROM dbo.VarietyItems v;

PRINT N'--- 1) خلاصه';
SELECT COUNT(*) AS TotalRows, SUM(CASE WHEN Deleted = 0 THEN 1 ELSE 0 END) AS ActiveRows,
       SUM(CASE WHEN Deleted = 0 AND NormName = N'' THEN 1 ELSE 0 END) AS ActiveBlankNames,
       MAX(LEN(Name)) AS MaxNameLength
FROM #Norm;

PRINT N'--- 2) گروه‌های تکراری در هر Variety (فقط غیرحذف‌شده‌ها؛ همین‌ها ساخت unique index را می‌شکنند)';
SELECT n.VarietyId, n.NormName, COUNT(*) AS DupCount,
       STRING_AGG(CONCAT(n.Id, N':', n.Name, N' (used=', ISNULL(u.Cnt, 0), N')'), N' | ') AS Rows_IdNameUsage
FROM #Norm n
LEFT JOIN (SELECT VarietyItemId AS Id, COUNT(*) AS Cnt FROM (
        SELECT VarietyItemId FROM dbo.ProductItems WHERE VarietyItemId IS NOT NULL AND Deleted = 0
        UNION ALL SELECT VarietyItem2Id FROM dbo.ProductItems WHERE VarietyItem2Id IS NOT NULL AND Deleted = 0) x
    GROUP BY VarietyItemId) u ON u.Id = n.Id
WHERE n.Deleted = 0
GROUP BY n.VarietyId, n.NormName
HAVING COUNT(*) > 1
ORDER BY n.VarietyId, n.NormName;

PRINT N'--- 3) ردیف‌های غیرحذف‌شده با نام خالی/بدون حرف';
SELECT Id, VarietyId, Name FROM #Norm WHERE Deleted = 0 AND NormName = N'';

PRINT N'--- 4) نام‌های بلندتر از 50 کاراکتر';
SELECT Id, VarietyId, LEN(Name) AS Len, Name FROM #Norm WHERE Deleted = 0 AND LEN(Name) > 50;

PRINT N'--- 5) همان نام نرمال‌شده در چند Variety (فقط اطلاعاتی)';
SELECT NormName, COUNT(DISTINCT VarietyId) AS VarietyCount FROM #Norm WHERE Deleted = 0 AND NormName <> N'' GROUP BY NormName HAVING COUNT(DISTINCT VarietyId) > 1;

DROP TABLE #Norm;
```

**نتیجه‌ی تست روی داده‌ی ساختگی:** ردیف‌های زیر را درست تکراری تشخیص داد: `قرمز` / `قرمز ` / `قرم‌ز`، `مشکي` / `مشکی`، `۱۲ عدد` / `12 عدد` / `12‌عدد`، `Red` / `red`، `آبی` / `ابی`، `وزن (کیلو)` / `وزن کیلو`. ردیف حذف‌شده را نگرفت و نام خالی را جدا گزارش کرد.

### ۴-۳. نرمال‌سازی: C# در برابر T-SQL
- C#: `Application/Common/Helpers/SearchNormalizeHelper.cs` (`NormalizeNoSpace`). ذخیره‌ی ستون از C# انجام می‌شود، نه با `REPLACE` روی هر کوئری (collation فارسی `Persian_100_CI_AS` ي/ك عربی را با ی/ک فارسی و ارقام را برابر نمی‌گیرد).
- **رفتارهای غیربدیهیِ تأییدشده با اجرای واقعی helper:**
  - `آ` → `ا` (پس «آبی» = «ابی»)
  - `ئ` → `ي` عربی (U+064A)، نه `ی`
  - `ۀ` → `ە` (U+06D5)
  - `ـ` (کشیده) حذف **نمی‌شود**
  - `é` → `e` (تجزیه‌ی یونیکد لاتین)
- **backfill در T-SQL فقط تقریب است.** برای اطمینان از برابری، تست موازی لازم است: حروف بازه‌ی U+0000–U+06FF و U+200C و ارقام را هم با C# و هم با T-SQL نرمال کنید و نتیجه را مقایسه کنید. اختلاف روی حروف نادر خطرناک نیست (فقط ممکن است یک تکراریِ نادر جا بماند یا هم‌ارزی‌ای تشخیص داده نشود)، ولی باید مستند شود. ⚠️ تأییدنشده تا اجرای این تست.

### ۴-۴. سیاست تکراری‌های قدیمی (منتظر تصمیم؛ بخش ۹-۱)
- **گزینه B (پیشنهاد):** در هر گروه تکراری فقط کمترین `Id` مقدار `NormalizedName` می‌گیرد و باقی `NULL` می‌مانند. نام خالی هم `NULL`. با شرط index (`NormalizedName IS NOT NULL`)، Migration هیچ‌وقت شکست نمی‌خورد و داده‌ای ادغام یا حذف نمی‌شود. در پنل ادمین فیلتر «تکراری قدیمی» = `NormalizedName IS NULL AND Deleted = 0` نشان داده می‌شود.
- **گزینه A:** اگر تکراری وجود داشت، اسکریپت متوقف شود و ادمین دستی حل کند.
- هر دو گزینه: ستون‌ها، backfill و index در **یک Migration** ساخته شوند، به این ترتیب: `AddColumn` ← `migrationBuilder.Sql(backfill)` ← `CreateIndex`.

### ۴-۵. نکات فنی ساخت Migration
- ⚠️ `migrationBuilder.Sql` که به ستونِ تازه‌اضافه‌شده اشاره می‌کند باید داخل `EXEC(N'...')` باشد، وگرنه اسکریپت idempotent در همان batch خطای کامپایل می‌دهد (کوتیشن‌ها دو برابر شوند و حروف فارسی `N''` باشند).
- دستورها (طبق قواعد پروژه):
  ```bash
  dotnet tool restore
  dotnet ef migrations add AddVarietyItemSharedPool --project Persistence --startup-project Api
  dotnet ef migrations script <قبلی> AddVarietyItemSharedPool --project Persistence --startup-project Api --idempotent -o scripts/database/AddVarietyItemSharedPool.sql
  ```
  (`<قبلی>` آخرین Migration فعلی است: `AddMissingProduct` = `20260919084100_AddMissingProduct`.)
- **حتماً** محتوای Migration و diff فایل Snapshot را بخوانید؛ فقط باید همین سه ستون و index را داشته باشد (سابقه‌ی `AlignPendingModelSnapshot` نشان می‌دهد Snapshot گاهی چیزهای دیگر را هم می‌آورد).
- Migrationها روی SQL Server **دستی** اعمال می‌شوند؛ اسکریپت **قبل** از بالا آمدن API نسخه‌ی جدید اجرا شود.

---

## ۵. بررسی‌ها و تغییرات بک‌اند

هر مورد: **کجا / چه / چرا / ملاک پذیرش.**

### R1. قفل نوع تنوع (انتخاب Variety توسط فروشنده)
- **کجا:** سرویس جدید یا متد جدید در `ProductService` (پیشنهاد؛ بعد از تصمیم ۹-۳).
- **چه:** فروشنده فقط وقتی می‌تواند `Variety` را انتخاب کند که `Product.VarietyId` و `Variety2Id` هر دو null باشند و هیچ فروشگاه دیگری `ProductItem` **غیرحذف‌شده** (فعال یا غیرفعال؛ پیشنهاد سخت‌گیرانه‌تر از متن اولیه) برای آن محصول نداشته باشد.
  قالب اتمی پیشنهادی (تک‌دستور، داخل تراکنش):
  ```csharp
  var affected = await _context.Products
      .Where(p => p.Id == productId && p.VarietyId == null && p.Variety2Id == null
               && !_context.ProductItems.Any(i => i.ProductId == p.Id && !i.Deleted && i.StoreId != storeId))
      .ExecuteUpdateAsync(s => s.SetProperty(p => p.VarietyId, varietyId).SetProperty(p => p.Variety2Id, variety2Id));
  // affected == 0 ⇒ قفل است (یا محصول Variety دارد)
  ```
  در همان تراکنش: آیتم «بدون تنوع» **همین فروشگاه** حذف نرم می‌شود (`Deleted=true, Active=false, SystemActive=false`) و سپس `UpdateProductPriceAsync`.
- **چرا:** جلوگیری از race دو فروشگاه؛ آیتم فعال بدون تنوع روی محصول دارای Variety صفحه‌ی مشتری را می‌شکند.
- **⚠️ race باقی‌مانده:** ثبت هم‌زمان آیتم «بدون تنوع» توسط فروشگاه دیگر می‌تواند بعد از چک `NOT EXISTS` و قبل از commit وارد شود. پیشنهاد: `InsertOrUpdateAsync` و متد قفل هر دو در تراکنش با `UPDLOCK` روی ردیف `Products` (یا `sp_getapplock`) اجرا شوند. **تأییدنشده؛ باید با تست هم‌زمانی ثابت شود.**
- **پذیرش:** (الف) محصول بدون Variety و بدون آیتم دیگران → موفق؛ (ب) فروشگاه دیگر آیتم (حتی غیرفعال) دارد → رد با پیام روشن؛ (ج) محصول Variety دارد → رد؛ (د) دو درخواست هم‌زمان → دقیقاً یکی موفق.
- **قاعده:** اگر محصول از قبل `Variety` دارد فقط از همان Variety مقدار انتخاب/اضافه می‌شود.

### R2. Unique index — بخش ۴.
### R3. افزودن مقدار توسط فروشنده: idempotent و بدون خطای ۵۰۰
- **چه:** `NormalizedName = SearchNormalizeHelper.NormalizeNoSpace(name)`. اگر خالی/بلندتر از سقف → رد. اگر مقدار نرمال‌شده در همان `VarietyId` (غیرحذف‌شده) وجود دارد → رکورد جدید ساخته نشود و پاسخ ساختاریافته برگردد:
  ```json
  { "isSuccess": false, "code": 409, "messages": [["", "این مقدار قبلاً وجود دارد؛ آن را انتخاب کنید."]],
    "data": { "existingVarietyItemId": 15, "existingName": "قرمز" } }
  ```
  (⚠️ مقدار `code` و شکل دقیق `messages` باید با قرارداد `BaseResultDto` فعلی و کدهایی که اپ‌ها استفاده می‌کنند چک شود؛ ۴۰۹ فقط پیشنهاد است.)
- **race:** دو درخواست هم‌زمان → بازنده `DbUpdateException` (SqlException شماره‌ی 2601/2627) می‌گیرد. باید آن را گرفت، ChangeTracker را پاک کرد، ردیف برنده را با یک کوئری جدید خواند و **همان پاسخ «تکراری»** را داد.
- **پذیرش:** تست هم‌زمانی (۲۰ درخواست هم‌زمان برای یک نام → دقیقاً یک ردیف در DB، بقیه پاسخ «تکراری» با همان `existingVarietyItemId`، هیچ ۵۰۰).

### R4. ستون‌های سازنده
- هنگام ساخت توسط فروشنده: `CreatedByStoreId = StoreId از توکن (ICurrentUserHelper)` — هرگز از بدنه؛ `CreateDate = DateTime.Now`.
- ساخت توسط ادمین: `CreatedByStoreId = null`، `CreateDate` مقدار می‌گیرد.

### R5. آیتم «بدون تنوع» قدیمی فروشگاه
- در R1، همان تراکنش، فقط آیتم همین فروشگاه. سبد باز خودکار پاک می‌شود (رفتار موجود `UpdateCartAsync`)، سفارش‌های قبلی سالم می‌مانند (ردیف حذف نمی‌شود). قبل از تأیید در اپ به فروشنده هشدار داده شود که قیمت/موجودی قبلی «بدون تنوع» کنار می‌رود.

### R6. `AiProductMatch` (کار ۱ + قاعده ۶)
- **کجا:** `AiProductMatchService.BuildPackages` (خط ~691) و `LoadCandidateProductsAsync` (خط ~630).
- **الان:** بسته‌ها فقط از `ProductItem`های موجود ساخته می‌شوند، پس برای محصول بدون هیچ آیتم `matches[].productItems=[]` برمی‌گردد.
- **چه:** بسته‌ها از «تعریف محصول» ساخته شوند، با همان منطق `GetInsertOrUpdateListAsync`: بدون Variety → یک گزینه با `varietyItemId=null`؛ با تنوع → همه‌ی ترکیب‌های `VarietyItem` غیرحذف‌شده. `productItemId` و `existsForCurrentStore` از آیتم **همین فروشگاه**. شکل JSON (`AiProductMatchPackageDto`) **نباید عوض شود**.
- **بار DB:** `Include` فقط برای محصولات کاندید (نه کل کاتالوگ).
- **⚠️ رشد مخزن:** با مخزن مشترک، حاصل‌ضرب کامل می‌تواند بزرگ شود. پیشنهاد (منتظر تصمیم ۹-۵): حداکثر ۳۰ بسته به ترتیب «آیتم‌های خود فروشگاه ← مقدارهای استفاده‌شده توسط فروشگاه‌ها ← بقیه» و بقیه از endpoint مخزن.

### R7. اعتبارسنجی هنگام ثبت آیتم
- **کجا:** `ProductItemService.InsertOrUpdateAsync` (مسیر مشترک برای `Seller/ProductItem` و `MissingProduct`).
- **چه:** برای هر سطر:
  - محصول Variety ندارد ⇒ `VarietyItemId` و `VarietyItem2Id` هر دو null.
  - فقط Variety ⇒ `VarietyItemId` non-null و متعلق به همان Variety (غیرحذف‌شده)، `VarietyItem2Id` null.
  - هر دو ⇒ هر دو non-null و هرکدام متعلق به Variety خودش.
- **چرا:** الان سرور هیچ‌کدام را چک نمی‌کند؛ فروشنده‌ی فنی‌دان می‌تواند قفل R1 را دور بزند یا صفحه‌ی مشتری را بشکند.
- **پذیرش:** درخواست نامعتبر → `isSuccess:false` با پیام فارسی و **هیچ سطری ذخیره نشود** (کل درخواست رد شود). درخواست‌های معتبرِ فعلی (JSON فعلی) بدون تغییر کار کنند.

### R8. فرم قیمت‌گذاری فروشنده (شکل جدید، سازگار با قدیم)
- **کجا:** `GetInsertOrUpdateListAsync` و `Api/Areas/Seller/Controllers/ProductItemController.cs` (`GET api/Seller/ProductItem/id`). ادمین هم از همین متد استفاده می‌کند (`Api/Areas/Admin/Controllers/ProductItemController.cs`) — **رفتار ادمین نباید عوض شود**.
- **چه:** پارامترهای **اختیاری** `varietyItemIds` و `variety2ItemIds` (لیست جدا شده با کاما). اگر بیایند فقط سطرهای همان مقدارها + آیتم‌های موجود همین فروشگاه برگردند؛ اگر نیایند رفتار قدیمی (حاصل‌ضرب کامل).
- **چرا:** با مخزن مشترک، فرم قدیمی برای هر محصول به‌اندازه‌ی کل مخزن (مثلاً ۴۰ رنگ) سطر می‌شود.
- **⚠️ نکته:** `Variety` سراسری است، پس مخزن یک Variety بین همه‌ی محصولاتی که آن Variety را دارند مشترک است.

### R9. حذف مقدار توسط ادمین
- حذف نرم فقط اگر هیچ `ProductItem` (هر وضعیتی، شامل حذف‌شده‌ها که ممکن است هنوز به سفارش قدیمی وصل باشند) با `VarietyItemId` یا `VarietyItem2Id` = این مقدار نباشد، و هیچ `CartItem`/`ProductOrderItem` از طریق آن آیتم‌ها وابسته نباشد. در غیر این‌صورت پیام روشن با شمارش («۳ آیتم فروشگاه و ۱ سفارش از این مقدار استفاده می‌کنند»).
- ⚠️ بحث باز: آیا ردیف `ProductItem`ِ حذف‌شده‌ی بدون سفارش/سبد هم مانع حذف باشد؟ پیشنهاد: فقط آیتم‌های غیرحذف‌شده یا آیتم‌هایی که سفارش/سبد دارند مانع شوند.

### R10. Endpointهای ادمین (جدید)
همه با `[Authorize(Policy = PolicyNames.AdminOnly)]` (`Api/Program.cs:138`؛ نیازمند claim `RoleId` = Admin):

| متد و مسیر (پیشنهاد) | کار |
|---|---|
| `GET api/Admin/VarietyItemManage?varietyId=&q=&page=&onlyLegacyDuplicates=` | فهرست: سازنده (`CreatedByStoreId` + نام فروشگاه)، `CreateDate`، `productItemCount`، `productCount` |
| `PUT api/Admin/VarietyItemManage/rename` `{id, name}` | تغییر نام با همان بررسی تکراری و همان پاسخ ساختاریافته (R3) |
| `DELETE api/Admin/VarietyItemManage/{id}` | حذف نرم مطابق R9 |

### R11. امنیت مسیرهای قدیمی (⚠️ اصل ماجرا: بدون این‌ها قفل قابل دور زدن است)
- `Api/Areas/Admin/Controllers/VarietyItemController.cs` (کلاس `varietyItemController`) و `ProductChangeVarietyController.cs` فقط `[Authorize]` دارند. یعنی **هر کاربر لاگین‌شده** (مشتری هم) می‌تواند مقدارها را بسازد/تغییر نام بدهد/حذف کند، و از `PUT api/Admin/ProductChangeVariety` همه‌ی آیتم‌های هر محصول را پاک کند.
- پیشنهاد: `AdminOnly`. ریسک: اگر کاربر پنلی وجود دارد که `RoleId` ادمین ندارد، دسترسی‌اش قطع می‌شود؛ قبل از deploy چک شود (تصمیم ۹-۶).

### R12. `NameIsUnique` قدیمی
- `VarietyItemService.InsertAsyncDto/UpdateDto` باید از همان بررسی نرمال‌شده‌ی **per-Variety** (نه سراسری) استفاده کنند وگرنه مسیر قدیمی تکراری می‌سازد و به خطای index می‌خورد.
- تغییر رفتار: «قرمز» حالا در دو Variety مختلف مجاز است (الان مجاز نیست). ردیف حذف‌شده مانع نیست.
- توجه: `UpdateDto` قدیمی اگر `id` پیدا نشود NullReference می‌دهد.

### R13. `ChangeProductVarietiesAsync` (اختیاری، پیشنهاد)
- تراکنش بگذارید، بعد از حذف نرم `UpdateProductPriceAsync` صدا بزنید، و اگر تغییری نیست `isSuccess:true` با پیام «تغییری لازم نبود» برگردانید. قرارداد پاسخ فعلی را نشکنید.

### R14. مقاوم‌سازی `GetVariety2Async` (اختیاری)
- آیتم فعال با `VarietyItem == null` روی محصول دارای Variety را نادیده بگیرد تا NullReference ندهد (کمربند و بند در کنار R7).

---

## ۶. طرح API فروشنده (پیشنهاد — شکل نهایی بعد از تأیید بخش ۹)

همه `[Authorize]` و `StoreId` **فقط از `ICurrentUserHelper`**.

| متد و مسیر | کار |
|---|---|
| `GET api/Seller/ProductVariety?productId=` | وضعیت: `HasVariety` (ساختار + مقدارها) / `Selectable` (+ لیست Varietyهای قابل‌انتخاب) / `Locked` (فروشگاه دیگر آیتم دارد) |
| `PUT api/Seller/ProductVariety` `{productId, varietyId, variety2Id?}` | R1 + R5 |
| `GET api/Seller/VarietyItem?varietyId=&q=&page=` | مخزن مشترک با جست‌وجوی نرمال‌شده؛ `{id, name, label}` |
| `POST api/Seller/VarietyItem` `{varietyId, name}` | R3 |
| `GET api/Seller/ProductItem/id?id=&varietyItemIds=&variety2ItemIds=` | R8 |
| `POST api/Seller/ProductItem` | **JSON بدون تغییر**؛ فقط R7 |

Varietyهای قابل‌انتخاب (پیشنهاد؛ تصمیم ۹-۲): اجتماع Varietyهای متصل (`CategoryVariety`) به دسته‌های محصول؛ اگر خالی بود همه‌ی Varietyهای غیرحذف‌شده به ترتیب `Priority`.

## ۷. طرح تست (روی LocalDB، طبق روال `MissingProduct`)

پروژه‌ی console موقت در scratchpad (**نه** داخل ریپو) که `Application.csproj` و `Persistence.csproj` را reference کند؛ `EnsureCreated` روی دیتابیس موقت؛ seed؛ اجرای سرویس واقعی (سرویس‌های وابسته را با `DispatchProxy` fake کنید)؛ در پایان دیتابیس موقت حذف شود. AI/LLM در دسترس نیست؛ منطق دور از AI را تست کنید.

سناریوهای الزامی:
1. تکراری املایی (قرمز / `قرمز ` / نیم‌فاصله / ي‌ك عربی / ارقام / حروف) → پاسخ «تکراری» با id موجود.
2. **هم‌زمانی:** ۲۰ درخواست هم‌زمان برای یک نام در یک Variety → یک ردیف، بقیه «تکراری»، بدون ۵۰۰.
3. هم‌زمانی قفل: دو فروشگاه هم‌زمان، یکی Variety انتخاب کند و دیگری آیتم «بدون تنوع» ثبت کند → وضعیت نهایی سازگار (نه Variety + آیتم null).
4. R7: سطر با مقدار Variety دیگر / null روی محصول دارای Variety / مقدار حذف‌شده → رد بدون ذخیره.
5. R1: با/بدون آیتم فروشگاه دیگر، آیتم غیرفعال دیگران، محصول با Variety، آیتم null خود فروشگاه (حذف نرم شود، سبد پاک شود، سفارش قدیمی سالم بماند).
6. `AiProductMatch`: محصول بدون آیتم و بدون Variety → یک بسته‌ی null؛ با Variety → بسته‌ها؛ `existsForCurrentStore` فقط برای همین فروشگاه.
7. Migration: اجرای اسکریپت روی دیتابیس موقت با تکراری‌ها (سناریوی B: نباید شکست بخورد)، اجرای دوباره‌ی اسکریپت idempotent، و برابری نرمال‌سازی C#/T-SQL (بخش ۴-۳).
8. جریان‌های موجود: سبد، سفارش، صفحه‌ی مشتری (`GetVariety2Async`)، `MissingProduct` تأیید و ساخت آیتم.

`Application.Tests` به‌خاطر یک فایل قدیمی (`ShippingProviderTests.cs`) build نمی‌شود؛ اصلاحش جزو این کار نیست.

## ۸. Deploy

1. `dotnet build Api/Api.csproj` و تست‌های بخش ۷.
2. **قبل** از بالا آمدن API نسخه‌ی جدید، اسکریپت SQL idempotent روی SQL Server اجرا شود (ستون‌ها + backfill + index).
3. `dotnet publish` و rebuild ایمیج.
4. بعد از deploy: کوئری شمارش `NormalizedName IS NULL AND Deleted = 0` (تکراری‌های قدیمی برای ادمین).

## ۹. تصمیم‌های محصول که هنوز باز است (پیشنهاد؛ منتظر تأیید)

| # | سؤال | پیشنهاد |
|---|---|---|
| ۱ | تکراری‌های قدیمی | گزینه B (بخش ۴-۴) |
| ۲ | لیست Varietyهای قابل‌انتخاب | متصل به دسته‌ی محصول؛ اگر نبود همه |
| ۳ | شرط قفل | هر `ProductItem` غیرحذف‌شده‌ی فروشگاه دیگر، حتی غیرفعال |
| ۴ | بازگشت انتخاب اشتباه توسط فروشنده | راه برگشت نباشد؛ فقط ادمین |
| ۵ | اندازه‌ی بسته‌های `AiProductMatch` | حداکثر ۳۰ بسته + جست‌وجو |
| ۶ | `AdminOnly` روی دو کنترلر قدیمی | بله |
| ۷ | «آبی» = «ابی» | بله (رفتار helper فعلی) |
| ۸ | مخزن مشترک برای محصولی که فروشنده خودش می‌سازد (`ProductAdmin`) هم کار کند؟ | بله |

## ۱۰. ادغام دو مقدار (فاز ۲ — پیاده‌سازی نشود مگر با تأیید جدا)

- فقط `ProductItem.VarietyItemId/VarietyItem2Id` مستقیماً به `VarietyItem` وصل است؛ `CartItem`/`ProductOrderItem`/`Discount`/`MissingProduct` از طریق `ProductItem` همراه می‌شوند. (⚠️ سایر جدول‌هایی که ممکن است به `ProductItem` FK داشته باشند به‌طور کامل بررسی نشده‌اند.)
- **تداخل:** اگر یک فروشگاه برای یک محصول هر دو مقدار (مثلاً «قرمز» و «سرخ») را با قیمت متفاوت ثبت کرده باشد، بعد از ادغام دو آیتم با یک کلید می‌شود. باید یکی برنده شود (قیمت؟ موجودی؟) و `CartItem`/`ProductOrderItem`/`Discount` دیگری به برنده وصل شود یا بازنده حذف نرم بماند.
- **ریسک:** در حالت دو Variety کلید جفتی است؛ ادغام بین Varietyهای مختلف ممنوع؛ نام مقدار در سفارش‌های قدیمی عوض می‌شود؛ سبد باز، `SellLimitCount` و قیمت `Product` باید بازمحاسبه شود.

## ۱۱. قراردادهای که نباید شکسته شوند

- قراردادهای JSON موجود فقط افزودنی تغییر کنند.
- پاسخ‌ها `BaseResultDto` با `isSuccess/code/messages`، پیام‌ها فارسی، تقریباً همیشه HTTP 200.
- `DbContext` پیش‌فرض NoTracking است؛ برای ویرایش `AsTracking` بزنید. `Product` به `Status/Type/Brand/Picture` AutoInclude دارد (INNER JOIN روی `Codes`).
