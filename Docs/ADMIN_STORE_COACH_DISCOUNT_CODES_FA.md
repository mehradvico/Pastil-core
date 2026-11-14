# تخفیف پت‌شاپ و کد تخفیف اختصاصی (پت‌شاپ / مربی / سایر ارائه‌دهنده‌ها) — مستند پنل ادمین

> **مخاطب:** تیم فرانت پنل ادمین (و QA).
> **وضعیت بک:** پیاده و تست شد (`Application.Tests/RebateTargetTests.cs`)، **دیپلوی نشده**.
> **پیش‌نیاز دیتابیس:** migration `20261114120000_RebateTarget` (دو ستون `TargetType` و `TargetId` روی جدول `Rebate`) یا بخش ۰ اسکریپت `scripts/store-coach-discount-codes.sql`.

---

## ۱) خلاصه

دو قابلیت جدا داریم که در پنل ادمین **دو صفحه‌ی جدا** هستند:

| قابلیت | چه می‌کند | کاربر کاری می‌کند؟ | API | جدول |
| --- | --- | --- | --- | --- |
| **الف) تخفیف پت‌شاپ** | قیمت **همه‌ی محصولات** یک پت‌شاپ را X٪ کم می‌کند (روی سایت با قیمت خط‌خورده دیده می‌شود) | خیر، خودکار است | `/api/Admin/Discount` | `Discounts` |
| **ب) کد تخفیف اختصاصی** | کدی می‌سازد که کاربر در سبد/رزرو وارد می‌کند و **فقط** برای همان پت‌شاپ یا همان مربی (یا کلینیک، آرایشگاه، پانسیون، همراه) قبول می‌شود | بله، کد را وارد می‌کند | `/api/Admin/Rebate` | `Rebate` |

اگر کد تخفیف به جایی محدود نشود (`targetType = null`)، مثل قبل برای همه کار می‌کند. یعنی **کدهای قبلی هیچ تغییری نمی‌کنند**.

---

## ۲) جای صفحه‌ها در منوی پنل ادمین

```
فروشگاه
 └─ تخفیف‌ها                      ← صفحه‌ی موجود (الف)
     └─ «تخفیف کل فروشگاه»         ← نوع تخفیف = DiscountType_Store

مالی / بازاریابی
 └─ کدهای تخفیف                    ← صفحه‌ی موجود (ب)
     └─ فرم «کد جدید»: بخش جدید «محدود به» اضافه شود
```

| صفحه | دسترسی ادمین (AdminPermissionCatalog) |
| --- | --- |
| تخفیف‌ها | `Discount` و `DiscountGroup` |
| کدهای تخفیف | `Rebate` |

همه‌ی endpointها `Authorize` دارند و توکن ادمین لازم است.

---

## ۳) قالب پاسخ (برای همه‌ی endpointها)

```json
{ "isSuccess": true, "messages": [{ "item1": "عملیات با موفقیت انجام شد", "item2": "" }], "data": { }, "code": 0 }
```

- خطاها **HTTP 200 با `isSuccess: false`** برمی‌گردند. موفقیت یعنی HTTP موفق **و** `isSuccess === true`.
- `messages[i].item1` متن خطاست که مستقیم نمایش داده می‌شود. `item2` نام فیلد خطادار است (مثلاً `TargetId`) و فرانت با آن زیر فیلد مربوط خطا را نشان می‌دهد.
- enumها در JSON **عدد** هستند، نه رشته.

---

## ۴) الف) تخفیف روی همه‌ی محصولات یک پت‌شاپ

### ۴-۱) فرم

| فیلد UI | فیلد API | نوع | الزامی | توضیح |
| --- | --- | --- | --- | --- |
| پت‌شاپ | `storeId` | number | ✅ | از جستجوی فروشگاه (بخش ۶) |
| نوع تخفیف | `typeId` | number | ✅ | برای کل فروشگاه **`95`** (`DiscountType_Store`). بقیه: 96 دسته، 97 برند، 98 محصول، 99 تنوع محصول |
| گروه تخفیف | `discountGroupId` | number | ✅ | dropdown از `GET /api/Admin/DiscountGroup?pageSize=100` |
| درصد | `percent` | int | ✅ | ۱ تا ۹۹ |
| تاریخ پایان | `endDate` | date \| null | ❌ | خالی = بدون انقضا. نباید قبل از امروز باشد |
| هزینه با پاستیل | `fundedByPastil` | bool | ❌ | `true` یعنی هزینه را پاستیل می‌دهد. سقف درصد = کمیسیون همان فروشگاه. جزئیات در `Docs/PASTIL_FUNDED_DISCOUNT_FA.md` |
| فعال | `active` | bool | ✅ | معمولاً `true` |

