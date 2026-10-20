namespace Application.Services.Order.ShippingSrv
{
    public class ShippingOptions
    {
        public const string SectionName = "Shipping";

        public bool TestMode { get; set; } = true;
        public int QuoteTtlMinutes { get; set; } = 5;
        public int DefaultWeightGrams { get; set; } = 1000;
        public decimal DefaultLengthCm { get; set; } = 20;
        public decimal DefaultWidthCm { get; set; } = 20;
        public decimal DefaultHeightCm { get; set; } = 20;
        // فقط میاره در ارسال فروشگاه نمایش داده شود (روش‌های دیگر هر فروشگاه در استعلام نادیده گرفته می‌شوند)
        public bool MiareOnly { get; set; }
        // بازه‌های تحویل: از فردا چند روز قابل انتخاب است (تحویل همان روز نداریم)
        public int SlotHorizonDays { get; set; } = 3;
        // فاصله‌ی اطمینان بعد از زمان آماده‌سازی فروشگاه (دقیقه): تأیید فروشنده، رسیدن پیک و ... (بازه فقط وقتی نمایش داده می‌شود
        // که تا الان + آماده‌سازی فروشگاه + این فاصله، هنوز بتوان کالا را قبل از «پایان بازه − MinDeliveryMinutes» به پیک داد)
        public int PrepBufferMinutes { get; set; } = 30;
        // مهلت تأیید آماده‌سازی توسط فروشنده بعد از پرداخت (دقیقه)
        public int SellerConfirmMinutes { get; set; } = 60;
        // هشدار به ادمین پاستیل: اگر فروشنده این‌قدر دقیقه بعد از پرداخت هنوز سفارش را تأیید/آماده نکرده (مرسوله هنوز «منتظر فروشنده»)،
        // یک‌بار پوش «فروشنده X هنوز سفارش Y را آماده نکرده» به ادمین‌ها می‌رود تا پیگیری کنند. صفر = غیرفعال.
        public int SellerNotReadyAdminAlertMinutes { get; set; } = 30;
        // حداقل زمان لازم برای رساندن کالا بعد از تحویل به پیک تا پایان بازه (دقیقه)
        public int MinDeliveryMinutes { get; set; } = 60;
        // ساعت پیش‌فرض تحویل به پیک: چند دقیقه قبل از شروع بازه
        public int PickupLeadMinutes { get; set; } = 30;
        // ارسال اکسپرس (میاره + Boost). طراحی: backend/Docs/MIARE_EXPRESS_SHIPPING_FA.md
        public ShippingExpressOptions Express { get; set; } = new();
        public ShippingProviderOptions AloPeyk { get; set; } = new();
        public ShippingProviderOptions Tipax { get; set; } = new();
        public ShippingProviderOptions SnappBox { get; set; } = new();
        // میاره فقط درون‌شهری/نزدیک است و area_coverage آن مقصد را نمی‌سنجد؛ پس فاصله‌ی فروشگاه تا آدرس مشتری اینجا محدود می‌شود (کیلومتر هوایی)
        public ShippingProviderOptions Miare { get; set; } = new() { MaxDistanceKm = 45 };
    }

    public class ShippingExpressOptions
    {
        /// <summary>کلید کلی؛ false = اکسپرس برای هیچ فروشگاهی نمایش داده نمی‌شود</summary>
        public bool Enabled { get; set; } = true;
        /// <summary>اضافه‌ی ثابت پاستیل (تومان) وقتی میاره سطح boost برنگرداند یا Boost برای حساب غیرفعال باشد</summary>
        public double FallbackSurchargeToman { get; set; } = 100000;
        /// <summary>
        /// از Boost میاره استفاده شود؟ طبق مستند میاره Boost برای هر حساب جدا فعال می‌شود (تیم بازرگانی) و «دریافت لیست سطوح» به معنی فعال بودن نیست؛
        /// برای حساب غیرفعال درخواست boost بی‌صدا نادیده گرفته می‌شود (تست staging: boost.level = 0 برگشت). تا میاره فعال‌سازی را تأیید نکرده false بماند؛
        /// در این حالت اضافه‌ی اکسپرس همان مبلغ ثابت پاستیل (FallbackSurchargeToman) است و هیچ boostی فرستاده نمی‌شود.
        /// </summary>
        public bool UseBoost { get; set; }
        /// <summary>سطح boost دلخواه (فقط وقتی UseBoost=true)؛ اگر وجود نداشت نزدیک‌ترین سطح پایین‌تر</summary>
        public int BoostLevel { get; set; } = 3;
        /// <summary>ساعت کاری اکسپرس (به وقت تهران): خارج از آن گزینه‌ی اکسپرس نمایش/پذیرفته نمی‌شود</summary>
        public string OpenTime { get; set; } = "09:00";
        public string CloseTime { get; set; } = "20:00";
        /// <summary>
        /// مهلت فروشنده برای آماده‌کردن کالا و «درخواست پیک» بعد از پرداخت (دقیقه). ۶۰ دقیقه؛ بعد از ۳۰ دقیقه بی‌اقدامی
        /// به ادمین پوش می‌رود (Shipping:SellerNotReadyAdminAlertMinutes)، و ۱۵ دقیقه به مهلت مانده به خود فروشنده یادآوری می‌شود.
        /// </summary>
        public int SellerConfirmMinutes { get; set; } = 60;
        /// <summary>پیک تقریباً این‌قدر بعد از درخواست به فروشگاه می‌رسد (pickup.deadline = الان + این مقدار). میاره کمتر از ۳۰ دقیقه را
        /// «deadline_too_soon» رد می‌کند؛ مقدار کمتر از ShippingSlotRules.MinPickupFromNowMinutes به همان حداقل بالا می‌رود.</summary>
        public int PickupLeadMinutes { get; set; } = 30;
        /// <summary>حداکثر فاصله‌ی هوایی فروشگاه تا آدرس برای اکسپرس (کیلومتر)؛ صفر = بدون محدودیت</summary>
        public double MaxDistanceKm { get; set; } = 30;
        /// <summary>قول تحویل به مشتری (ساعت کاری)</summary>
        public int DeliveryMinHours { get; set; } = 2;
        public int DeliveryMaxHours { get; set; } = 3;
    }

    public class ShippingProviderOptions
    {
        public bool Enabled { get; set; } = true;
        public string BaseUrl { get; set; }
        public string ApiKey { get; set; }

        // فقط میاره از این استفاده می‌کند: سرویس استعلام قیمت روی Base URL جدای «Accounting» است،
        // نه Base URL بالا که مخصوص Trip Management (ساخت/لغو سفر) است.
        public string AccountingBaseUrl { get; set; }

        /// <summary>
        /// تصویر پیش‌فرض محل تحویل‌گیری (pickup.image). میاره این فیلد را الزامی می‌کند («Pickup: Image cannot be blank»، تست staging)؛
        /// اگر فروشگاه آیکن/تصویر نداشت همین استفاده می‌شود. باید URL قابل‌دسترس یک تصویر باشد.
        /// </summary>
        public string DefaultPickupImageUrl { get; set; } = "https://pastil.pet/favicon.ico";

        // حداکثر فاصله‌ی هوایی فروشگاه تا آدرس برای نمایش این روش (کیلومتر)؛ صفر یا منفی = بدون محدودیت
        public double MaxDistanceKm { get; set; }

        /// <summary>
        /// فقط میاره: واحد مبلغ‌هایی که API میاره برمی‌گرداند (برآورد قیمت، مبلغ Boost، delivery_cost): "Toman" (پیش‌فرض) یا "Rial".
        /// Staging تومان بود ولی Production ریال برگرداند (برآورد ≈۱۱ کیلومتر = ۹۰۱٬۰۰۰ → ۹۰٬۱۰۰ تومان). قبل از لانچ با یک برآورد
        /// نمونه (یا تیم میاره) تأیید و در env سرور تنظیم شود: Shipping__Miare__AmountUnit=Rial. جزئیات: MiareAmountRules.
        /// </summary>
        public string AmountUnit { get; set; } = "Toman";
    }
}
