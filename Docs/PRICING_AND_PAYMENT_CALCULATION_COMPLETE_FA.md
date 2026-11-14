# مستند سراسری دریافت و محاسبهٔ قیمت در پاستیل

مبنای بررسی: کد بک‌اند مخزن در commit `d15ef9e1923b5d754c45afb30600a91cee6c68b3`، بررسی‌شده در ۱۴۰۵/۰۷/۲۶. این سند قرارداد و رفتار **کد موجود** است، نه تضمین وضعیت دیتابیس یا نسخهٔ مستقر. مسیرها نسبت به میزبان API نوشته شده‌اند. برای هر پاسخ عملیاتی، `HTTP 200` به‌تنهایی موفقیت نیست؛ `isSuccess`، `code` و `messages` نیز باید بررسی شوند.

## ۱. نقشهٔ سریع: قیمت هر بخش از کجا می‌آید؟

| بخش | منبع نرخ/قیمت | API دریافت/پیش‌نمایش | مبلغ نهاییِ همان جریان |
|---|---|---|---|
| کالای فروشگاهی | `ProductItem.basePrice` فروشنده و قیمت مؤثر `ProductItem.price` پس از تخفیف | `GET /api/Product`، `GET /api/Product/{id}`، `GET /api/Seller/ProductItem?productId=...`، سبد با `POST /api/EndUser/Cart` | `Cart.paymentPrice` و سپس `ProductOrder.paymentPrice` |
| ارسال سفارش | Delivery ثابت/فاصله‌ای یا Quote ارائه‌دهندهٔ زنده؛ حالت پیش‌پرداخت/پرداخت گیرنده | `POST /api/EndUser/ShippingQuote`، سپس `POST /api/EndUser/ShippingSelection`؛ روش ثابت در `Cart` با `GetDelivery` | `Cart.deliveryPrice` و `Cart.paymentPrice`؛ در ReceiverPays هزینهٔ ارسال درگاه صفر است |
| سفر پت‌رسان | جدول زمانی `PriceCalculation` + مسافت + توقف/گزینه/پت اضافه/وانت | `POST /api/EndUser/PriceCalculation` | `Trip.price`، پس از کد تخفیف `Trip.paymentPrice` |
| سرویس هفتگی پت‌رسان | مجموع قیمت نوبت‌ها با نرخ ساعت هر نوبت | `POST /api/EndUser/PetResanService/preview-price` | هر `Trip` تولیدی هنگام ایجاد جداگانه قیمت‌گذاری می‌شود |
| پانسیون روزانه و مهد ساعتی | `Pansion.pansionPrice` یا `Pansion.schoolPrice` | `GET /api/Pansion/{id}`؛ رزرو و جزئیات با `POST/GET /api/EndUser/PansionReserve` | `PansionReserve.paymentPrice` |
| دورهٔ آموزشی مدرسه | `SchoolCourse.price` | `GET /api/SchoolCourse/{id}`؛ رزرو با `POST /api/EndUser/SchoolReserve` | `SchoolReserve.paymentPrice` |
| خدمت نماینده/کلینیک | قیمت پکیج برای نحوهٔ ارائه، تعداد پت و انتخاب آنلاین | `GET /api/EndUser/CompanionAssistancePackage`، رزرو `POST/GET /api/EndUser/CompanionReserve` | ابتدا `prePaymentPrice`؛ هزینهٔ نهایی اپراتوری می‌تواند جداگانه تعیین شود |
| مشاورهٔ آنلاین | `ConsultationPackage.price` | `GET /api/EndUser/ConsultationPackage?companionId=...`؛ خرید `POST /api/EndUser/ConsultationPurchase` | `ConsultationPurchase.paymentPrice` و سهم کیف/درگاه |
| بیمه | `CompanionInsurancePackage.price` | `GET /api/CompanionInsurancePackage`؛ فروش/جزئیات در `api/EndUser/CompanionInsurancePackageSale` | `CompanionInsurancePackageSale.paymentPrice` |
| PastilAI | `PastilAiPlan.price` | `GET /api/EndUser/PastilAI/plans`؛ خرید `POST /api/EndUser/PastilAI/purchase` | `plan.price - rebatePrice`، سپس کیف پول/درگاه |
| کارگو | هنگام ایجاد `price=paymentPrice=0` | `GET /api/EndUser/Cargo/{id}`؛ وضعیت/جزئیات ادمین | پس از ثبت قیمت واقعی، `paymentPrice = price - rebatePrice`؛ مسیر ثبت قیمت در کنترلرهای بررسی‌شده یافت نشد |
| درخواست محصولِ ناموجود در کاتالوگ | `MissingProduct.price` فقط پیشنهاد فروشنده برای آیتم فروشگاه | `GET /api/Seller/MissingProduct`، `GET /api/Admin/MissingProduct/{id}` | پس از تأیید ادمین، قیمت مؤثر `ProductItem.basePrice` می‌شود؛ مبلغ سفارش نیست |

