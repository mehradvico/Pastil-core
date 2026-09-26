# راهنمای Flutter: فهرست یکپارچه‌ی ثبت‌نام‌های مدرسه‌ی نماینده

این راهنما مربوط به صفحه‌ی `companion_reservations_page.dart` است. صفحه باید ثبت‌نام‌های **تمام مدرسه‌های متعلق به نماینده‌ی لاگین‌شده** را نمایش دهد.

## قرارداد API

```http
GET /api/Companion/SchoolReserve?PageIndex=1&PageSize=20
Authorization: Bearer <access-token>
Accept: application/json
```

توکن اجباری است. سرور `CompanionId` را از توکن استخراج و نتایج را به تمام مدرسه‌هایی محدود می‌کند که متعلق به همان نماینده‌اند.

**`SchoolId` را در این درخواست نفرستید.** ارسال آن فقط نتایج را به یک مدرسه محدود می‌کند و علت نمایش‌ندادن ثبت‌نام‌های مدرسه‌های دیگر است. همچنین `CompanionId` را از کلاینت نفرستید؛ سرور آن را از توکن تعیین می‌کند.

فیلترهای مجاز و اختیاری:

| پارامتر | کاربرد |
| --- | --- |
| `PageIndex` | شماره‌ی صفحه، از ۱ شروع می‌شود؛ پیش‌فرض ۱ |
| `PageSize` | تعداد آیتم صفحه؛ پیش‌فرض ۲۰ و حداکثر ۱۰۰۰ |
| `StatusId` | وضعیت ثبت‌نام: `1` ثبت‌شده، `2` پرداخت‌شده، `3` تکمیل‌شده، `4` لغوشده |
| `SchoolCourseId` | محدودکردن نتایج به یک دوره (در صورت نیاز UI) |
| `BookerId` | محدودکردن نتایج به یک ثبت‌نام‌کننده (در صورت نیاز UI) |
| `SortBy` | `New` (پیش‌فرض؛ جدیدترین اول) یا `Old` |

## پاسخ موفق

نام کلیدهای JSON به صورت camelCase است. شکل پاسخ:

```json
{
  "pageIndex": 1,
  "pageSize": 20,
  "totalCount": 42,
  "list": [
    {
      "id": 9001,
      "reserveCode": "...",
      "schoolCourseId": 37,
      "bookerId": 101,
      "userPetId": 55,
      "price": 2500000,
      "paymentPrice": 2500000,
      "isReserved": true,
      "isCancel": false,
      "statusId": 2,
      "createDate": "2026-09-24T10:00:00",
      "schoolCourse": {
        "id": 37,
        "name": "...",
        "schoolId": 12,
        "school": { "id": 12, "name": "..." }
      },
      "userPet": { "id": 55, "name": "..." },
      "booker": { "id": 101 }
    }
  ]
}
```

همه‌ی فیلدهای نمونه لزوماً برای رندر صفحه لازم نیستند. برای نمایش نام مدرسه از `item.schoolCourse.school` استفاده کنید؛ مدرسه را از یک مقدار ثابت یا اولین مدرسه‌ی پروفایل نسازید.

## الگوی پیشنهادی در Flutter/Dio

```dart
Future<SchoolReservationPage> fetchReservations({
  required int page,
  int pageSize = 20,
  int? statusId,
}) async {
  final response = await dio.get<Map<String, dynamic>>(
    '/api/Companion/SchoolReserve',
    queryParameters: {
      'PageIndex': page,
      'PageSize': pageSize,
      if (statusId != null) 'StatusId': statusId,
      // عمداً SchoolId و CompanionId اضافه نمی‌شوند.
    },
    options: Options(
      headers: {'Authorization': 'Bearer $accessToken'},
    ),
  );

  return SchoolReservationPage.fromJson(response.data!);
}
```

برای refresh، صفحه را به ۱ برگردانید و فهرست قبلی را جایگزین کنید. برای بارگذاری بیشتر، تا زمانی که `list.length < totalCount` است صفحه‌ی بعدی را بگیرید. هنگام تغییر فیلتر وضعیت نیز لیست و شماره‌ی صفحه را reset کنید.

## معیار پذیرش

1. نماینده‌ای با دست‌کم دو مدرسه و حداقل یک ثبت‌نام در هر مدرسه وارد شود.
2. صفحه فقط یک درخواست به `GET /api/Companion/SchoolReserve?PageIndex=1&PageSize=...` بفرستد؛ query string نباید `SchoolId` یا `CompanionId` داشته باشد.
3. فهرست پاسخ باید ثبت‌نام‌های هر دو مدرسه را نشان دهد و نام مدرسه‌ی صحیح هر آیتم نمایش داده شود.
4. در صفحه‌بندی، مجموع آیتم‌های لودشده نباید از `totalCount` بیشتر شود و آیتم تکراری نباید نمایش داده شود.
5. خطای `401` به جریان ورود/تازه‌سازی توکن سپرده شود؛ سایر خطاها به‌صورت حالت خطای قابل تلاش مجدد نمایش داده شوند.
