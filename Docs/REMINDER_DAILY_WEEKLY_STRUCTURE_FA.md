# ساختار یادآور روزانه/هفتگی — آماده‌سازی بک‌اند

> تاریخ: ۱ مهر ۱۴۰۵ · فقط ساختار (schema + منطق زمان‌بندی + پوش)؛ چیزی روی دیتابیس اعمال نشده و پنل/وب‌اپ تغییر نکرده‌اند.

## چه چیزی از قبل بود
`Reminder` (یادآور واقعیِ یک پت) → `ReminderType` (موضوع، فعلاً فقط واکسن‌ها) + `ReminderCycle` (فاصله‌ی تکرار). مشکل: `ReminderCycle.Cycle` همیشه به معنای **ماه** بود (`ReminderScheduleCalculator` با `AddMonths` کار می‌کرد) و یادآوری در سه لحظه‌ی نسبت‌به-موعد اطلاع می‌داد: ۷ روز قبل، ۱ روز قبل، ۱ روز بعد — مناسب واکسن، نه برای «هر روز دارو بده».

## چه چیزی اضافه شد

### ۱) واحد چرخه
`Application/Common/Enumerable/ReminderCycleUnitEnum.cs`: `Day=1 / Week=2 / Month=3`.
`ReminderCycle` حالا `UnitId` دارد (پیش‌فرض DB = 3 = Month، پس چرخه‌های قدیمی دست‌نخورده می‌مانند). معنای `Cycle` می‌شود «تعداد این واحد» — مثلاً `Cycle=1, UnitId=1` = هر روز، `Cycle=2, UnitId=2` = هر دو هفته، `Cycle=3, UnitId=3` = هر سه ماه (رفتار قبلی، بدون تغییر).

### ۲) زمان‌بندی (`ReminderScheduleCalculator`)
- امضای قدیمی سه‌آرگومانه (`Resolve(startDate, cycleMonths, today)`) **دقیقاً همان رفتار قبلی** را دارد؛ هیچ تستی نشکسته.
- امضای جدید چهارآرگومانه `Resolve(startDate, cycleCount, unit, today)`:
  - `Month` → همان منطق قبلی (۷روز‌قبل/۱روز‌قبل/۱روز‌بعد).
  - `Week`/`Day` → قدم ثابت روز (`7×count` یا `1×count`)؛ فقط یک لحظه‌ی جدید `OnTheDay` **دقیقاً روز نوبت** (بدون یادآوری چند روز قبل/بعد — چون فاصله‌ی نوبت‌ها خودش کوتاه است).

### ۳) پوش/پیامک برای `OnTheDay`
- `PushTypeEnum.PushReminderToday = 75`.
- `MessageTypeEnum.UserReminderToday` (انتهای enum؛ این enum عمداً append-only نگه داشته شده).
- متن‌ها: `Resource.Pattern.fa.PushReminderToday` = «یادآوری! امروز موعد «{1}» برای پت «{0}» است.»، `Resource.Notification.fa.ReminderTodayText` = «امروز است.» (برای پیامک).
- `ReminderService.SyncReminderAsync`/`SendReminderAsync` این لحظه را مثل سه لحظه‌ی قبلی می‌فرستد.

### ۴) اعتبارسنجی و کاتالوگ (`ReminderCycleService`، Admin API)
- `UnitId` باید یکی از ۱/۲/۳ باشد (`InvalidData` وگرنه).
- **باگ رفع‌شده:** تکراری‌بودن قبلاً فقط با `Cycle` سنجیده می‌شد، یعنی «هر ۱ روز» با «هر ۱ ماه» یکی حساب می‌شد و دومی رد می‌شد. حالا تکراری‌بودن با `(Cycle, UnitId)` است.

### ۵) Migration (تولید شده، **اعمال نشده**)
`Persistence/Migrations/20260923071517_AddReminderCycleUnit.cs` + اسکریپت `backend/scripts/AddReminderCycleUnit.sql`:
1. ستون `UnitId int NOT NULL DEFAULT 3` + `CK_ReminderCycle_Unit CHECK ([UnitId] IN (1,2,3))`.
2. دو ردیف کاتالوگ جدید در `ReminderCycles`: «هر روز» (`Cycle=1, UnitId=1`) و «هر هفته» (`Cycle=1, UnitId=2`) — idempotent (`WHERE NOT EXISTS`).
3. seed رویداد پوش `PushReminderToday` (Id=75) در `PushTypes`/`PushPatterns`/`PushSettings`، دقیقاً هم‌الگوی seedهای قبلی این پروژه.