قیمت‌های مالی **برای هر دامنه مستقل‌اند**. مثلاً `totalPrice` نمایش رزرو پانسیون/مدرسه/خدمت ممکن است قیمت سفر پت‌رسان متصل را هم جمع کند، ولی پرداخت سفر و پرداخت رزرو دو جریان مستقل دارند. برای دریافت وجه، شناسه و مبلغ هر پرداخت را جداگانه از سرور بگیرید.

### مسئول تنظیم نرخ‌ها در پنل‌ها

| نرخ | پنل و مسیر نوشتن/مدیریت |
|---|---|
| قیمت پایه و موجودی SKU | فروشنده: `GET/POST /api/Seller/ProductItem`؛ تخفیف فروشنده: `GET/POST/PUT/DELETE /api/Seller/Discount`. |
| روش و نرخ ثابت ارسال | فروشنده: `api/Seller/Delivery`؛ مدیریت سراسری نیز در `api/Admin/Delivery` و تعرفه‌های فاصله در `api/Admin/DeliveryDistance` وجود دارد. Quote زنده توسط ارائه‌دهنده در زمان استعلام تولید می‌شود. |
| نرخ ساعتی سفر، توقف، گزینهٔ سفر | ادمین: `api/Admin/PriceCalculation`، `api/Admin/TripStop`، `api/Admin/TripOption`. |
| نرخ روزانه/ساعتی پانسیون و مهد | نماینده: `api/Companion/Pansion`؛ ادمین: `api/Admin/Pansion`. |
| قیمت دورهٔ آموزشی | نماینده: `api/Companion/SchoolCourse`؛ ادمین: `api/Admin/SchoolCourse`. |
| قیمت خدمت و نحوهٔ ارائه | نماینده: `api/Companion/CompanionAssistancePackage` و ردیف‌های نوع پکیج؛ ادمین نیز مسیر متناظر دارد. |
| قیمت مشاوره و بیمه | نماینده: `api/Companion/ConsultationPackage` و `api/Companion/CompanionInsurancePackage`؛ ادمین نیز مسیر متناظر دارد. |
| قیمت پلن PastilAI | ادمین: `api/Admin/PastilAI`؛ کاربر قیمت پلن قابل‌خرید را از `api/EndUser/PastilAI/plans` می‌خواند. |

## ۲. فرهنگ فیلدهای مالی و قاعدهٔ فرانت

| نام رایج | معنی در جای استفاده |
|---|---|
| `basePrice` | قیمت پایهٔ SKU یا جمع پایه پیش از تخفیف فروشنده. |
| `price` | بسته به موجودیت: نرخ تعریف‌شدهٔ خدمت/دوره، قیمت مؤثر SKU، یا قیمت اولیهٔ رزرو/سفر. نام یکسان به معنی فرمول یکسان نیست. |
| `discountPercent`, `discount`, `discountPrice` | تخفیف کالای فروشنده که در قیمت مؤثر SKU لحاظ شده؛ دوباره از `price` کم نشود. |
| `rebatePrice` | مبلغ کد تخفیف/کوپن مستقل از تخفیف SKU. در اکثر رزروها `paymentPrice = max(0, price - rebatePrice)`؛ خدمت نماینده در پیش‌پرداخت از `prePaymentPrice` استفاده می‌کند. |
| `deliveryPrice` | هزینهٔ ارسال که اکنون در پرداخت سفارش منظور شده؛ در ReceiverPays می‌تواند صفر باشد، در حالی که `quotedPrice` مثبت است. |
| `clubDeliveryDiscount` | کاهش هزینهٔ ارسال توسط مزیت باشگاه، حداکثر تا هزینهٔ ارسال و سقف همان مزیت. |
| `paymentPrice` | مبلغ پس از تخفیف در خودِ جریان خرید/رزرو، معمولاً **پیش از کسر کیف پول**. در سطح `CartStore` شامل قیمت کالا + ارسال همان فروشگاه است و لزوماً برابر مبلغ نهایی `Cart` نیست. |
| `walletPrice` / `walletAmount` | سهم انتخاب‌شده از کیف پول؛ نباید مجدداً از `paymentPrice` برای نمایش قیمت محصول کم شود، فقط برای نمایش شیوهٔ تسویه. |
| `Payment.amount` | مبلغی که واقعاً به درگاه فرستاده می‌شود، معمولاً `paymentPrice - walletPrice`؛ در تسویهٔ تماماً داخلی می‌تواند صفر باشد. |
| `grossAmount`, `rebateAmount`, `walletAmount` در Payment | عکس مالی تلاش پرداخت؛ سرور در شروع/تأیید پرداخت با رکورد مرجع تطبیق می‌دهد. |
| `siteShare`, `storeShare`, `companionShare`, `driverShare` | تقسیم درآمد پس از پرداخت/تسویه؛ هزینهٔ اضافه برای مشتری نیست. |

