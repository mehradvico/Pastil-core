# لغو سفارش و کم/حذف کالا توسط ادمین و فروشنده

مشتری بعد از پرداخت **هیچ‌چیز** را نمی‌تواند لغو یا حذف کند. فقط ادمین و فروشنده، **قبل از ارسال**، دو کار می‌توانند بکنند و مبلغ همیشه به **کیف پول پاستیل مشتری** برمی‌گردد.

## ۱) لغو کامل سفارش
- ادمین: `POST api/Admin/ProductOrderCancel` با `{ orderId, reason? }`
- فروشنده: `POST api/Seller/ProductOrderCancel` با `{ orderId, reason }` (دلیل الزامی؛ فروشگاه از توکن می‌آید)
- مبلغ برگشتی = کل پرداختی سفارش (`PaymentPrice`: سهم کیف پول + درگاه، با ارسال).
- موجودی کالاها برمی‌گردد، امتیاز باشگاه برگشت می‌خورد، سفارش «لغو شده» می‌شود.
- شرط‌ها: پرداخت‌شده، وضعیت «ثبت/در حال پردازش»، لغو نشده، مشتری «تحویل گرفتم/نگرفتم» نزده.
- فروشنده فقط سفارش تک‌فروشگاهی خودش را کامل لغو می‌کند.

## ۲) کم/حذف یک کالا
- ادمین: `PUT api/Admin/ProductOrderItemAdjust`، فروشنده: `PUT api/Seller/ProductOrderItemAdjust`
- بدنه: `{ productOrderItemId, newCount, reason? }` — `newCount = 0` یعنی حذف؛ فقط کاهش مجاز است.
- هزینه‌ی کاسته‌شده (با تخفیف/کد تخفیف متناسب؛ هزینه‌ی ارسال سر جایش می‌ماند) به کیف پول برمی‌گردد.
- اگر با این تغییر کل سفارش خالی شود، لغو کامل انجام می‌شود. اگر همه‌ی کالاهای یک فروشگاه از سفارش چندفروشگاهی خالی شود، خطای `OrderAdjustStoreWouldBeEmpty`.
- مسیر قدیمی `ProductOrderItem` (UpdateDto) دیگر تعداد/حذف را عوض نمی‌کند، فقط توضیحات.

## وضعیت پیک (میاره)
| وضعیت مرسوله | لغو کامل | کم/حذف کالا |
|---|---|---|
| قبل از درخواست پیک | ادمین و فروشنده | ادمین و فروشنده |
| پیک درخواست/پذیرفته شده | فقط ادمین (سفر میاره لغو می‌شود؛ اگر نشد هشدار `OrderCancelMiareWarning`) | مجاز |
| پیک کالا را برداشته/تحویل شده | هیچ‌کس | هیچ‌کس |

## اطمینان از واریز یک‌بار
دفتر کیف پول با نام یکتا ثبت می‌شود: `OrderCancelRefund:{orderId}` و `OrderItemRefund:{itemId}:{newCount}`؛ تکرار درخواست، دوبار پول برنمی‌گرداند.

## پیامک (قالب‌های کاوه‌نگار — نام قالب = نام enum بدون `_`)
هر کدام یک `MessageType` (Label = نام enum) و `SmsSetting` در پنل لازم دارد. فاصله‌ها در توکن‌ها خودکار به نیم‌فاصله تبدیل می‌شود.

| قالب | گیرنده | توکن‌ها |
|---|---|---|
| ProductOrderCancelledByAdminStore | فروشگاه | token=کد سفارش |
| ProductOrderCancelledByAdminUser | مشتری | token=کد سفارش (لینک سفارش با همان کد ساخته می‌شود) |
| ProductOrderCancelledByStoreAdmin | ادمین | token=کد، token2=نام مشتری، token10=فروشگاه، token20=دلیل |
| ProductOrderCancelledByStoreUser | مشتری | token=نام، token2=کد، token10=فروشگاه |
| ProductOrderItemChangedByAdminStore | فروشگاه | token=کد، token10=کالا، token20=شرح تغییر |
| ProductOrderItemChangedByAdminUser | مشتری | token=کد، token10=کالا، token20=کاهش یافت/حذف شد |
| ProductOrderItemChangedByStoreUser | مشتری | token=کد، token2=کاهش داد/حذف کرد، token10=فروشگاه، token20=کالا |

متن پیشنهادی (مطابق خواسته‌ی محصول):
- AdminStore: «سفارش %token توسط پاستیل لغو شد؛ لطفا آماده‌سازی و ارسال سفارش را انجام ندهید.»
- AdminUser: «سفارش %token توسط پاستیل لغو شد و مبلغ آن به کیف پول شما برگشت. لینک سفارش: pastil.pet/orders/%token»
- StoreAdmin: «سفارش %token (%token2) توسط فروشگاه %token10 لغو شد. دلیل: %token20»
- StoreUser: «%token عزیز، متاسفیم؛ سفارش %token2 توسط فروشگاه %token10 لغو شد و مبلغ آن به کیف پول پاستیل شما واریز گردید.»
- ItemChangedByAdminStore: «سفارش %token توسط پاستیل تغییر کرد: %token10 — %token20»
- ItemChangedByAdminUser: «در سفارش %token محصول %token10 توسط پاستیل %token20 و هزینه آن به کیف پول شما واریز گردید.»
- ItemChangedByStoreUser: «متاسفیم؛ فروشگاه %token10 محصول %token20 را در سفارش %token %token2 و هزینه آن به کیف پول شما واریز گردید.»

## پوش
`PushOrderCancelledByAdminStore`(107)، `PushOrderCancelledByStoreAdmin`(108)، `PushOrderChangedByAdminStore`(109)، `PushOrderChangedByStoreAdmin`(110) — با مایگریشن `SeedOrderAdjustPush` و اسکریپت `scripts/AddOrderCancelAdjust.sql` (idempotent؛ شامل زنجیره‌ی تا `SeedOrderReceiptPush`).

## پنل ادمین
صفحه‌ی جزئیات سفارش: «تأیید/لغو سفارش» حالا لغو واقعی با برگشت پول است؛ روی هر کالا دکمه‌ی «کاهش / حذف کالا». توضیح ادمین به‌عنوان دلیل ارسال می‌شود.

## اپ فروشنده و وب‌اپ
- اپ فروشنده: دو endpoint بالا؛ پاسخ استاندارد `{IsSuccess, Messages}`؛ دکمه‌ها را فقط قبل از ارسال نشان دهید.
- وب‌اپ مشتری: بعد از پرداخت هیچ دکمه‌ی لغو/حذفی نشان داده نشود (مشتری فقط پیام و کیف پول را می‌بیند).
