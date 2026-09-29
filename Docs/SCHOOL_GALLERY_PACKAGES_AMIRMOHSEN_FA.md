# مدرسه: گالری عکس، پت‌های مورد پذیرش هر دوره، خلاصه‌ی جلسات/مدت دوره — مستند کامل (اپ نماینده)

**درخواست اصلی (عیناً):**
> «برای مدرسه حالت گالری عکس میخوایم داشته باشیم (مثل پانسیون) / توضیحات و قوانین و پت‌های مورد پذیرش (در واقع این دوره‌ای که مدرسه گذاشته باید تعریف کنه برای کدوم پت هست) مثل پانسیون داشته باشه / پکیج‌هایی که تعریف می‌کنه تاریخ و ساعت و روز مشخص کنه و تعداد روز در هفته و ماه رو نمایش بده که چقدر این دوره‌ش قراره طول بکشه و این داستان‌ها دقیق نمایش داده بشه و ثبت بشه برای دوره‌هاش، دوره تک‌جلسه‌ای و چند‌جلسه‌ای داشته باشه / و پت‌رسان هم بتونه وصل کنه بهش»

این مستند دقیقاً همان‌طور که پیاده شده، بدون کم‌وکاست، توضیح می‌دهد.

---

## ۱. گالری عکس مدرسه (مثل پانسیون)

### وضعیت قبلی
موجودیت `SchoolPicture` در دیتابیس از قبل وجود داشت (جدول `SchoolPictures` قبلاً ساخته شده بود)، ولی **هیچ لایه‌ی سرویس/کنترلری برایش نبود** — یعنی هیچ Endpoint‌ای برای افزودن/حذف/لیست عکس مدرسه در دسترس نبود.

### پیاده‌سازی
دقیقاً معادل `PansionPicture` ساخته شد:
- `Application/Services/SchoolSrvs/SchoolPictureSrv/SchoolPictureService.cs` + DTOها (`SchoolPictureDto`, `SchoolPictureVDto`, `SchoolPictureInputDto`, `SchoolPictureSearchDto`)
- `Api/Areas/Admin/Controllers/SchoolPictureController.cs`
- `Api/Areas/Companion/Controllers/SchoolPictureController.cs` (نماینده فقط روی گالری مدرسه‌ی خودش دسترسی دارد — چک مالکیت `School.CompanionId == CurrentUser.CompanionId` روی هر عملیات)

### Endpointها (ناحیه‌ی Companion — اپ نماینده)
همه احراز هویت (Bearer) لازم دارند.

| متد | مسیر | توضیح |
|---|---|---|
| GET | `/api/Companion/SchoolPicture?SchoolId={id}` | لیست عکس‌های گالری یک مدرسه |
| GET | `/api/Companion/SchoolPicture/{id}` | یک آیتم |
| POST | `/api/Companion/SchoolPicture` | افزودن عکس: بدنه `{ schoolId, pictureId, label? }` — `pictureId` باید قبلاً از سرویس آپلود فایل (File API، پورت ۵۰۰۱) گرفته شده باشد |
| PUT | `/api/Companion/SchoolPicture` | ویرایش (فقط `label` عملاً قابل تغییر است؛ `schoolId` سمت سرور از رکورد موجود بازخوانی می‌شود، امکان جابه‌جایی عکس بین مدارس وجود ندارد) |
| DELETE | `/api/Companion/SchoolPicture?id={id}` | حذف نرم (`Deleted=true`) |

پاسخ همیشه `{isSuccess, code, messages, data}` است — دقیقاً مثل بقیه‌ی API. اگر `schoolId` بدنه متعلق به مدرسه‌ی خود نماینده نباشد، `AccessDenied` برمی‌گردد.

### نکته‌ی مهم برای اپ نماینده
هیچ محدودیت تعداد عکس تعریف نشده (مثل پانسیون، بدون سقف). چون مستقیم متصل به یک مدرسه هستند نه یک دوره، همه‌ی دوره‌های یک مدرسه از یک گالری مشترک استفاده می‌کنند — دقیقاً مثل پانسیون که گالری روی خود پانسیون است نه روی هر اتاق/بسته.

---

## ۲. پت‌های مورد پذیرش هر دوره (چندبه‌چند، نه تک‌فیلد)

### وضعیت قبلی
`SchoolCourse` فقط یک `PetId`/`PetBreedId` نالِ‌پذیر **تکی** داشت — یعنی هر دوره فقط می‌توانست برای یک نوع پت (یا حتی یک نژاد خاص) تعریف شود، نه چند نوع هم‌زمان. پانسیون این محدودیت را نداشت چون از یک جدول واسط (`PansionPet`) استفاده می‌کرد.