قاعدهٔ عملی: **نمایش قیمت فعلی از پاسخ مدل همان صفحه، پرداخت از پاسخ سرورِ شروع پرداخت، و تأیید موفقیت از callback/وضعیت سرور**. قیمت را صرفاً با محاسبهٔ فرانت یا با مقدار ارسال‌شده در بدنهٔ create قطعی ندانید. کاربر ممکن است بین پیش‌نمایش و پرداخت با تغییر موجودی، نرخ، کوپن یا Quote روبه‌رو شود.

### واحد پول؛ یک ناسازگاری مهم

کد همهٔ مدل‌ها واحد پول را یک‌دست و صریح در DTO اعلام نمی‌کند. در محاسبهٔ سفر، ثابت پایه در توضیح کد «تومان» است؛ در ارسال زنده، ارائه‌دهندهٔ میاره قیمت را به **تومان** می‌دهد و همان عدد بدون ×۱۰ در `ShippingQuote.price` می‌ماند، اما `ShippingQuoteVDto.currency` مقدار **`IRR`** می‌دهد. بنابراین `currency=IRR` در Quote فعلی برای تصمیم تبدیل عدد کافی نیست. تا اصلاح قرارداد، در UI واحد را مطابق قرارداد عملی هر endpoint و عدد برگشتی سرور نمایش دهید و **تبدیل ×۱۰ یا ÷۱۰ را خودکار و سراسری اعمال نکنید**. این مورد باید در یکپارچه‌سازی مالی محیط مستقر هم سنجیده شود.

در Shipment نیز `quotedPrice` و `chargedPrice` مبلغ برآورد/دریافت از مشتری‌اند، اما `providerCost` هزینهٔ واقعی ارائه‌دهنده است. در webhook تحویل میاره، `delivery_cost` برای `providerCost` در ۱۰ ضرب می‌شود، در حالی که مسیر Quote این تبدیل را ندارد؛ این دو فیلد را هم‌واحد و قابل تفریق مستقیم فرض نکنید تا قرارداد واحد پول اصلاح و آزموده شود.

## ۳. فروشگاه: قیمت SKU، تخفیف، سبد و سفارش

### ۳.۱ تعیین قیمت کالا

فروشنده برای محصول موجود در کاتالوگ، با `GET /api/Seller/ProductItem/id?id={productId}` سطرهای فروش/تنوع فروشگاه را می‌گیرد و با `POST /api/Seller/ProductItem` `productId` و `productItems[]` شامل `basePrice`, `quantity`, `active` و شناسه‌های تنوع را می‌فرستد. `storeId` در کنترلر از توکن فروشنده جایگزین می‌شود. در سرویس، روی آیتم موجود `basePrice` و موقتاً `price` با مقدار ورودی تنظیم می‌شود؛ آیتم تازه فقط با قیمت پایهٔ مثبت ساخته می‌شود (موجودی صفر مجاز است). پس از ذخیره، `UpdateProductPriceAsync` اجرای stored procedure `UpdateProductItemDiscount` را برای بازسازی قیمت مؤثر درخواست می‌کند.

فروشنده با `POST/PUT/DELETE /api/Seller/Discount` تخفیف درصدی را در دامنهٔ فروشگاه، دسته، برند، محصول یا SKU ایجاد/فعال/حذف می‌کند. گروه تخفیف از `GET /api/Seller/DiscountGroup` فقط برای انتخاب کمپین خوانده می‌شود. تخفیف مؤثر و قیمت نهایی در `ProductItem.price`, `discountPercent`, `discountGroupId`, `discountEndDate` برمی‌گردد؛ `Product.price` در سطح محصول نیز نمایشی است. **قیمت قطعی SKU دارای تنوع `ProductItem.price` است.** منطق اولویت تخفیف‌های هم‌پوشان داخل stored procedure است و متن SQL آن در مخزن نیست؛ فرانت نباید فرض کند درصدها جمع می‌شوند یا همیشه بزرگ‌ترین درصد انتخاب می‌شود. پس از تغییر قیمت یا تخفیف، آیتم را دوباره بخوانید.

### ۳.۲ سبد؛ فرمول واقعی

`POST /api/EndUser/Cart` با `cartUpdateType` عملیات `GetCart`, `Add`, `Delete`, `SetAddress`, `GetDelivery`, `SetDelivery`, `SetRebate`, `RemoveRebate`, `SetMerchant`, `SetOrder` و ... را انجام می‌دهد. برای صرفاً خواندن سبد، `cartUpdateType=14` (`GetCart`) در enum فعلی است. پاسخ `CartVDto` مبنای صفحهٔ تسویه است. سرویس روی **فروشگاه فعال سبد** قیمت‌ها را دوباره از ProductItem محاسبه می‌کند:

```text
CartStore.basePrice = Σ(ProductItem.basePrice × count)
CartStore.price     = Σ(ProductItem.price × count)
CartStore.discount  = Σ((ProductItem.basePrice - ProductItem.price) × count)
CartStore.paymentPrice = CartStore.price + CartStore.deliveryPrice

Cart.price = CartStore.price
Cart.paymentPrice = max(0, Cart.price - Cart.rebatePrice
                           + Cart.deliveryPrice - Cart.clubDeliveryDiscount)
```