وقتی `typeId = 95` است، فیلدهای `categoryId`، `brandId`، `productId` و `productItemId` را نفرستید یا `null` بفرستید.

### ۴-۲) ثبت

```http
POST /api/Admin/Discount
```
```json
{
  "storeId": 12,
  "typeId": 95,
  "discountGroupId": 1,
  "percent": 10,
  "endDate": "2026-12-31",
  "fundedByPastil": false,
  "active": true
}
```

بعد از ثبت، بک **خودش** قیمت همه‌ی محصولات آن فروشگاه را دوباره حساب می‌کند. `Store.MaxDiscountPercent` هم به‌روز می‌شود (همان برچسب «تا X٪ تخفیف» روی کارت فروشگاه).

> اگر یک محصول چند تخفیف داشته باشد (مثلاً فروشگاه ۱۰٪ و محصول ۲۰٪)، **بیشترین** تخفیفِ خود فروشگاه اعمال می‌شود، نه جمع آن‌ها.

### ۴-۳) لیست، فعال/غیرفعال، حذف

| کار | درخواست |
| --- | --- |
| لیست تخفیف‌های یک فروشگاه | `GET /api/Admin/Discount?storeId=12&discountType=DiscountType_Store&available=true&pageIndex=1&pageSize=20` |
| جزئیات | `GET /api/Admin/Discount/{id}` |
| فعال/غیرفعال | `PUT /api/Admin/Discount` با بدنه‌ی `{ "id": 5, "storeId": 12, "active": false }` (**فقط** وضعیت فعال بودن را عوض می‌کند و ویرایش درصد ندارد) |
| حذف | `DELETE /api/Admin/Discount` با **بدنه‌ی** `{ "id": 5, "storeId": 12 }` |

برای عوض کردن درصد: تخفیف قبلی را حذف یا غیرفعال کنید و یک تخفیف جدید بسازید. بعد از هر تغییر، قیمت‌ها خودکار دوباره حساب می‌شوند.

### ۴-۴) خطاها

| پیام | علت |
| --- | --- |
| بازه درصد صحیح نیست | `percent` خارج از ۱ تا ۹۹ است، یا با `fundedByPastil` از کمیسیون فروشگاه بیشتر است |
| تاریخ صحیح نیست | `endDate` قبل از امروز است |
| `DISCOUNT_TARGET_NOT_OWNED_BY_STORE` | دسته، برند یا محصول انتخاب‌شده در این فروشگاه کالا ندارد (برای نوع 95 رخ نمی‌دهد) |

---

## ۵) ب) کد تخفیف اختصاصی

### ۵-۱) فرم «کد تخفیف جدید»

| فیلد UI | فیلد API | نوع | الزامی | قانون |
| --- | --- | --- | --- | --- |
| عنوان | `name` | string | ✅ | فقط برای ادمین نمایش داده می‌شود |
| کد | `codeValue` | string | ✅ | بک فاصله‌ها را حذف و حروف را کوچک می‌کند. بین کدهای حذف‌نشده باید **یکتا** باشد |
| نوع کد | `typeId` | number | ✅ | از جدول ۵-۲. **این فیلد تعیین می‌کند کد کجا قابل استفاده است** |
| نوع تخفیف | `isPriceRebate` | bool | ✅ | `false` = درصدی، `true` = مبلغ ثابت (تومان) |
| مقدار | `priceValue` | number | ✅ | درصدی: ۱ تا ۹۹. مبلغی: ≥ ۱ |
| حداقل مبلغ سبد | `minCartPrice` | number | مبلغی ✅ | فقط برای کد مبلغی. باید ≥ `priceValue + 1000` باشد |
| از تاریخ | `startDatetime` | date | ✅ | ≥ امروز |
| تا تاریخ | `endDatetime` | date | ✅ | ≥ از تاریخ |
| تعداد کل استفاده | `useCount` | int | ✅ | ≥ ۱ |
| حداکثر استفاده هر کاربر | `maxUsePerUser` | int | ✅ | بین ۱ و `useCount` |
| فقط برای یک کاربر | `userId` | number \| null | ❌ | خالی = همه‌ی کاربران |
| فقط برای یک محصول | `productId` | number \| null | ❌ | فقط برای نوع سبد خرید. تخفیف فقط روی مبلغ همان محصول حساب می‌شود |
| فعال | `active` | bool | ✅ | |
| **🆕 محدود به (نوع)** | `targetType` | int \| null | ❌ | جدول ۵-۳. خالی = بدون محدودیت |
| **🆕 محدود به (کدام)** | `targetId` | number \| null | اگر `targetType` پر باشد ✅ | شناسه‌ی پت‌شاپ یا مربی یا ... |

