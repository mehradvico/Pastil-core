# تخصیص رزرو خدمت به کاربر نمایندگی — مستند اپ

این مستند برای پیاده‌سازی و نگهداری فرایند تخصیص رزرو خدمت در `app.pastil.pet` تهیه شده است.

## مفهوم شناسه‌ها

- `UserId`: شناسه حساب کاربری شخص.
- `CompanionUserId`: شناسه عضویت شخص در یک نمایندگی.
- `CompanionAssistanceUserId`: شناسه اتصال همان شخص به یک خدمت مشخص نمایندگی.

در ثبت رزرو و عملیات Assign فقط باید `CompanionAssistanceUserId` ارسال شود. ارسال `UserId` یا `CompanionUserId` اشتباه است.

## فرایند نهایی

1. کاربر نمایندگی باید عضویت فعال و تأییدشده داشته باشد: `Active=true` و `UserAccept=true`.
2. نماینده باید آن کاربر را به خدمت موردنظر متصل کرده باشد؛ در نتیجه یک `CompanionAssistanceUser` فعال ساخته می‌شود.
3. پس از پرداخت موفق، رزرو `IsReserved=true` می‌شود.
4. صاحب نمایندگی از صفحه رزروها، مسئول انجام خدمت را انتخاب می‌کند.
5. بک تعلق کاربر به همان نمایندگی و همان خدمت، فعال بودن عضویت و تداخل زمانی را کنترل می‌کند.
6. رزرو به کاربر نمایندگی تخصیص داده می‌شود و برای او Push ارسال می‌شود.
7. کاربر نمایندگی رزرو را در مسیر `/operator` می‌بیند و نتیجه و هزینه نهایی را ثبت می‌کند.

اگر کاربر نهایی هنگام ثبت رزرو متخصصی انتخاب کند، همان اتصال خدمت به‌عنوان مسئول اولیه ذخیره می‌شود. Push متخصص بعد از پرداخت موفق ارسال خواهد شد.

## دریافت متخصصان همان خدمت در صفحه ثبت رزرو

```http
GET /api/CompanionAssistanceUser?CompanionAssistanceId={companionAssistanceId}&Available=true
```

از `item.id` پاسخ به‌عنوان `companionAssistanceUserId` استفاده شود.

نمونه بخشی از پاسخ:

```json
{
  "totalCount": 1,
  "list": [
    {
      "id": 18,
      "userId": 42,
      "companionAssistanceId": 7,
      "active": true,
      "user": {
        "firstName": "علی",
        "lastName": "رضایی"
      }
    }
  ]
}
```

Payload ثبت رزرو:

```json
{
  "companionAssistanceId": 7,
  "companionAssistanceUserId": 18
}
```

## دریافت کاربران قابل Assign توسط صاحب نمایندگی

```http
GET /api/Companion/CompanionReserveAssignee/{reserveId}
Authorization: Bearer {token}
```

نمونه پاسخ:

```json
{
  "isSuccess": true,
  "messages": [],
  "data": [
    {
      "companionAssistanceUserId": 18,
      "userId": 42,
      "fullName": "علی رضایی",
      "pictureId": 120,
      "isFemale": false,
      "expertiseId": 3,
      "expertiseName": "دامپزشک",
      "isAssigned": true
    }
  ]
}
```

فقط کاربران زیر برگردانده می‌شوند:

- عضو همان نمایندگی؛
- عضویت فعال و تأییدشده؛
- متصل به همان خدمت؛
- کاربر و اتصال خدمت حذف یا غیرفعال نشده باشند.

## Assign یا Reassign رزرو توسط صاحب نمایندگی

```http
PUT /api/Companion/CompanionReserveAssign
Authorization: Bearer {token}
Content-Type: application/json
```

```json
{
  "id": 125,
  "companionAssistanceUserId": 18
}
```

- `id`: شناسه `CompanionReserve`.
- `companionAssistanceUserId`: شناسه اتصال کاربر به همان خدمت.

در موفقیت، `data` شامل جزئیات کامل و به‌روز رزرو است.

### خطاهای قابل نمایش

- `فقط رزرو پرداخت‌شده و فعال قابل تخصیص است.`
- `رزرو انجام‌شده قابل تخصیص مجدد نیست.`
- `کاربر انتخاب‌شده متعلق به این خدمت نیست یا فعال نیست.`
- `عضویت کاربر انتخاب‌شده در این نمایندگی فعال و تأییدشده نیست.`
- `کاربر انتخاب‌شده در این بازه زمانی رزرو فعال دیگری دارد.`
- `شما دسترسی ندارید.`

در Reassign، وضعیت عملیاتی مسئول قبلی، توضیحات و مبالغ نهایی اپراتور پاک می‌شوند و وضعیت انجام خدمت دوباره روی «انجام نشده» قرار می‌گیرد.

## Endpointهای ادمین

برای پنل مدیریت اصلی همین قرارداد با Area ادمین در دسترس است:

```http
GET /api/Admin/CompanionReserveAssignee/{reserveId}
PUT /api/Admin/CompanionReserveAssign
```

Payload بخش ادمین با بخش نماینده یکسان است.

## پنل کاربر نمایندگی در اپ

مسیر:

```text
/operator
```

لیست رزروهای تخصیص‌یافته:

```http
GET /api/Operator/CompanionReserve?PageIndex=1&PageSize=50
Authorization: Bearer {token}
```

این Endpoint از اطلاعات توکن استفاده می‌کند و فقط رزروهای پرداخت‌شده‌ای را برمی‌گرداند که به همان کاربر تخصیص یافته‌اند. فرانت نباید `UserId` را در Query ارسال کند.

جزئیات یک رزرو:

```http
GET /api/Operator/CompanionReserve/{id}
Authorization: Bearer {token}
```

اگر رزرو متعلق به کاربر جاری نباشد، اطلاعات آن برگردانده نمی‌شود.

ثبت نتیجه نهایی مانند قبل انجام می‌شود:

```http
PUT /api/Operator/CompanionReserveOperatorUpdate
```

## Push Notification

پس از Assign یا پرداخت موفق رزروی که مسئول اولیه دارد، Push زیر برای کاربر نمایندگی ارسال می‌شود:

- نوع: `PushCompanionReserveAssigned`
- عنوان: `رزرو جدید برای شما`
- متن: `خدمت {نام خدمت} برای کاربر {نام کاربر} به شما اختصاص داده شد.`
- مسیر: `/operator`

کاربر باید Push Subscription فعال داشته باشد. خطای ارسال Push باعث Rollback شدن تخصیص رزرو نمی‌شود.

## نکات UI

- در کارت رزرو نماینده، نام مسئول فعلی نمایش داده شود.
- دکمه `تخصیص مسئول` فقط برای رزرو پرداخت‌شده و فعال نمایش داده شود.
- اگر مسئول وجود دارد، متن دکمه `تغییر مسئول` باشد.
- کاربر دارای `isCompanionUser=true` در پروفایل خود گزینه `خدمات تخصیص‌یافته به من` را ببیند.
- هنگام خطا، متن `messages[0].item1` مستقیماً به کاربر نمایش داده شود.
- بعد از Assign موفق، لیست رزروها مجدداً دریافت شود.

## تغییر دیتابیس

Migration زیر Push جدید را وارد می‌کند:

```text
20260816071411_SeedCompanionReserveAssignedPush
```

پس از انتشار بک باید `Update-Database` اجرا شود.