**قبل از استفاده باید این اسکریپت روی دیتابیس اجرا شود** — من به‌صورت پیش‌فرض چیزی روی DB اجرا نمی‌کنم؛ وقتی آماده بودید بگویید تا با هم بررسی و اجرا کنیم.

## API — چیزی که برای پنل/وب‌اپ عوض می‌شود
`GET/POST/PUT /api/Admin/ReminderCycle` و `ReminderCycleVDto` حالا `unitId` هم دارند. مثال ساخت چرخه‌ی روزانه توسط ادمین:
```json
POST /api/Admin/ReminderCycle
{ "name": "هر روز", "cycle": 1, "unitId": 1 }
```
`EndUser/Reminder` (ثبت یادآور برای یک پت) تغییری نکرده — فقط `reminderCycleId` می‌فرستد؛ همان id به یکی از چرخه‌های جدید (روزانه/هفتگی) هم می‌تواند اشاره کند.

## وضعیت فعلی UI (بررسی شد، تغییر داده نشد)
- **وب‌اپ** (`pages/reminder/index.vue`, `components/profile/MyReminders.vue`): چرخه را از یک `<select>` عمومی که از API پر می‌شود انتخاب می‌کند — هیچ فرضی درباره‌ی «ماه» در کد نیست. یعنی بعد از اجرای migration، «هر روز»/«هر هفته» **بدون هیچ تغییر کد در وب‌اپ** در همان select ظاهر می‌شوند.
- **پنل** (`panel/components/reminder/ReminderMetaCreateForm.vue`): فرم ساخت چرخه فعلاً هاردکد روی «تعداد ماه چرخه» است (بدون فیلد واحد) و درخواست را بدون `unitId` می‌فرستد (یعنی پیش‌فرض 3=Month می‌ماند). اگر می‌خواهید ادمین از پنل چرخه‌ی روزانه/هفتگیِ دلخواه (نه فقط دو مورد seed‌شده) بسازد، این فرم باید یک انتخاب‌گر واحد (روز/هفته/ماه) بگیرد و در `store.createReminderCycle` مقدار `unitId` را هم بفرستد — من این را نساختم چون گفتید خودتان می‌سازید؛ اگر خواستید همین را هم برایتان بسازم.

## چیزی که عمداً دست نخورد
- **`ReminderType`** (کاتالوگ موضوع یادآور) همچنان فقط واکسن‌هاست (طبق کامنت صریح در `SeedPetReminderTypes.sql`). اگر یادآور روزانه/هفتگی برای کارهای مراقبتی (دادن غذا، دادن دارو، مسواک، حمام، پیاده‌روی …) می‌خواهید، باید ردیف‌های جدید `ReminderType` اضافه شود — این یک تصمیم محتوایی/محصولی است، نه بخشی از «ساختار»، و دست نزدم.
- منطق `SendReminderAsync` برای لحظه‌ی `OneDayAfter` از قبل به `MessageTypeEnum.UserReminderTomorrow` نگاشت شده بود (اسم گمراه‌کننده، ولی preexisting و خارج از این تغییر) — تغییرش ندادم چون بخشی از درخواست فعلی نبود.

## تست
`Application.Tests/Reminder/*` — ۴۹۵ تست سبز، شامل:
- `ReminderScheduleCalculatorTests`: چرخه‌ی روزانه/هفتگی/ماهانه، مرز شروع، عدم شلیک قبل از تاریخ شروع.
- `ReminderModelTests`: `CK_ReminderCycle_Unit`، مقدار پیش‌فرض `UnitId`.
- `ReminderPushConfigurationTests`: شناسه‌ی یکتای `PushReminderToday`، متن فارسی، append-only بودن `MessageTypeEnum`.

## قدم بعدی پیشنهادی
1. تصمیم بگیرید کدام `ReminderType`های جدید برای روزانه/هفتگی لازم است (اگر غیر از واکسن می‌خواهید).
2. اسکریپت `scripts/AddReminderCycleUnit.sql` را روی دیتابیس اجرا کنید (با تأیید شما).
3. Api را deploy کنید.
4. اگر پنل باید بتواند چرخه‌ی دلخواه (نه فقط دو مورد seed‌شده) بسازد، فرم `ReminderMetaCreateForm.vue` را به‌روز کنید (یا از من بخواهید).
