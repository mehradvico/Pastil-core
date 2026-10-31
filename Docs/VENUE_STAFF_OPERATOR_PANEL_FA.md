# پنل اپراتور برای پرسنل کلینیک، آرایشگاه و مربی

تاریخ: ۲۰۲۶-۱۰-۳۱ · فقط بک‌اند پیاده شده · بدون migration و بدون تغییر دیتابیس

## مشکل

پرسنل کلینیک، آرایشگاه و مربی ثبت می‌شدند، دعوت را می‌پذیرفتند و مشتری می‌توانست آن‌ها را روی رزرو بگذارد (`assigneeUserId`). بک‌اند هم به پرسنل مسئول اجازه می‌داد رزرو را **تکمیل** کند، **تماس** بگیرد و **چت** کند.

اما هیچ endpointی رزروهای سپرده‌شده را به خود پرسنل **نشان نمی‌داد**. لیست و جزئیات رزرو فقط برای مالک بود. پنل `/operator` هم فقط رزروهای نمایندگی (Companion) را می‌شناخت. نتیجه: پرسنل ثبت می‌شد، ولی پنل اپراتور نداشت.

## چه اضافه شد

### ۱) آیا منوی «پنل اپراتور مراکز» را نشان بدهیم؟

`GET /api/Operator/Workplace`

```json
{ "isSuccess": true, "data": [
  { "kind": "clinic", "venueId": 12, "name": "کلینیک ...", "isOwner": false, "openAssignedCount": 3 }
]}
```

- `kind` یکی از `clinic`، `groomer` یا `coach` است.
- مرکزهایی برمی‌گردند که کاربر **مالک** یا **عضو پذیرفته‌شده و فعال** آن‌هاست.
- `openAssignedCount` تعداد رزروهای پرداخت‌شده، لغونشده و تکمیل‌نشده‌ای است که الان به همین کاربر سپرده شده (برای badge).
- **لیست خالی** یعنی منو را نشان ندهید.
- این پرچم عمداً در اطلاعات کاربر (`currentUser`) نیامده، چون آن کوئری روی هر درخواست اجرا می‌شود. یک بار بعد از ورود و هر بار که اپ به foreground برمی‌گردد صدا بزنید.

### ۲) پنل اپراتور: رزروهای من

برای هر نوع مرکز سه endpoint هست. `{Kind}` یکی از `ClinicReserve`، `GroomerReserve` یا `CoachReserve` است.

| متد | مسیر | کار |
|---|---|---|
| `GET` | `/api/Operator/{Kind}` | لیست رزروهای پرداخت‌شده‌ای که به من سپرده شده. همان query و همان خروجی (`{Kind}Page`) لیست مالک: `pageIndex`، `pageSize`، `status`، `isCancel`، `dateFrom`، `dateTo`، `q`، `sortBy` |
| `GET` | `/api/Operator/{Kind}/{id}` | جزئیات همان رزرو (اگر به من سپرده نشده باشد، `NothingFound`) |
| `PUT` | `/api/Operator/{Kind}/Complete?id=` | تکمیل رزرو. بدنه‌ی اختیاری و قواعدش عیناً مثل مالک است: `RESERVE_COMPLETE_FINAL_AMOUNT_NEMAYANDE_APP_FA.md` |

قواعد:
- فقط رزروهای **پرداخت‌شده** برمی‌گردند.
- اگر پرسنل از مرکز **حذف یا غیرفعال** شود، دیگر رزروهای قبلی‌اش را نمی‌بیند. مالک باید آن رزروها را به نفر دیگری بسپارد.
- سهم‌های حسابداری (`providerShare`، `siteShare`، `commissionPercent`) برای پرسنل **صفر** برمی‌گردند.
- تأیید، رد و تغییر زمان رزرو **فقط کار مالک است** و در پنل اپراتور نیامده.
- چت و تماس رزرو تغییری نکرده. پرسنل مسئول از قبل با همان endpointها دسترسی دارد.

### ۳) سپردن رزرو به پرسنل توسط مالک (بعد از پرداخت)

قبلاً پرسنل فقط موقع رزرو و توسط مشتری انتخاب می‌شد و مالک نمی‌توانست آن را عوض کند.

`PUT /api/Companion/{Kind}/Assign`

