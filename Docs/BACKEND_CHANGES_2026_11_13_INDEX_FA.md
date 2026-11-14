# فهرست تغییرات بک (۱۴۰۵/۰۸/۲۲ — ۲۰۲۶-۱۱-۱۳) و کار هر تیم

> همه‌ی این تغییرات **در بک و پنل ادمین انجام و تست شده‌اند، دیپلوی نشده‌اند.** هر ردیف سندِ کاملِ خودش را دارد.
> پاسخ‌های بک همیشه HTTP 200 با `{ isSuccess, messages, data }` هستند (به‌جز endpointهای لیستیِ مشخص‌شده در هر سند).

## ترتیب دیپلوی (مهم)

1. **SQL (کاربر اجرا کند)** — به ترتیب:
   1. `backend/scripts/DiscussionChatRoom.sql` (۳ ستون روی `DiscussionAnswers`)
   2. `backend/scripts/PastilFundedDiscount.sql` (ستون‌های تخفیف دولایه + بازنویسی procedure `UpdateProductItemDiscount`)
   هر دو چندبار اجرا امن‌اند. (بقیه‌ی موارد زیر SQL ندارند.)
2. **Api و Payment با هم** (محاسبه‌ی سهم سفارش در Payment هم اجرا می‌شود).
3. **پنل ادمین** (`panel/output.zip` — build مجدد).
4. **وب‌اپ** و **اپ نماینده/ارائه‌دهنده** (بعد از Api؛ اگر زودتر بیایند BFF/endpoint جدید ۴۰۴ می‌گیرد و بخش مربوط پنهان می‌ماند).

## جدول تغییرات

| # | تغییر | سند | وب‌اپ | اپ نماینده/ارائه‌دهنده | پنل (انجام شد) | SQL |
| --- | --- | --- | --- | --- | --- | --- |
| ۱ | دیدگاه کالا فقط بعد از «تحویل گرفتم» | [PRODUCT_COMMENT_PURCHASE_ONLY_WEBAPP_FA.md](PRODUCT_COMMENT_PURCHASE_ONLY_WEBAPP_FA.md) | حذف فرم از صفحه‌ی کالا + دکمه‌ی دیدگاه در جزئیات سفارش + ۲ BFF | — | — | — |
| ۲ | «کالاهای مشابه» و «در کنارش خریداری شده» هوشمند | [PRODUCT_SIMILAR_AND_BOUGHT_TOGETHER_WEBAPP_FA.md](PRODUCT_SIMILAR_AND_BOUGHT_TOGETHER_WEBAPP_FA.md) | ۲ BFF + بازنویسی `similarProducts.vue` | — | — | — |
| ۳ | «چیزی فراموش نکردید؟» (پیشنهاد مکمل سبد) | [CART_CROSS_SELL_SMART_WEBAPP_FA.md](CART_CROSS_SELL_SMART_WEBAPP_FA.md) | BFF + جایگزینی `collectRelatedProducts` در `CrossSellModal.vue` | — | — | — |
| ۴ | حداقل/حداکثر پیش‌پرداخت پکیج ارائه‌دهنده (سهم پاستیل ≤ پیش‌پرداخت ≤ کل مبلغ) | [PACKAGE_PREPAYMENT_MINIMUM_APP_FA.md](PACKAGE_PREPAYMENT_MINIMUM_APP_FA.md) | — | **بله**: محاسبه‌ی داینامیک حداقل با `effectiveCommissionPercent` + نمایش خطا | — | — |
| ۵ | پرسش و پاسخ کالا به‌صورت چت‌روم (تأیید ادمین، ریپلای، پاسخ فروشگاه) | [PRODUCT_QA_CHATROOM_WEBAPP_FA.md](PRODUCT_QA_CHATROOM_WEBAPP_FA.md) | بازنویسی `question.vue` + ۳ BFF | — | لیست/جزئیات `admin/discussionquestion` | `DiscussionChatRoom.sql` |
| ۶ | تخفیف محصول دولایه: سهم فروشگاه + سهم پاستیل، حسابداری سفارش | مرجع بک: [PASTIL_FUNDED_DISCOUNT_FA.md](PASTIL_FUNDED_DISCOUNT_FA.md) · وب‌اپ: [PASTIL_FUNDED_DISCOUNT_WEBAPP_FA.md](PASTIL_FUNDED_DISCOUNT_WEBAPP_FA.md) · اپ: [PASTIL_FUNDED_DISCOUNT_APP_FA.md](PASTIL_FUNDED_DISCOUNT_APP_FA.md) | پنل فروشنده: نشان «تخفیف پاستیل»، فیلدهای مالی | فقط اگر ماژول فروشگاه دارد | `admin/discount/*`، `admin/finance/store-[id]`، `admin/productorder/detail-[id]` | `PastilFundedDiscount.sql` |
| ۷ | همه‌ی فروشگاه‌های دارای کالا (ارزان به گران) + پیش‌فرض فروشگاه مبدأ + قیمت هر فروشگاه در صفحه‌ی فروشگاه | [PRODUCT_MULTI_STORE_OFFERS_WEBAPP_FA.md](PRODUCT_MULTI_STORE_OFFERS_WEBAPP_FA.md) | `storeId` در لینک کارت + انتخاب پیش‌فرض + لیست فروشندگان مرتب در `seller.vue` | — | — | اسکریپت داده‌ی `RecomputeStoreProductPrices.sql` (مستقل) |
| ۸ | روش‌های ارسال با قیمت فروشگاه: پست/تیپاکس/ماهکس/الوپیک، پس‌کرایه، الوپیک فقط تهران | [وب‌اپ](STORE_PRICED_SHIPPING_WEBAPP_FA.md) · [اپ نماینده](STORE_PRICED_SHIPPING_APP_FA.md) | بدون تغییر ساختاری (همان استعلام ارسال) | فرم تعریف روش ارسال فروشنده + قواعد اعتبارسنجی | `StorePricedShippingMethods.sql` (قبل از Api) |