سرور آیتمِ `systemActive=false` را حذف و تعداد بیش از موجودی را به موجودی کاهش می‌دهد. پس جمع فرانت از آیتم‌های قبلی می‌تواند با پاسخ تازه فرق کند. تخفیف کالا از قبل در `ProductItem.price` است؛ کوپن در `rebatePrice` جداگانه کم می‌شود. تخفیف ارسال باشگاه فقط برای کاربر و مزیت معتبر، تا سقف `min(deliveryPrice, maximumDeliveryAmount)` اعمال می‌شود. `CartStore.paymentPrice` کوپن و مزیت باشگاه را کم نمی‌کند؛ برای دکمهٔ پرداخت **`Cart.paymentPrice`** را نمایش دهید.

در `SetOrder` همین مقادیر به ProductOrder و سطرهای آن snapshot می‌شوند؛ تلاش پرداخت از `ProductOrder.paymentPrice` ساخته می‌شود. تغییر قیمت کاتالوگ پس از سفارش، snapshot سفارش گذشته را به‌تنهایی عوض نمی‌کند. اگر ادمین/فروشنده آیتم سفارش را بعداً تعدیل کند، `ProductOrderAdjustmentRules` جمع سطرهای باقی‌مانده را بازسازی، کوپن قبلی را متناسب با نسبت قیمت جدید/قدیم (با گردکردن و سقف قیمت جدید) تعدیل و اختلاف مبلغ را برای بازپرداخت حساب می‌کند.

### ۳.۳ ارسال: ثابت، زنده، گیرنده‌پرداخت، اکسپرس

برای ارسال ثابت، `Cart.GetDelivery` روش‌های مجاز و `deliveryPrice` را می‌دهد. منطق ثابت ابتدا `Delivery.basePrice` است؛ اگر **هم** حداقل قیمت خرید **و هم** حداقل تعداد مقرر برقرار باشند، رایگان می‌شود؛ در غیر این صورت اگر بازهٔ فاصلهٔ معتبر تعریف شده باشد، قیمت همان بازه گرفته می‌شود. **نکتهٔ اجرایی:** شرط تعداد از `cart.itemCount` استفاده می‌کند، اما `UpdateCartAsync` فعلاً این فیلد را با تعداد `CartStores` تنظیم می‌کند، نه مجموع تعداد کالا؛ بنابراین شرط «حداقل تعداد برای ارسال رایگان» ممکن است با انتظار محصولیِ UI فرق کند. `Delivery.afterRent` می‌تواند به معنی پرداخت بعدی نزد گیرنده باشد.

برای Quote زنده، بعد از انتخاب آدرس و فروشگاه فعال، `POST /api/EndUser/ShippingQuote` با `{"storeId":12}` لیستی از `quoteToken`, `quotedPrice`, `payableDeliveryPrice`, `paymentMode`, `payAtDestination`, `currency`, `expiresAtUtc`, `requiresSlot`, `isExpress`, `expressSurcharge` می‌دهد. Quote فقط برای سبد/آدرس جاری و تا انقضا معتبر است. با `POST /api/EndUser/ShippingSelection` و `quoteToken` (و برای میارهٔ عادی `slotId`, `slotDate`) انتخاب می‌شود. در `Prepaid`، `CartStore.deliveryPrice=quote.price`؛ در `ReceiverPays` صفر است و مبلغ Quote جداگانه باید به‌عنوان پرداخت در مقصد نمایش داده شود. اکسپرس میاره فقط پیش‌پرداخت است و `quotedPrice` شامل اضافهٔ اکسپرس است؛ آن اضافه را دوباره جمع نکنید. تغییر آدرس/محتوای سبد یا پایان Quote، استعلام تازه می‌خواهد.

## ۴. پت‌رسان: نرخ سفر، پیش‌نمایش، سرویس هفتگی

### ۴.۱ تعرفه و فرمول سفر

ادمین ردیف‌های ساعتی را با `GET/POST/PUT/DELETE /api/Admin/PriceCalculation` مدیریت می‌کند. ورودی نرخ: `fromTime`, `toTime`, `price` (نرخ در متر مسافت)، `pickupVehicleExtraPrice` و ستون‌های `stopPrice`, `extraPetPrice`. **دو ستون آخر در فرمول فعلی استفاده نمی‌شوند.** گزینه‌ها از `GET /api/EndUser/TripOption` و توقف‌ها از `GET /api/EndUser/TripStop` دریافت می‌شوند.

