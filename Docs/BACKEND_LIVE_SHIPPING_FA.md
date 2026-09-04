# معماری ارسال لحظه‌ای پاستیل

## ساختار

- `Entities/Entities/ShippingField`: Quote، Shipment و Enumهای دامنه
- `Application/Services/Order/ShippingSrv/Provider`: قرارداد Provider و Adapterهای الوپیک، تیپاکس، اسنپ‌باکس و میاره
- `Application/Services/Order/ShippingSrv/ShippingQuoteService.cs`: قیمت‌گیری، Snapshot و انتخاب امن Quote
- `Application/Services/Order/ShippingSrv/ShipmentService.cs`: ساخت مرسوله بعد از پرداخت موفق
- `Api/Areas/EndUser/Controllers/ShippingQuoteController.cs`: دریافت قیمت
- `Api/Areas/EndUser/Controllers/ShippingSelectionController.cs`: انتخاب Quote
- `Api/Controllers/MiareWebhookController.cs`: دریافت Webhook میاره (بدون Auth کاربر، با Token اختصاصی میاره)

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
PASTIL_SHIPPING_MIARE_BASE_URL=
PASTIL_SHIPPING_MIARE_API_KEY=
PASTIL_SHIPPING_MIARE_ACCOUNTING_BASE_URL=
```

کلیدها نباید داخل appsettings یا Git قرار گیرند.

## وضعیت Adapterها

Adapterهای الوپیک، تیپاکس و اسنپ‌باکس هنوز Test Mode هستند و Quote و Shipment تستی تولید می‌کنند؛ به‌صورت Fail-closed نوشته شده‌اند و تا دریافت مستند و قرارداد رسمی هر Provider هیچ قیمت یا Shipment ساختگی در Production ثبت نمی‌کنند.

پس از دریافت مستند هر شرکت، فقط متدهای `GetQuoteAsync` و `CreateShipmentAsync` Adapter همان Provider باید به HTTP API رسمی متصل شوند. سرویس Cart، Order و Payment نیاز به تغییر ندارد.

### میاره (Miare) — اولین Adapter زنده

برخلاف سه Provider بالا، Adapter میاره (`MiareShippingProvider.cs`) مستقیماً به API واقعی میاره وصل است:

- `GetQuoteAsync`: استعلام واقعی قیمت روی `GET /estimate/price/` (Base URL جدای Accounting:
  `Shipping:Miare:AccountingBaseUrl` / `.../api/accounting`، جدا از Base URL بالا که مخصوص Trip Management
  است). قیمت پاسخ میاره به تومان است و قبل از ذخیره در Quote، در کد ضرب‌در‌ده می‌شود تا با واحد ریالی
  بقیه‌ی سیستم یکی باشد. اگر `area_coverage` پاسخ `false` باشد، استعلام Fail می‌شود (پیام:
  `ShippingProviderAreaNotCoveredFormat`).
- `CreateShipmentAsync` / `CancelShipmentAsync`: واقعی، روی `POST /trips/` و `POST /trips/{id}/cancel/`.
- **Webhook**: `Api/Controllers/MiareWebhookController.cs` روی `POST /api/webhooks/miare` رویدادهای میاره
  (`state_changed`, `delivered`, `courier_assigned`, ...) را دریافت می‌کند. هدر `Authorization: Token <key>`
  با `Shipping:Miare:ApiKey` تطبیق داده می‌شود؛ همیشه سریع `200` برمی‌گرداند (طبق رفتار Retry میاره: تا ۵ بار
  با فاصله‌ی ۵ ثانیه روی غیر ۲XX) و خطاها فقط لاگ می‌شوند. پردازش واقعی در
  `ShipmentService.HandleMiareWebhookAsync` است: `state` سفر میاره به `ShipmentStatusEnum` نگاشت می‌شود
  (`assign_queue→Requested`, `pickup→Accepted`, `dropoff→PickedUp`, `delivered→Delivered`,
  `canceled_by_*→Cancelled`, `returning→Failed`).
  - مبلغ واقعی `delivery_cost` (هزینه‌ی سفر نزد میاره، به تومان — مبلغی متفاوت از `ChargedPrice`/`QuotedPrice`
    که مبلغ دریافتی از مشتری‌اند) روی رویداد `delivered` در ستون جدید `Shipment.ProviderCost` (ریال، بعد از
    ضرب‌در‌ده) ذخیره می‌شود. Migration: `AddShipmentProviderCost` (فقط تولید شده؛ هنوز روی دیتابیس اجرا نشده،
    طبق قانون شماره ۵ در CLAUDE.md اصلی پروژه اجرای آن نیاز به تایید و اقدام صریح کاربر دارد).
- توکن فعلی مربوط به محیط Staging میاره است (`ws.staging.miare.ir` / `www.staging.miare.ir`)، نه Production؛
  آدرس Webhook بالا هنوز باید نزد میاره ثبت شود (از طریق پنل یا پشتیبانی میاره).

## دیتابیس

Migrationها:

```text
AddLiveShippingProviders
AddShipmentProviderCost
```

`AddLiveShippingProviders`:

- جدول‌های `ShippingQuotes` و `Shipments` را ایجاد می‌کند.
- Snapshot ارسال را به `CartStores` و `ProductOrderStores` اضافه می‌کند.
- تنظیم Provider را به `Deliveries` اضافه می‌کند.
- وزن و ابعاد ارسال را به `Products` اضافه می‌کند.
- Codeهای الوپیک و اسنپ‌باکس را در گروه فعلی DeliveryType درج می‌کند.

`AddShipmentProviderCost`:

- ستون `ProviderCost` (`float`, nullable) را به `Shipments` اضافه می‌کند — هزینه‌ی واقعی سفر نزد میاره
  (از Webhook، رویداد `delivered`)، جدا از `ChargedPrice`/`QuotedPrice`.