### ۵-۲) نوع کد (`typeId`)

dropdown از `GET /api/Admin/Code?codeGroupLabel=RebateType&available=true&pageSize=100`:

| id | Label | کجا استفاده می‌شود |
| ---: | --- | --- |
| 62 | `RebateType_Cart` | سبد خرید فروشگاه (**پت‌شاپ**) |
| 63 | `RebateType_CompanionReserve` | رزرو همراه |
| 64 | `RebateType_Cargo` | ارسال بار |
| 65 | `RebateType_Trip` | سفر |
| 66 | `RebateType_InsurancePackageSale` | بسته بیمه |
| 67 | `RebateType_PansionReserve` | رزرو پانسیون |
| 68 | `RebateType_PastilAI` | اشتراک پاستیل AI |
| 69 | `RebateType_SchoolReserve` | دوره مدرسه |
| 70 | `RebateType_ConsultationPurchase` | خرید مشاوره |
| 71 | `RebateType_GroomerReserve` | رزرو آرایشگاه |
| 72 | `RebateType_ClinicReserve` | رزرو کلینیک |
| 73 | `RebateType_CoachReserve` | رزرو **مربی** |

### ۵-۳) 🆕 «محدود به» (`targetType`) و ارتباطش با نوع کد

`targetType` فقط با نوع کد مناسب خودش قبول می‌شود. **فرانت باید dropdown «محدود به» را بر اساس نوع کد انتخاب‌شده فیلتر کند:**

| نوع کد (`typeId`) | گزینه‌های مجاز «محدود به» | `targetType` | `targetId` = | picker |
| --- | --- | ---: | --- | --- |
| 62 سبد خرید | **پت‌شاپ** | `2` | `Stores.Id` | جستجوی فروشگاه |
| | محصول | `3` | `Products.Id` | جستجوی محصول |
| | دسته محصول | `4` | `Categories.Id` | درخت دسته |
| | شهر (آدرس سبد) | `11` | `Cities.Id` | شهر |
| 73 رزرو مربی | **مربی** | `15` | `Coaches.Id` | جستجوی مربی |
| | خدمت (Assistance) | `6` | `Assistances.Id` | لیست خدمات |
| 72 رزرو کلینیک | کلینیک | `14` | `Clinics.Id` | جستجوی کلینیک |
| 71 رزرو آرایشگاه | آرایشگاه | `13` | `Groomers.Id` | جستجوی آرایشگاه |
| 67 رزرو پانسیون | پانسیون | `8` | `Pansions.Id` | جستجوی پانسیون |
| 63 رزرو همراه | همراه | `5` | `Companions.Id` | جستجوی همراه |
| 68 پاستیل AI | پلن | `10` | شناسه‌ی پلن | لیست پلن‌ها (`/api/Admin/PastilAI`) |
| همه | بدون محدودیت | `null` (یا `1` = Global) | `null` | — |

> برای ارسال بار، سفر، بیمه، مدرسه و مشاوره هنوز محدودیت ارائه‌دهنده **پشتیبانی نمی‌شود**. اگر برای این نوع‌ها `targetType` بفرستید، بک خطا می‌دهد. در این نوع‌ها بخش «محدود به» را مخفی کنید.

**رفتار UI پیشنهادی:**
1. کاربر «نوع کد» را انتخاب می‌کند ← dropdown «محدود به» با گزینه‌های همان ردیف جدول پر می‌شود. گزینه‌ی اول همیشه «بدون محدودیت» است.
2. اگر گزینه‌ای غیر از «بدون محدودیت» انتخاب شد ← picker مربوط (autocomplete) نمایش داده می‌شود و الزامی است.
3. اگر «نوع کد» عوض شد ← `targetType` و `targetId` پاک شوند.

### ۵-۴) نمونه‌ی ثبت: کد اختصاصی یک پت‌شاپ