`POST /api/EndUser/PriceCalculation` بدنهٔ `TripDto` می‌گیرد. فیلدهای اثرگذار: `origin`, `destination`, `secondDestination` (اگر معتبر باشد)، `tripStartDateTime`, `roundTrip`, `tripStopId`, `stopMinutes`, `tripOptionIds`, `userPetIds` یا `userPetId`, `vehicleTypeId`. پاسخ `BaseResultDto<double>` با قیمت در `data` است. نرخ ساعتی از ساعت **حرکت** (`tripStartDateTime`) یا در نبود آن از ساعت جاری انتخاب می‌شود. اگر ردیف نرخ یافت نشود، تابع قیمت `0` برمی‌گرداند؛ صفر را قیمت معتبر سفر معرفی نکنید.

```text
مسافت = مسیر مبدأ→مقصد + مسیر مقصد→مقصد دوم (اگر باشد)
هزینهٔ متغیر = مسافتِ متر × PriceCalculation.price
             + TripStop.price انتخابی
             + ceil(stopMinutes / 5) × قیمت نخستین TripStop فعال
             + Σ(TripOption.price)
             + max(0, تعداد پت یکتا - 1) × (100000 + Σ(TripOption.price)/2)
             + PickupVehicleExtraPrice (فقط اگر نوع خودروی انتخابی «وانت» باشد)
کل یک‌طرفه = 500000 + هزینهٔ متغیر
کل رفت‌وبرگشت = کل یک‌طرفه × 1.5
```

ارقام ثابت بالا مطابق توضیح کد به تومان‌اند. `roundTrip` مسافت را دو برابر نمی‌کند؛ نصف **کل هزینه با مبلغ پایه و افزوده‌ها** را اضافه می‌کند. `StopPrice` جدول نرخ و `ExtraPetPrice` آن در این نسخه نادیده گرفته می‌شوند. هنگام ثبت سفر، سرویس `Trip.price` را خودش محاسبه و `paymentPrice` را ابتدا برابر آن می‌گذارد؛ مبلغ فرانت مرجع پرداخت نیست. کد تخفیف روی `paymentPrice` اثر می‌گذارد؛ کیف پول سهم پرداخت را تغییر می‌دهد. سفر متصل به پانسیون/مدرسه/خدمت، پرداختی مستقل دارد.

### ۴.۲ سرویس تکرارشوندهٔ هفتگی

`POST /api/EndUser/PetResanService/preview-price` با `PetResanServiceCreateDto` **جمع یک هفته** را در `data` می‌دهد. هر ردیف یکتای `(weekDayId,time)` محاسبه می‌شود. ردیف دارای `returnTime`، رفت و برگشت را به‌صورت **دو سفر یک‌طرفه با ساعت‌های متفاوت** جمع می‌کند؛ ردیف بدون `returnTime` از `roundTrip` استفاده می‌کند. بدون هیچ schedule، یک پیش‌نمایش با نرخ همین لحظه برمی‌گردد. در `POST /api/EndUser/PetResanService`، `pricePerOccurrence` میانگین جمع هفتگی بر تعداد ردیف‌های یکتا ذخیره می‌شود؛ `pricePerWeek` نمایشی از آن میانگین × تعداد schedule فعال است. **قیمت قطعی و کسر کیف پول هر Trip تولیدشده دوباره در زمان ایجاد همان Trip حساب می‌شود.**

محدودیت قابل توجه کد فعلی: DTO واسط قیمت‌گذاری در پیش‌نمایش سرویس و هنگام تولید Trip، `userPetIds` را به `CalculateTripPrice` نمی‌فرستد؛ بنابراین هزینهٔ «پت اضافه» که در محاسبهٔ سفر عادی وجود دارد در این دو مسیر سرویس هفتگی وارد فرمول نمی‌شود. این یک تفاوت اجرایی است، نه قاعدهٔ قابل‌تعمیم به سفر عادی.

## ۵. اقامت، آموزش و خدمات نماینده

| جریان | منبع و فرمول ثبت | اثر کوپن و کیف پول | نکتهٔ نمایش |
|---|---|---|---|
| پانسیون روزانه | `Pansion.pansionPrice × ((toDate-fromDate).Days+1)`؛ هر دو روز ابتدا و انتها حساب می‌شوند. | `paymentPrice=max(0, price-rebatePrice)`؛ انتخاب کیف در رزرو ثبت و هنگام شروع پرداخت سهم واقعی محاسبه می‌شود. | در `GET /api/Pansion/{id}` نرخ واحد و در `GET /api/EndUser/PansionReserve/{id}` مبلغ snapshot رزرو را ببینید. |
| مهد ساعتی در پانسیون | `Pansion.schoolPrice × ceil((endTime-startTime).TotalHours)` | مانند پانسیون. | تاریخ و `startTime/endTime` لازم‌اند؛ کسری ساعت رو به بالا گرد می‌شود. |
| دورهٔ مدرسه | `SchoolCourse.price` یک‌بار برای ثبت‌نام یک پت در دوره | کوپن از `paymentPrice` کم می‌شود؛ کیف پول در مرحلهٔ پرداخت اعمال می‌شود. | retry ثبت‌نام پرداخت‌نشده، قیمت را از نرخ فعلی دوره از نو آغاز و کوپن/کیف قبلی را پاک می‌کند. |
| خدمت نماینده | برای هر پکیج نرخ حالت ارائهٔ انتخابی (`CompanionAssistancePackageType.price/prePaymentPrice`)؛ در پکیج قدیمی نرخ خود پکیج؛ مجموع × تعداد پت. انتخاب آنلاین قدیمی ممکن است افزودهٔ مستقل داشته باشد. | کوپن روی `prePaymentPrice` اعمال می‌شود؛ پیش‌پرداخت پرداخت می‌گردد. | `packagePrice` قیمت کل بسته و `prePaymentPrice` مبلغ پیش‌پرداخت هستند؛ اینها را یکی نگیرید. |
| هزینهٔ نهایی اپراتوری خدمت | `operatorFinalPrice = operatorStuffPrice + operatorWagesPrice` در `PUT /api/Operator/CompanionReserveOperatorUpdate` | بدهی پایان خدمت می‌تواند بر همین مبلغ ساخته شود. | `paymentPrice` رزرو پس از ثبت هزینهٔ نهایی به این مقدار تغییر می‌کند؛ با پیش‌پرداخت اولیه یکی نیست. |