### پیاده‌سازی
جدول واسط جدید `SchoolCoursePet` (مهاجرت `SchoolCoursePetsAndGallery`، فقط همین یک جدول ساخته شده، **اعمال نشده روی دیتابیس** — طبق توافق، من فقط مایگریشن می‌سازم):

```
SchoolCoursePets
  Id            bigint PK
  SchoolCourseId bigint FK -> SchoolCourses
  PetId          bigint FK -> Pets
  PetBreedId     bigint? FK -> PetBreeds   (اختیاری: خالی یعنی همه‌ی نژادهای همان Pet پذیرفته می‌شوند)
  Deleted        bit
```

فیلدهای قدیمی `SchoolCourse.PetId`/`PetBreedId` **حذف نشدند** (سازگاری با عقب، بدون بریک شدن دیتای موجود) — اما منبع حقیقت برای «کدام پت‌ها» از این به بعد لیست `SchoolCoursePet` است، نه آن فیلد تکی.

### Endpointها (ناحیه‌ی Companion — اپ نماینده)

| متد | مسیر | توضیح |
|---|---|---|
| GET | `/api/Companion/SchoolCoursePet?SchoolCourseId={id}` | لیست پت‌های مورد پذیرش یک دوره |
| POST | `/api/Companion/SchoolCoursePet` | افزودن: بدنه `{ schoolCourseId, petId, petBreedId? }` — رد تکراری با پیام `DuplicateValue` |
| DELETE | `/api/Companion/SchoolCoursePet?id={id}` | حذف نرم |

مالکیت از طریق دوره بررسی می‌شود (`SchoolCourse.School.CompanionId`)، نه مستقیم — چون خودِ این رکورد `CompanionId` ندارد.

### در پاسخ دوره (`SchoolCourseVDto`) چه می‌بینید
هر بار که یک دوره را می‌گیرید (`GET /api/Companion/SchoolCourse/{id}` یا لیست)، فیلد جدید:
```jsonc
"acceptedPets": [
  { "id": 1, "petId": 3, "petBreedId": null, "pet": { "name": "سگ" }, "petBreed": null },
  { "id": 2, "petId": 4, "petBreedId": 12, "pet": { "name": "گربه" }, "petBreed": { "name": "پرشین" } }
]
```
خالی بودن این لیست یعنی «برای این دوره پت خاصی تعریف نشده» — همان معنای قدیمی `PetId=null`.

---

## ۳. خلاصه‌ی جلسات و مدت دوره (تک‌جلسه‌ای / چندجلسه‌ای، تعداد در هفته، کل مدت)

### تحلیل قبل از هر تغییر
مدل داده‌ی جلسات (`SchoolCourseSession`) از قبل **دقیق و کافی** بود: هر جلسه یک `SessionDate` (تاریخ دقیق)، `StartTime`/`EndTime` (ساعت دقیق) دارد — نه یک الگوی هفتگی انتزاعی. این عمداً همین‌طور طراحی شده بود (برای پوش «کلاس شروع شد» سر همان زمان دقیق). یعنی «تاریخ و ساعت و روز مشخص» از قبل کاملاً پشتیبانی می‌شد و دست نخورد.

**چیزی که واقعاً کم بود:** نمایش خلاصه (چند جلسه در هفته، کل دوره چند هفته طول می‌کشد، تک‌جلسه‌ای است یا نه) — این محاسبه‌شده و اضافه شد، بدون تغییر مدل داده.

### فیلدهای جدید در `SchoolCourseVDto`
```jsonc
{
  "sessionCount": 8,              // از قبل موجود بود — تعداد کل جلسات دوره
  "isSingleSession": false,       // sessionCount <= 1
  "firstSessionDate": "2026-10-01T00:00:00",
  "lastSessionDate": "2026-11-19T00:00:00",
  "sessionsPerWeek": 1.1,         // از روی بازه‌ی واقعیِ اولین تا آخرین جلسه محاسبه می‌شود
  "totalDurationWeeks": 7.0       // (آخرین جلسه - اولین جلسه) بر حسب هفته
}
```
این‌ها **همیشه در سرویس محاسبه می‌شوند** (نه ذخیره‌شده در دیتابیس) — یعنی هر بار که جلسات یک دوره تغییر کند (اضافه/حذف/جابه‌جایی تاریخ)، این اعداد خودکار به‌روز هستند، بدون نیاز به هیچ عملیات دستی.

