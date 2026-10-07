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
        // حداقل زمان لازم برای رساندن کالا بعد از تحویل به پیک تا پایان بازه (دقیقه)
        public int MinDeliveryMinutes { get; set; } = 60;
        // ساعت پیش‌فرض تحویل به پیک: چند دقیقه قبل از شروع بازه
        public int PickupLeadMinutes { get; set; } = 30;
        public ShippingProviderOptions AloPeyk { get; set; } = new();
        public ShippingProviderOptions Tipax { get; set; } = new();
        public ShippingProviderOptions SnappBox { get; set; } = new();
        // میاره فقط درون‌شهری/نزدیک است و area_coverage آن مقصد را نمی‌سنجد؛ پس فاصله‌ی فروشگاه تا آدرس مشتری اینجا محدود می‌شود (کیلومتر هوایی)
        public ShippingProviderOptions Miare { get; set; } = new() { MaxDistanceKm = 45 };
    }

    public class ShippingProviderOptions
    {
        public bool Enabled { get; set; } = true;
        public string BaseUrl { get; set; }
        public string ApiKey { get; set; }

        // فقط میاره از این استفاده می‌کند: سرویس استعلام قیمت روی Base URL جدای «Accounting» است،
        // نه Base URL بالا که مخصوص Trip Management (ساخت/لغو سفر) است.
        public string AccountingBaseUrl { get; set; }

        // حداکثر فاصله‌ی هوایی فروشگاه تا آدرس برای نمایش این روش (کیلومتر)؛ صفر یا منفی = بدون محدودیت
        public double MaxDistanceKm { get; set; }
    }
}