برای خدمت نماینده، ردیف قیمتِ نحوهٔ ارائه اولویت دارد. اگر پکیج اصلاً ردیف نحوهٔ ارائه ندارد، `price/prePaymentPrice` پایه استفاده می‌شود؛ اگر ردیف‌ها دارد ولی حالت انتخابی ارائه نمی‌شود، رزرو رد می‌شود. برای حالت آنلاین، مبلغ انتخاب آنلاین فقط در مسیر پکیج قدیمیِ فاقد ردیف حالت ممکن است جداگانه افزوده شود. رزرو چندخدمتی `TotalPrePaymentPrice` و `TotalPackagePrice` را از جمع اعضا تولید می‌کند و پرداخت گروهی جدا دارد.

`totalPrice` در DTO جزئیات رزرو پانسیون/مدرسه/خدمت، در صورت وجود سفر متصل، برای **نمایش** `reserve.paymentPrice + trip.paymentPrice/price` است. این فیلد دستور پرداخت یکپارچه نیست. در UI دو صورت‌حساب و دو وضعیت پرداخت را نگه دارید.

## ۶. مشاوره، بیمه، PastilAI و کارگو

- **مشاورهٔ آنلاین:** `GET /api/EndUser/ConsultationPackage?companionId=...` بسته‌های فعالِ دارای قیمت مثبت را می‌دهد. در `POST /api/EndUser/ConsultationPurchase`، قیمت پکیج snapshot می‌شود. قاعدهٔ قطعی: `rebate=min(max(0, rebatePrice), price)`، `paymentPrice=max(0, price-rebate)`، `wallet=GetWalletContribution(balance,paymentPrice)`، `gateway=max(0,paymentPrice-wallet)`. درصد کمیسیون بسته روی مبلغ پس از تخفیف اعمال می‌شود. بازپرداخت لغو، مبلغ واقعاً پرداخت‌شده بعد از تخفیف است؛ خود تخفیف بازپرداخت نمی‌شود.
- **بیمه:** نرخ از `GET /api/CompanionInsurancePackage` و `CompanionInsurancePackage.price` می‌آید؛ هنگام ایجاد فروش برای کاربر snapshot می‌شود. `rebatePrice` جداگانه کم و `paymentPrice` برای پرداخت استفاده می‌شود. APIهای مستقلِ set/remove rebate و set wallet برای فروش بیمه وجود دارند. قیمت پکیج روز بعد، فروش قبلی را بازقیمت‌گذاری نمی‌کند.
- **PastilAI:** `GET /api/EndUser/PastilAI/plans` قیمت پلن را می‌دهد. خرید با `POST /api/EndUser/PastilAI/purchase` و `planId`, `rebateCode?`, `fromWallet`, `merchantId?` انجام می‌شود. پلن رایگان از خرید پولی جداست؛ پلن پولی باید فعال، قابل‌خرید و حداقل `10000` باشد. کوپن حداکثر تا قیمت پلن کم می‌شود؛ `priceSnapshot` مبلغ پس از تخفیف را نگه می‌دارد؛ کیف پول و باقیماندهٔ درگاه توسط سرور حساب می‌شود.
- **کارگو:** `POST /api/EndUser/Cargo` هنگام ایجاد، ورودی قیمت کاربر را کنار می‌گذارد و `price`, `defaultPrice`, `paymentPrice`, `rebatePrice`, `walletPrice` را صفر می‌کند. اعمال کد تخفیف تا وقتی `price=0` است رد می‌شود. در کنترلرهای ادمین بررسی‌شده، فقط فهرست/جزئیات و تغییر وضعیت (`PUT /api/Admin/UpdateCargoStatus`) دیده می‌شود و endpoint ثبت قیمت نهایی کارگو یافت نشد؛ بنابراین نباید برای کارگو «قیمت قطعی» یا دکمهٔ پرداخت را فقط بر پایهٔ درخواست اولیه نمایش داد. این شکاف قرارداد باید با بک‌اند روشن/تکمیل شود.
- **MissingProduct:** قیمت فروشنده در درخواست افزودن محصول فقط پیشنهاد است. ادمین می‌تواند هنگام approve قیمت و موجودی را override کند؛ ProductItem فقط اگر قیمت مؤثر `>0` باشد ساخته می‌شود. این قیمت به خودی خود بدهی یا پرداخت نیست.