```json
{ "id": 345, "assigneeUserId": 78 }
```

- `assigneeUserId: null` پرسنل را از رزرو برمی‌دارد.
- فقط رزرو پرداخت‌شده، لغونشده و تکمیل‌نشده قابل سپردن است. خطا: «فقط رزرو پرداخت‌شده، فعال و تکمیل‌نشده را می‌توان به پرسنل سپرد.»
- پرسنل باید مالک یا عضو پذیرفته‌شده‌ی فعال باشد. اگر برای خدمت، افراد مشخصی تعریف شده باشند، باید یکی از همان‌ها باشد (همان قاعده‌ی زمان رزرو). خطا: `{Kind}ReserveAssigneeInvalid`.
- اگر آن پرسنل در همان بازه رزرو پرداخت‌شده‌ی دیگری داشته باشد، خطا می‌گیرد: «این پرسنل در همین زمان رزرو دیگری دارد.»
- پرسنل جدید پوش می‌گیرد: همان قالب نمایندگی `PushCompanionReserveAssigned` (id 45) با متن «خدمت {نام خدمت} برای کاربر {نام مشتری} به شما اختصاص داده شد.» و مسیر `/operator`. اگر مالک رزرو را به خودش بسپارد، پوشی نمی‌رود.
- برای پیدا کردن رزروهای بی‌پرسنل در لیست مالک: `GET /api/Companion/{Kind}?unassigned=true`.
- خروجی: جزئیات کامل رزرو (`{Kind}VDto`) با `assigneeUserId` و `assigneeName` جدید.

## کار اپ و وب‌اپ (کدنویسی نشده)

1. بعد از ورود `GET /api/Operator/Workplace` را صدا بزنید. اگر لیست خالی نبود، منوی «پنل اپراتور» را نشان بدهید. صفحه‌ی فعلی `/operator` نمایندگی را به چند tab بشکنید: نمایندگی، کلینیک، آرایشگاه، مربی. فقط tab مرکزهایی که در لیست هست دیده شود.
2. لیست و جزئیات هر tab از `/api/Operator/{Kind}` بیاید. دکمه‌ی «تکمیل» همان فرم مبلغ نهایی مالک است.
3. پوش `/operator` حالا ممکن است برای رزرو کلینیک، آرایشگاه یا مربی هم بیاید. شناسه‌ی رزرو در پوش نیست. صفحه را باز کنید و لیست همه‌ی tabها را تازه کنید.
4. در صفحه‌ی رزرو مالک، کنار نام پرسنل دکمه‌ی «سپردن به پرسنل / تغییر» بگذارید. انتخاب‌گر از لیست پرسنل مرکز (`GET /api/Companion/{Kind-User}`) پر شود. سپس `PUT /api/Companion/{Kind}/Assign` صدا زده شود. یک فیلتر «بدون پرسنل» هم (`unassigned=true`) اضافه کنید.
5. **وب‌اپ:** پنل‌های کلینیک و آرایشگاه allow-list مسیر سرور دارند (`webapp/app/server/utils/clinicRoutes.js`). مسیرهای `Operator/*` و `Companion/*/Assign` باید آن‌جا اضافه شوند.

## فایل‌های بک‌اند

- `Application/Services/{Clinic,Groomer,Coach}Srvs/*ReserveSrv/*ReserveService.cs`: `AssignAsync`، `FindForAssigneeAsync`، فیلترهای `AssigneeUserId` و `Unassigned` در `Search`
- `Application/Services/{Clinic,Groomer,Coach}Srvs/*Notifier.cs`: `ReserveAssignedAsync`
- `Application/Services/VenueOperatorSrv/VenueOperatorService.cs`: لیست محل کار و `VenueReserveAssignDto`
- `Api/Areas/Operator/Controllers/{Clinic,Groomer,Coach}ReserveController.cs` و `WorkplaceController.cs`
- `Api/Areas/Companion/Controllers/{Clinic,Groomer,Coach}ReserveController.cs`: `Assign`
- `Resource/Notification*`: `VenueReserveAssignNotAllowed`، `VenueReserveAssigneeBusy`
- تست: `Application.Tests/Clinic/ClinicReserveServiceTests.cs` → `Assign_ThenStaffSeesOnlyOwnReserves_UntilRemoved`
