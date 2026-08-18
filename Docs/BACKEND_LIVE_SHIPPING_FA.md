# معماری ارسال لحظه‌ای پاستیل

## ساختار

- `Entities/Entities/ShippingField`: Quote، Shipment و Enumهای دامنه
- `Application/Services/Order/ShippingSrv/Provider`: قرارداد Provider و Adapterهای الوپیک، تیپاکس و اسنپ‌باکس
- `Application/Services/Order/ShippingSrv/ShippingQuoteService.cs`: قیمت‌گیری، Snapshot و انتخاب امن Quote
- `Application/Services/Order/ShippingSrv/ShipmentService.cs`: ساخت مرسوله بعد از پرداخت موفق
- `Api/Areas/EndUser/Controllers/ShippingQuoteController.cs`: دریافت قیمت
- `Api/Areas/EndUser/Controllers/ShippingSelectionController.cs`: انتخاب Quote

## اصول امنیتی

- قیمت، Provider، نوع پرداخت و DeliveryId از فرانت اعتماد نمی‌شوند.
- فرانت فقط `QuoteToken` غیرقابل حدس را انتخاب می‌کند.
- Quote به User، CartStore، Address و محتوای سبد متصل است.
- اعتبار Quote پنج دقیقه و قابل تنظیم است.
- پس‌کرایه از ارسال رایگان تفکیک شده است.
- خطای Provider بعد از پرداخت، نتیجه پرداخت موفق را تغییر نمی‌دهد و Shipment را Failed می‌کند.
- ساخت Shipment نسبت به `ProductOrderStoreId` idempotent است.

## تنظیمات سرور

```env
PASTIL_SHIPPING_TEST_MODE=true
PASTIL_SHIPPING_ALOPEYK_BASE_URL=
PASTIL_SHIPPING_ALOPEYK_API_KEY=
PASTIL_SHIPPING_TIPAX_BASE_URL=
PASTIL_SHIPPING_TIPAX_API_KEY=
PASTIL_SHIPPING_SNAPPBOX_BASE_URL=
PASTIL_SHIPPING_SNAPPBOX_API_KEY=
```

کلیدها نباید داخل appsettings یا Git قرار گیرند.

## وضعیت Adapterها

Adapterهای Test Mode کامل هستند و Quote و Shipment تستی تولید می‌کنند. Adapterهای Production به‌صورت Fail-closed نوشته شده‌اند؛ تا زمانی که Endpoint و قرارداد رسمی Provider تنظیم نشده باشد هیچ قیمت یا Shipment ساختگی در Production ثبت نمی‌شود.

پس از دریافت مستند هر شرکت، فقط متدهای `GetQuoteAsync` و `CreateShipmentAsync` Adapter همان Provider باید به HTTP API رسمی متصل شوند. سرویس Cart، Order و Payment نیاز به تغییر ندارد.

## دیتابیس

Migration:

```text
AddLiveShippingProviders
```

این Migration:

- جدول‌های `ShippingQuotes` و `Shipments` را ایجاد می‌کند.
- Snapshot ارسال را به `CartStores` و `ProductOrderStores` اضافه می‌کند.
- تنظیم Provider را به `Deliveries` اضافه می‌کند.
- وزن و ابعاد ارسال را به `Products` اضافه می‌کند.
- Codeهای الوپیک و اسنپ‌باکس را در گروه فعلی DeliveryType درج می‌کند.