## ۷. کد تخفیف، کیف پول و شروع پرداخت

**کد تخفیف (`Rebate`)** از تخفیف درصدی کالا (`Discount`) جداست. سرویس Rebate اعتبار نوع جریان، تاریخ، سقف استفادهٔ کل و هر کاربر، کاربر مجاز، حداقل مبلغ و هدف محصول/باشگاه را می‌سنجد. تخفیف ثابت `priceValue` است؛ تخفیف درصدی در کد با `Math.Round(basePrice × priceValue/100)` حساب می‌شود؛ کوپن باشگاه ممکن است سقف مزیت داشته باشد. در سبد، کوپن وابسته به یک محصول فقط روی جمع همان محصول محاسبه می‌شود. مبلغ نهایی را از `rebatePrice` و `paymentPrice` **پاسخ همان رزرو/سبد** بخوانید. برای سفر/پانسیون/مدرسه/خدمت/بیمه/کارگو، endpointهای مستقل `SetRebate` یا `RemoveRebate` وجود دارند؛ تغییر کوپن بعد از شروع پرداخت موفق مجاز نیست و guard پرداختِ قبلی اعمال می‌شود.

**کیف پول:** `GET /api/EndUser/WalletAmount` موجودی دفتر کیف را می‌دهد، اما `GetSpendableAmountValueAsync` در بعضی جریان‌ها اعتبار تشویقی واجد شرایطِ هدف را نیز به موجودی نقدی اضافه می‌کند؛ بنابراین این GET لزوماً سهم قابل‌مصرف برای یک خرید مشخص نیست. سهم واقعی را از `walletPrice` و نتیجهٔ شروع پرداخت بگیرید. `PaymentAmountHelper` چنین رفتار می‌کند:

```text
اگر موجودی >= مبلغ پرداختی: سهم کیف = تمام مبلغ و سهم درگاه = ۰
اگر موجودی < مبلغ پرداختی: سهم کیف <= مبلغ پرداختی - ۱۰۰۰۰
پس اگر درگاه لازم باشد، حداقل ۱۰۰۰۰ برای آن باقی می‌ماند.
```

شروع پرداخت `Payment.amount` را به مبلغ درگاه و `grossAmount/rebateAmount/walletAmount` را به snapshot مرجع وصل می‌کند. صفرشدن مبلغ درگاه می‌تواند تسویهٔ داخلی با کیف/تخفیف باشد و نباید به‌صورت «پرداخت درگاه صفر تومان» نمایش داده شود. سرور قبل از اعمال callback، snapshot مبلغ را دوباره با سفارش/رزرو مرتبط تطبیق می‌دهد. شارژ کیف پول (`PaymentType_Wallet`) حداقل `10000` دارد. برای retry پرداخت، ممکن است رزرو/قیمت دوباره از وضعیت فعلی ساخته شود؛ از لینک درگاه یا مبلغ کهنه استفاده نکنید.

## ۸. تقسیم درآمد؛ جدا از قیمت مشتری

- سفارش کالا: برای هر `ProductOrderStore`، درصد `Store.commissionPercent` روی `store.paymentPrice` محاسبه می‌شود؛ سهم سایت = مبلغ × درصد و بقیه سهم فروشگاه. در محاسبهٔ فعلی `store.paymentPrice` شامل هزینهٔ ارسال پیش‌پرداخت نیز هست؛ این نکته برای تسویه مهم است.
- رزرو پانسیون/مهد: سهم سایت از `PansionReserve.paymentPrice` با `dailyCommissionPercent` یا `hourlyCommissionPercent` متناسب با نوع رزرو؛ باقی‌مانده سهم نماینده.
- دورهٔ مدرسه: سهم سایت از `SchoolReserve.paymentPrice × SchoolCourse.commissionPercent/100`؛ باقی‌مانده نماینده.
- خدمت نماینده: سهم سایت از `CompanionReserve.paymentPrice × CompanionAssistance.commissionPercent/100`؛ باقی‌مانده نماینده. بدهی اپراتوری جریان جداگانه دارد.
- سفر: `SiteShare = round((paymentPrice>0 ? paymentPrice : price) × Driver.commissionPercent/100)`؛ باقی‌مانده سهم راننده. نبود درصد تنظیم‌شده سهم سایت را صفر می‌کند.
- مشاوره: سهم سایت از مبلغ پس از تخفیف با درصد بسته، باقی‌مانده سهم نماینده است.

