# راهنمای Flutter نماینده: مدیریت تخفیف فروشگاه

این راهنما برای صفحه‌ی مدیریت تخفیف نماینده/فروشنده است. همه‌ی endpointها با توکن فروشنده کار می‌کنند و سرور `StoreId` را از توکن می‌گیرد.

## قرارداد API

```http
GET    /api/Seller/Discount
POST   /api/Seller/Discount
PUT    /api/Seller/Discount
DELETE /api/Seller/Discount
GET    /api/Seller/Store/{storeId}
Authorization: Bearer <access-token>
```

در درخواست‌های `POST`، `PUT` و `DELETE`، به `storeId` ارسالی از اپ اعتماد نمی‌شود و سرور آن را با فروشگاه همان نماینده جایگزین می‌کند. با این حال بهتر است اپ آن را اصلاً در payload نفرستد.

## نوع تخفیف و هدف

`typeId` باید شناسه‌ی Code متناظر با یکی از نوع‌های زیر باشد. اگر اپ Codeها را از API می‌گیرد، نوع را بر اساس `label` تشخیص دهد، نه متن نمایشی.

| label | هدف اجباری | شرح |
| --- | --- | --- |
| `DiscountType_Store` | ندارد | تخفیف برای همه‌ی اقلام فروشگاه |
| `DiscountType_Category` | `categoryId` | فقط دسته‌ای که فروشگاه حداقل یک کالا در آن دارد |
| `DiscountType_Brand` | `brandId` | فقط برندی که فروشگاه حداقل یک کالا از آن دارد |
| `DiscountType_Product` | `productId` | فقط محصولی که فروشگاه برای آن SKU دارد |
| `DiscountType_ProductItem` | `productItemId` | فقط SKU متعلق به همان فروشگاه |

سرور مالکیت هدف را دوباره بررسی می‌کند. برای نوع‌های دسته، برند، محصول و SKU، انتخاب شناسه‌ی متعلق به فروشگاه دیگر با پاسخ ناموفق و پیام `DISCOUNT_TARGET_NOT_OWNED_BY_STORE` رد می‌شود. بنابراین لیست انتخاب‌گرهای Flutter را فقط از کالاها، SKUها، دسته‌ها و برندهای همان فروشگاه بسازید.

## ساخت تخفیف

نمونه‌ی تخفیف برای یک SKU:

```json
{
  "typeId": 99,
  "discountGroupId": 1,
  "percent": 15,
  "endDate": "2026-10-30T00:00:00",
  "productItemId": 841,
  "active": true
}
```

نمونه‌ی Dio:

```dart
Future<void> createSkuDiscount({
  required int typeId,
  required int discountGroupId,
  required int productItemId,
  required int percent,
  DateTime? endDate,
}) async {
  final response = await dio.post<Map<String, dynamic>>(
    '/api/Seller/Discount',
    data: {
      'typeId': typeId,
      'discountGroupId': discountGroupId,
      'percent': percent,
      'endDate': endDate?.toIso8601String(),
      'productItemId': productItemId,
      'active': true,
      // StoreId نفرستید؛ سرور آن را از توکن می‌گیرد.
    },
    options: Options(headers: {'Authorization': 'Bearer $accessToken'}),
  );

  final body = response.data!;
  if (body['isSuccess'] != true) {
    throw DiscountApiException(body['messages']);
  }
}
```

قواعد ورودی:

- `percent` باید بین ۱ تا ۹۹ باشد.
- `endDate` در صورت ارسال، نباید قبل از امروز باشد.
- فقط فیلد هدف مربوط به نوع انتخابی را بفرستید؛ سرور فیلدهای هدف نامرتبط را پاک می‌کند.
- برای تخفیف فروشگاه، هیچ فیلد هدفی نفرستید.

## فهرست، فعال/غیرفعال و حذف

فهرست تخفیف‌های فروشگاه لاگین‌شده:

```http
GET /api/Seller/Discount?PageIndex=1&PageSize=20&SortBy=New
```

برای فعال/غیرفعال‌کردن، فقط شناسه و وضعیت جدید لازم است:

```json
{ "id": 120, "active": false }
```

برای حذف:

```json
{ "id": 120 }
```

پس از هر سه عملیات موفق (`POST`، `PUT` یا `DELETE`)، فهرست تخفیف را دوباره بگیرید.

## سقف تخفیف فروشگاه

سرور پس از ساخت، تغییر وضعیت، حذف یا انقضای تخفیف، `maxDiscountPercent` فروشگاه را محاسبه و ذخیره می‌کند. اپ نباید آن را محلی محاسبه کند یا به پاسخ ایجاد تخفیف تکیه کند.

پس از عملیات موفق، برای نمایش نشان/برچسب فروشگاه مقدار تازه را از سرور بخوانید:

```http
GET /api/Seller/Store/{storeId}
```

از `data.maxDiscountPercent` استفاده کنید. اگر نماینده آخرین تخفیف فعال را حذف یا غیرفعال کند، این مقدار می‌تواند `0` شود.

## معیار پذیرش Flutter

1. نماینده نتواند در payload، تخفیف را به فروشگاه دیگری منتسب کند.
2. انتخاب SKU، محصول، برند و دسته فقط داده‌های فروشگاه فعلی را نشان دهد.
3. اگر سرور `DISCOUNT_TARGET_NOT_OWNED_BY_STORE` بازگرداند، فرم پیام مناسب نشان دهد و submit تکرار نشود.
4. پس از ساخت/فعال‌سازی/حذف، فهرست و `maxDiscountPercent` از سرور refresh شوند.
5. مبلغ نهایی کالا و تخفیف قابل اعمال همیشه از API سرور خوانده شود؛ محاسبه‌ی نهایی قیمت در Flutter انجام نشود.