## خلاصه‌ی کار هر تیم

### تیم وب‌اپ
- ۱) فرم دیدگاه را از `products/comments.vue` بردارید؛ در `reserve/order-[orderId].vue` بعد از `userReceived===true` دکمه‌ی «ثبت دیدگاه» زیر هر کالا (BFFهای `comments/orderEligibility.get.js`، `comments/eligibility.get.js`).
- ۲) `products/similarProducts.vue` را روی `/api/products/similarProducts` و `/api/products/boughtTogether` ببرید؛ تب خالی پنهان شود.
- ۳) `basket/CrossSellModal.vue`: `/api/cart/suggestions` با `storeId`؛ دلیل پیشنهاد (`reasons[]`) زیر هر کالا.
- ۵) `products/question.vue`: ترد چت با ریپلای، پیش‌نمایش «مطمئن هستید؟»، نمایش پیام‌های در انتظارِ خودِ کاربر و قفل ارسال (`/api/DiscussionChat/*`).
- ۶) پنل فروشنده: ردیف‌های `fundedByPastil` فقط‌خواندنی با نشان؛ `pastilFundedAmount` در سفارش و مالی (سند وب‌اپ تخفیف).

### اپ‌نویس نماینده/ارائه‌دهنده
- ۴) در فرم ثبت/ویرایش پکیج (کلینیک، مربی، آرایشگاه): حداقل پیش‌پرداخت = `price × effectiveCommissionPercent ÷ 100` (رو به بالا)، سقف = `price` (برای پانسیون آموزشی مربی: `price + dailyPrice × stayDays`). فیلد `effectiveCommissionPercent` روی هر خدمت در لیست خدمات می‌آید. خطای سرور فارسی است.
- ۶) فقط اگر ماژول «فروشگاه» دارد: `fundedByPastil` را فقط‌خواندنی نشان دهد و فیلدهای مالی جدید را اضافه کند (سند اپِ تخفیف).

### پنل ادمین (انجام شده — فقط build/deploy)
- تخفیف: سوئیچ «از سهم پاستیل» + شناسه‌ی فروشگاه؛ نشان در لیست/ویرایش.
- مالی فروشگاه و جزئیات سفارش: تخفیف سهم پاستیل.
- پرسش و پاسخ: فیلتر «در انتظار»، شمارنده‌ی پاسخ‌های در انتظار، «رد پاسخ»، نشان «پاسخ فروشگاه».

## نکته‌های مشترک
- پیام‌های خطای بک فارسی‌اند و در `messages[0].item1` می‌آیند؛ مستقیم نمایش دهید.
- هیچ‌کدام از اینها فیلد قبلی را حذف یا تغییر معنا نداده‌اند؛ فقط فیلد/endpoint اضافه شده (به‌جز سخت‌گیری امنیتی: دیدگاه کالا فقط خریدار تحویل‌گرفته، و پرسش/پاسخ همیشه «در انتظار تأیید» ثبت می‌شود).
- گزینه‌ی باز: «هزینه‌ی ارسال از کمیسیون جدا شود؟» (سند ۶، بخش ۸) — هنوز تصمیم گرفته نشده و رفتار فعلی ارسال دست‌نخورده است.