```http
POST /api/Admin/Rebate
```
```json
{
  "name": "کد اختصاصی پت‌شاپ آریا",
  "codeValue": "ARIA15",
  "typeId": 62,
  "isPriceRebate": false,
  "priceValue": 15,
  "minCartPrice": 0,
  "startDatetime": "2026-11-15",
  "endDatetime": "2026-12-31",
  "useCount": 1000,
  "maxUsePerUser": 1,
  "active": true,
  "targetType": 2,
  "targetId": 12
}
```

### ۵-۵) نمونه‌ی ثبت: کد اختصاصی یک مربی

```json
{
  "name": "کد مربی رضایی",
  "codeValue": "coach-rezaei",
  "typeId": 73,
  "isPriceRebate": true,
  "priceValue": 200000,
  "minCartPrice": 500000,
  "startDatetime": "2026-11-15",
  "endDatetime": "2027-01-31",
  "useCount": 200,
  "maxUsePerUser": 2,
  "active": true,
  "targetType": 15,
  "targetId": 7
}
```

> ذخیره می‌شود به شکل `coach-rezaei` (کوچک و بدون فاصله).

### ۵-۶) لیست، جزئیات، حذف

| کار | درخواست |
| --- | --- |
| لیست و جستجو (روی عنوان یا کد) | `GET /api/Admin/Rebate?q=aria&pageIndex=1&pageSize=20` |
| جزئیات | `GET /api/Admin/Rebate/{id}` |
| حذف | `DELETE /api/Admin/Rebate?id={id}` |
| ویرایش | **endpoint ندارد.** کد را حذف کنید و دوباره بسازید (مثل قبل) |

هر ردیف لیست، `targetType` و `targetId` را هم دارد. **ستون پیشنهادی «محدود به»** در جدول:
- `null` ← «همه»
- `2` ← «پت‌شاپ: {نام}»: نام را با `GET /api/Admin/Store/{targetId}` بگیرید و در حافظه‌ی صفحه cache کنید
- `15` ← «مربی: {نام}»: با `GET /api/Admin/Coach/{targetId}`
- بقیه‌ی گزینه‌ها به همین شکل

ستون «استفاده‌شده» هم = `usedCount / useCount`.

### ۵-۷) خطاهای ثبت کد

| پیام (`item1`) | `item2` | علت |
| --- | --- | --- |
| لطفا نام را وارد نمایید | `Name` | عنوان خالی است |
| عدد وارد شده صحیح نیست | `PriceValue` / `MaxUsePerUser` | مقدار < ۱، یا `useCount`/`maxUsePerUser` نامعتبر |
| کد وارد شده تکراری است | `CodeValue` | کد تکراری است |
| بازه درصد صحیح نیست | `PriceValue` | کد درصدی با مقدار بیشتر از ۹۹ |
| حداقل مبلغ سفارش باید … باشد | `MinCartPrice` | کد مبلغی و `minCartPrice < priceValue + 1000` |
| تاریخ صحیح نیست | `StartDatetime` | تاریخ گذشته است یا شروع بعد از پایان است |
| **🆕 دیتا اشتباه است** | **`TargetId`** | `targetType` پر است ولی `targetId` خالی است، **یا** `targetType` با نوع کد سازگار نیست (جدول ۵-۳) |

> بک وجود فروشگاه یا مربیِ `targetId` را چک **نمی‌کند**. چون picker فقط شناسه‌های واقعی را می‌دهد، مقدار را دستی وارد نکنید.

---

## ۶) pickerها (autocomplete)

| picker | درخواست | نمایش |
| --- | --- | --- |
| فروشگاه | `GET /api/Admin/Store?q={متن}&approved=true&pageIndex=1&pageSize=20` | `name` (+ شهر) |
| مربی | `GET /api/Admin/Coach?q={متن}&approve=true&pageIndex=1&pageSize=20` | `name` |
| کلینیک / آرایشگاه / پانسیون / همراه | `GET /api/Admin/Clinic` / `Groomer` / `Pansion` / `Companion` با `q` | `name` |
| گروه تخفیف | `GET /api/Admin/DiscountGroup?pageSize=100` | `name` |
| نوع کد | `GET /api/Admin/Code?codeGroupLabel=RebateType&available=true&pageSize=100` | `name` |

مقداری که به‌عنوان `targetId` / `storeId` فرستاده می‌شود همان `id` آیتم انتخاب‌شده است.

---

## ۷) سمت کاربر (برای آگاهی ادمین و QA؛ وب‌اپ تغییری لازم ندارد)

