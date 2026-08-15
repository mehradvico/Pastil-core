# مستند درخواست راننده، نماینده، فروشگاه و پانسیون

این مستند برای پیاده‌سازی فرایند درخواست همکاری در `app.pastil.pet` است. تمام Endpointهای متقاضی به توکن کاربر نیاز دارند و مالک درخواست از توکن استخراج می‌شود؛ بنابراین `ownerId` یا `userId` ارسالی فرانت ملاک دسترسی نیست.

## قرارداد عمومی پاسخ

همه عملیات‌های ثبت، ویرایش و تصمیم ادمین با قرارداد عمومی بک برمی‌گردند:

```json
{
  "isSuccess": true,
  "messages": [],
  "data": {}
}
```

فرانت حتی در HTTP 200 باید `isSuccess` را بررسی کند. در خطاهای اعتبارسنجی، هر عضو `messages` شامل متن خطا و نام فیلد است؛ خطا را کنار همان فیلد نمایش دهید.

## چرخه وضعیت

1. کاربر فرم را ثبت می‌کند.
2. درخواست با وضعیت «در انتظار بررسی» ذخیره می‌شود و Notice پنل ادمین ایجاد می‌شود.
3. ادمین درخواست را تأیید یا با دلیل رد می‌کند.
4. برای کاربر Push ارسال می‌شود. کلیک روی Push به صفحه همان درخواست می‌رود.
5. درخواست ردشده قابل ویرایش و ارسال مجدد است و دوباره در انتظار بررسی قرار می‌گیرد.
6. درخواست تأییدشده از Endpoint ارسال مجدد قابل تغییر نیست و باید از صفحات مدیریتی عادی ویرایش شود.

تشخیص وضعیت در فرانت:

| نوع | در انتظار | ردشده | تأییدشده |
|---|---|---|---|
| نماینده | `approved=false`, `active=false`, `activationValue=null` | `approved=false`, `activationValue` دارای دلیل | `approved=true`, `active=true` |
| فروشگاه | `approved=false`, `active=false`, `approvalValue=null` | `approved=false`, `approvalValue` دارای دلیل | `approved=true`, `active=true` |
| پانسیون | `approve=false`, `active=false`, `approvalValue=null` | `approve=false`, `approvalValue` دارای دلیل | `approve=true`, `active=true` |
| راننده | `statusId=82` | `statusId=84` و `adminDetail` دارای دلیل | `statusId=83`, `approved=true`, `active=true` |

## آپلود تصاویر

تصاویر ابتدا با API استاندارد `Picture` پروژه آپلود می‌شوند. شناسه خروجی را در فیلدهای `pictureId`، `iconId`، `profilePictureId`، `certificatePictureId` یا `vehicleCardPictureId` ارسال کنید. فایل Base64 یا خود فایل را داخل درخواست ثبت همکاری نفرستید.

## درخواست رانندگی

### APIها

- لیست درخواست‌های خود کاربر: `GET /api/EndUser/Driver`
- جزئیات درخواست خود کاربر: `GET /api/EndUser/Driver/{id}`
- ثبت: `POST /api/EndUser/Driver`
- ویرایش و ارسال مجدد: `PUT /api/EndUser/Driver`

نمونه ثبت:

```json
{
  "name": "مهراد حسینی",
  "phone": "09339750767",
  "vehicle": "پراید سفید",
  "licensePlateNumber": "12الف345-67",
  "ownerDetail": "توضیحات کاربر",
  "cityId": 1,
  "neighborhoodId": null,
  "profilePictureId": 101,
  "certificatePictureId": 102,
  "vehicleCardPictureId": 103
}
```

`certificatePictureId` و `vehicleCardPictureId` الزامی هستند. در `PUT` همین بدنه به‌همراه `id` ارسال می‌شود. فیلدهای `ownerId`، `statusId`، `active`، `approved`، `rate` و `adminDetail` توسط بک کنترل می‌شوند.

## درخواست نمایندگی

### APIها

- لیست درخواست‌های خود کاربر: `GET /api/EndUser/Companion`
- جزئیات درخواست خود کاربر: `GET /api/EndUser/Companion/{id}`
- ثبت: `POST /api/EndUser/Companion`
- ویرایش و ارسال مجدد: `PUT /api/EndUser/Companion`

نمونه ثبت:

```json
{
  "name": "کلینیک پاستیل",
  "isPersonal": false,
  "phone": "02112345678",
  "cityId": 1,
  "neighborhoodId": null,
  "addressValue": "تهران، ...",
  "location": { "x": 51.389, "y": 35.689 },
  "pictureId": 110,
  "backgroundPictureId": 111,
  "iconId": 112,
  "summary": "معرفی کوتاه",
  "description": "توضیحات کامل"
}
```

در `PUT`، `id` نیز ارسال شود. `ownerId`، `referralCode`، حساب طلایی/نقره‌ای، `approved`، `active`، `activationValue` و `showToSite` توسط بک کنترل می‌شوند.

## درخواست فروشگاه

### APIها

- لیست درخواست‌های خود کاربر: `GET /api/EndUser/Store`
- جزئیات درخواست خود کاربر: `GET /api/EndUser/Store/{id}`
- ثبت: `POST /api/EndUser/Store`
- ویرایش و ارسال مجدد: `PUT /api/EndUser/Store`