این فیلدها را به جمع قابل پرداخت کاربر اضافه نکنید.

## ۹. راهنمای نمایش و عیب‌یابی

| نشانه | بررسی درست |
|---|---|
| قیمت کالا در لیست و سبد فرق دارد | SKU/تنوع و فروشگاه را تطبیق دهید؛ پس از تغییر تخفیف، `ProductItem.price` و `Cart` را refresh کنید. فرمول procedure تخفیف در مخزن نیست. |
| کوپن ظاهراً دو بار کم شده | `ProductItem.price` از قبل تخفیف کالاست؛ `rebatePrice` فقط کد تخفیف سبد/رزرو است. از `paymentPrice` بار دیگر تخفیف کم نکنید. |
| Quote ارسال با جمع سبد فرق دارد | `ReceiverPays` هزینهٔ درگاه صفر ولی `quotedPrice` مثبت دارد. برای `Prepaid`، پس از انتخاب Quote سبد را refresh کنید؛ اکسپرس در Quote گنجانده شده است. |
| رقم ارسال ده برابر/یک‌دهم دیده می‌شود | مسیر میاره عدد تومان را در Quote با `currency=IRR` برمی‌گرداند. تبدیل خودکار از روی currency فعلی قابل اتکا نیست. |
| قیمت سفر صفر است | پوشش ساعت حرکت در `PriceCalculation`، مبدأ/مقصد و خطای سرویس مسافت را بررسی کنید؛ `isSuccess` و `data>0` را هر دو بسنجید. |
| قیمت هفتگی با جمع سفرهای واقعی فرق دارد | پیش‌نمایش سرویس یک محاسبهٔ هفتهٔ نمونه است؛ Trip در ساعت واقعی دوباره قیمت می‌گیرد. علاوه بر آن، `pricePerOccurrence` میانگین نمایشی است. |
| جمع رزرو با درگاه برابر نیست | `totalPrice` ممکن است سفر متصل را فقط نمایشی جمع کند؛ `walletPrice` هم سهم تسویه را تغییر می‌دهد. مرجع، رکورد پرداخت همان نوع است. |
| قیمت کارگو صفر است | در create عمداً صفر می‌شود و endpoint روشن برای ثبت قیمت نهایی در کنترلرهای موجود دیده نشد. |
| پرداخت با مبلغ قدیمی رد می‌شود | سرور snapshot پرداخت را با مبلغ و تخفیف/کیف رکورد فعلی تطبیق می‌دهد؛ فهرست/جزئیات و شروع پرداخت را تازه بگیرید. |

## ۱۰. منابع کد و حد قطعیت

- کالا، تخفیف و سبد: `Application/Services/ProductSrvs/ProductItemSrv/ProductItemService.cs`، `ProductSrv/ProductService.cs`، `DiscountSrv/DiscountService.cs`، `Application/Services/Order/CartSrv/CartService.cs`، `ProductOrderSrv/ProductOrderAdjustmentRules.cs`.
- ارسال: `Application/Services/Order/DeliverySrv/DeliveryService.cs`، `ShippingSrv/ShippingQuoteService.cs`، `ShippingSrv/Provider/MiareShippingProvider.cs`.
- سفر/سرویس: `Application/Services/TripSrv/PriceCalculationSrv/PriceCalculationService.cs`، `TripSrv/TripService.cs`، `PetResanServiceSrv/PetResanServiceService.cs`.
- رزروها: `Application/Services/PansionSrvs/PansionReserveSrv/PansionReserveService.cs`، `SchoolSrvs/SchoolReserveSrv/SchoolReserveService.cs`، `CompanionSrvs/CompanionReserveSrv/CompanionReserveService.cs`، `CompanionSrvs/CompanionAssistancePackageSrv/CompanionPackagePricing.cs`.
- پرداخت: `Application/Services/Order/RebateSrv/RebateService.cs`، `Order/PaymentSrv/PaymentService.cs`، `Order/PaymentSrv/PaymentAmountHelper.cs`، `ProductSrvs/WalletSrv/WalletService.cs`.
- سایر خریدها: `ConsultationSrvs/ConsultationPurchaseSrv/ConsultationPurchaseRules.cs`، `PastilAISrv/PastilAiPlanService.cs`، `CompanionSrvs/CompanionInsurancePackageSaleSrv/CompanionInsurancePackageSaleService.cs`، `Content/CargoSrv/CargoService.cs`.

محدودیت شفاف: تعریف SQL stored procedure `UpdateProductItemDiscount` و وضعیت واقعی دیتابیس/تنظیمات ارائه‌دهندگان در این workspace نیست. بنابراین اولویت دقیق تخفیف‌های هم‌پوشان و نرخ زندهٔ بیرونی فقط با API محیط مستقر قابل تأیید است؛ در سند به‌جای ساختن فرمول فرضی، فیلد خروجیِ مرجع مشخص شده است.