| جریان | درخواست | قانون جدید |
| --- | --- | --- |
| سبد خرید | `POST /api/EndUser/Cart` با `{ "cartUpdateType": 7, "rebateCode": "aria15" }` (`7` = `SetRebate`) | کد پت‌شاپ فقط وقتی قبول می‌شود که **فروشگاه فعال سبد** همان پت‌شاپ باشد |
| تغییر فروشگاه فعال سبد | `cartUpdateType: 21` (`SetActiveCart`) | 🆕 اگر فروشگاه فعال عوض شود، **کد تخفیف از سبد برداشته می‌شود** و کاربر باید دوباره کد بزند |
| رزرو مربی | `PUT /api/EndUser/CoachReservePayment/Rebate` با `{ "id": رزرو, "rebateCode": "coach-rezaei" }` | کد مربی فقط برای رزرو همان مربی قبول می‌شود |
| کلینیک / آرایشگاه / پانسیون / همراه | endpointهای `...Rebate` همان سرویس | همان قانون |

پیام به کاربر وقتی کد مال جای دیگری است: **«امکان استفاده از این کد تخفیف نیست»**.

بقیه‌ی چک‌ها مثل قبل است: فعال بودن، بازه‌ی تاریخ، سقف استفاده‌ی کل و هر کاربر، حداقل مبلغ.

**مبنای محاسبه‌ی کد درصدی در سبد:** مبلغ سبد فروشگاه فعال، **بعد از** تخفیف پت‌شاپ (الف). یعنی تخفیف الف و کد ب روی هم اعمال می‌شوند: اول قیمت محصول با تخفیف پت‌شاپ کم می‌شود، بعد کد از جمع سبد کم می‌شود.

**هزینه‌ی کد تخفیف (ب) با پاستیل است.** کمیسیون فروشگاه روی مبلغ قبل از کد حساب می‌شود. تخفیف (الف) با فروشگاه است، مگر `fundedByPastil = true` باشد.

---

## ۸) سناریوهای تست (QA)

| # | سناریو | نتیجه‌ی مورد انتظار |
| --- | --- | --- |
| 1 | تخفیف ۱۰٪ نوع 95 برای فروشگاه A | قیمت همه‌ی کالاهای A در سایت ۱۰٪ کم می‌شود و برچسب «تا ۱۰٪» روی فروشگاه دیده می‌شود |
| 2 | غیرفعال کردن همان تخفیف | قیمت‌ها برمی‌گردند |
| 3 | کد `typeId=62, targetType=2, targetId=A` در سبد A | قبول می‌شود |
| 4 | همان کد در سبد B | «امکان استفاده از این کد تخفیف نیست» |
| 5 | کد را در سبد A بزن، بعد فروشگاه فعال را B کن | کد از سبد برداشته می‌شود |
| 6 | کد `typeId=73, targetType=15, targetId=X` برای رزرو مربی X | قبول می‌شود |
| 7 | همان کد برای رزرو مربی Y | رد می‌شود |
| 8 | ثبت `typeId=73` با `targetType=2` | خطای «دیتا اشتباه است» روی `TargetId` |
| 9 | ثبت `targetType=2` بدون `targetId` | همان خطا |
| 10 | کد بدون `targetType` | مثل قبل برای همه کار می‌کند |
| 11 | `maxUsePerUser=1` و استفاده‌ی دوم همان کاربر | «محدودیت تعداد استفاده از این کد تخفیف به اتمام رسیده است» |

---

## ۹) فایل‌های بک (برای مرجع)

| فایل | تغییر |
| --- | --- |
| `Entities/Entities/Rebate.cs` | `TargetType`، `TargetId` |
| `Application/Services/Order/RebateSrv/Dto/RebateDto.cs`، `RebateVDto.cs` | همان دو فیلد |
| `Application/Services/Order/RebateSrv/RebateService.cs` | `TargetsOf` (تارگت خود کد، وگرنه تارگت کوپن باشگاه)، `TargetMatchesType` (اعتبارسنجی هنگام ثبت) |
| `Application/Services/Order/CartSrv/CartService.cs` | حذف کد هنگام عوض شدن فروشگاه فعال |
| `Persistence/Migrations/20261114120000_RebateTarget.cs` | migration |
| `scripts/store-coach-discount-codes.sql` | اسکریپت مستقیم: ستون‌ها + نمونه‌ی ثبت تخفیف پت‌شاپ، کد پت‌شاپ و کد مربی |
| `Application.Tests/RebateTargetTests.cs` | تست |