نمونه ثبت:

```json
{
  "name": "پت‌شاپ پاستیل",
  "phone": "02112345678",
  "mobile": "09121234567",
  "email": "shop@example.com",
  "address": "تهران، ...",
  "location": { "x": 51.389, "y": 35.689 },
  "typeId": 1,
  "cityId": 1,
  "pictureId": 120,
  "iconId": 121,
  "summary": "معرفی کوتاه",
  "description": "توضیحات کامل"
}
```

`typeId` باید از Code/CodeGroup مربوط به نوع فروشگاه و `cityId` از API شهرها دریافت شود. نام، آدرس، شهر، نوع فروشگاه و حداقل یکی از `phone` یا `mobile` الزامی است. در `PUT`، `id` نیز ارسال شود. `referralCode`، تخفیف، کمیسیون، امتیاز، `approved`، `active` و `showToSite` مدیریتی هستند.

## درخواست پانسیون توسط نماینده

این API فقط با حساب نماینده تأییدشده قابل استفاده است. پانسیون زیرمجموعه همان نماینده توکن است و فرانت نباید اجازه انتخاب نماینده دیگری بدهد.

### APIها

- لیست پانسیون‌های نماینده: `GET /api/Companion/Pansion`
- جزئیات: `GET /api/Companion/Pansion/{id}`
- ثبت درخواست: `POST /api/Companion/Pansion`
- ویرایش و ارسال مجدد: `PUT /api/Companion/Pansion`

نمونه ثبت:

```json
{
  "name": "پانسیون شبانه‌روزی پاستیل",
  "isSchool": false,
  "stateId": 1,
  "cityId": 1,
  "addressValue": "تهران، ...",
  "discription": "توضیحات مرکز",
  "pictureId": 130,
  "pansionPrice": 500000,
  "schoolPrice": 0,
  "regulations": "قوانین پذیرش",
  "openHour": "08:00",
  "closeHour": "22:00"
}
```

برای مدرسه `isSchool=true` و `schoolPrice>0`، و برای پانسیون `isSchool=false` و `pansionPrice>0` لازم است. شهر باید متعلق به استان باشد. `companionId` از توکن اعمال می‌شود. `approve`، `active`، `suggested`، امتیاز، کمیسیون و `showToSite` مدیریتی هستند.

## Endpointهای پنل ادمین

لیست‌ها همان Endpointهای فعلی پنل هستند:

- راننده‌ها: `GET /api/Admin/Driver`
- نماینده‌ها: `GET /api/Admin/Companion`
- فروشگاه‌ها: `GET /api/Admin/Store`
- پانسیون‌ها: `GET /api/Admin/Pansion`

تصمیم ادمین:

```http
PUT /api/Admin/DriverUpdateStatus
```

```json
{ "id": 10, "statusId": 83, "adminDetail": null }
```

برای رد راننده `statusId=84` و `adminDetail` اجباری است.

```http
PUT /api/Admin/CompanionActivation
```

```json
{ "id": 10, "approved": true, "activationValue": null }
```

```http
PUT /api/Admin/StoreApproval
```

```json
{ "id": 10, "approved": false, "approvalValue": "تصویر مجوز خوانا نیست." }
```

```http
PUT /api/Admin/PansionApprove
```

```json
{ "id": 10, "approve": false, "approvalValue": "آدرس و ساعات فعالیت را اصلاح کنید." }
```

برای رد نماینده، فروشگاه و پانسیون، دلیل رد اجباری است. پنل باید متن دلیل را در جزئیات درخواست نشان دهد.

## Push و Notice

- ثبت اولیه و ارسال مجدد هر چهار نوع درخواست، Notice جدید در پنل ادمین ایجاد می‌کند.
- تأیید/رد، Push مخصوص همان کاربر ایجاد می‌کند.
- Push رد شامل دلیل رد است.
- مقصد Pushها:
  - نماینده: `/profile/companion-request`
  - راننده: `/profile/driver-request`
  - فروشگاه: `/profile/store-request`
  - پانسیون: `/companion/pansion`

فرانت باید این Routeها را ایجاد کند یا در Router پوش به Route متناظر موجود نگاشت دهد.

## نکات UI

- هنگام `isSuccess=false` فرم را نبندید و پیام فیلدمحور را نمایش دهید.
- در وضعیت در انتظار، دکمه «در حال بررسی» نمایش داده شود.
- در وضعیت ردشده، دلیل رد و دکمه «اصلاح و ارسال مجدد» نمایش داده شود.
- در وضعیت تأییدشده، کاربر به صفحه مدیریت راننده/نماینده/فروشگاه/پانسیون هدایت شود و دیگر از API ارسال مجدد استفاده نشود.
- Dropdown شهر، محله، نوع فروشگاه و تصاویر باید از APIهای داینامیک پروژه پر شوند؛ شناسه دستی وارد نشود.

## استقرار بک

Migration زیر باید قبل از تست نهایی روی دیتابیس اجرا شود:

`20260814194436_AddPartnerApplicationWorkflow`

این Migration فیلدهای نتیجه بررسی فروشگاه/پانسیون و تنظیمات Push/Notice موردنیاز را اضافه می‌کند.