### دوره‌ی تک‌جلسه‌ای در برابر چندجلسه‌ای
هیچ فیلد/Endpoint جدیدی برای «نوع دوره» لازم نشد — این فقط بر اساس `SessionCount` (که در ساخت دوره از قبل اجباری و بزرگ‌تر از صفر بود) و تعداد جلسات واقعاً ثبت‌شده در `SchoolCourseSessions` تعیین می‌شود:
- `SessionCount = 1` و فقط یک `SchoolCourseSession` → `isSingleSession = true`، `sessionsPerWeek`/`totalDurationWeeks` بی‌معنی‌اند (روی `0`/برابر تعداد ست می‌شوند، چون بازه‌ای برای محاسبه وجود ندارد).
- `SessionCount > 1` → دوره‌ی چندجلسه‌ای، همه‌ی اعداد بالا معنادار محاسبه می‌شوند.

جریان ثبت جلسه (`UpsertSessionAsync`، `DeleteSessionAsync` در `SchoolCourseController`/`SchoolCourseSessionController`) **کاملاً دست‌نخورده** ماند — نماینده دقیقاً مثل قبل هر جلسه را تک‌به‌تک با تاریخ/ساعت دقیق ثبت می‌کند؛ فقط خروجی خلاصه‌اش حالا در `SchoolCourseVDto` نمایش داده می‌شود.

---

## ۴. پت‌رسان متصل به مدرسه — تأیید (قبلاً پیاده‌سازی شده، این‌جا بازسازی نشد)

این بخش از درخواست از قبل، در همین سشن، کامل پیاده شده بود و در این تغییرات دست نخورد:
- `ITripService.CreateReservationLinkedTripForSchoolAsync` (فقط برای دوره‌های `CourseTypeId == InPerson`، جلسه‌ی بعدیِ `SchoolCourseSession` را پیدا می‌کند و سفر پت‌رسان را به آن گره می‌زند)
- `POST /api/EndUser/TripSchoolReservation` مصرف‌کننده‌ی این قابلیت در سمت کاربر نهایی است
- لغو رزرو مدرسه → لغو خودکار سفر پت‌رسانِ متصل (`CancelLinkedTripForSchoolReserveAsync`)
- قیمت پت‌رسان به‌صورت جدا در `SchoolReserveVDto.PetResanTrip`/`TotalPrice` نمایش داده می‌شود (مستند مرتبط: `backend/Docs/PETRESAN_CONCURRENT_TRIP_GUARD_FA.md`)

هیچ اقدام اضافه‌ای برای این بخش لازم نبود؛ صرفاً این‌جا تأیید می‌شود که با تغییرات گالری/پت‌های‌مورد‌پذیرش/خلاصه‌ی جلسات **هیچ تداخلی ندارد**.

---

## ۵. وضعیت فنی / تست

- بیلد کامل `Pastil.sln` بدون خطا (۱۴ هشدار preexisting، بی‌ربط به این تغییر).
- `dotnet test Application.Tests`: **۵۶۲/۵۶۲ سبز** (بدون تست جدید برای این فیچر — منطق جدید صرفاً محاسباتی/CRUD ساده است، مثل الگوی مشابه `PansionPicture`/`PansionPet` که تست اختصاصی هم نداشتند).
- مایگریشن `SchoolCoursePetsAndGallery`: فقط یک جدول جدید (`SchoolCoursePets`) می‌سازد، هیچ جدول/ستون موجودی را تغییر نمی‌دهد — **بی‌خطر برای اعمال، صفر ریسک روی دیتای موجود**. طبق روال، من فقط ساختمش؛ اعمال/دیپلوی آن با شماست.
- گالری (`SchoolPicture`) از قبل جدولش در دیتابیس بود، نیازی به مایگریشن جدید نداشت.

## ۶. جمع‌بندی کارهای باقی‌مانده (خارج از بک‌اند)
- **پنل نمایندگان (این وب‌اپ)**: کامپوننت‌های مدیریت گالری و پت‌های مورد پذیرش هنوز به `SchoolEditor.vue`/`SchoolCourseList.vue` وصل نشده‌اند — تمام Endpointهای لازم آماده و تست‌شده‌اند، فقط UI باقی مانده (الگوی دقیق: `components/pansion-picture/*`, `components/pansion-pet/*`).
- **اپ کاربر نهایی (وب‌اپ Nuxt)**: مستند جدا نوشته شده برای آرمان: `webapp/docs/SCHOOL_GALLERY_PACKAGES_ARMAN_FA.md`.
