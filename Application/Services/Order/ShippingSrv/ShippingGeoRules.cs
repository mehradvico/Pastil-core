using System;

namespace Application.Services.Order.ShippingSrv
{
    // قواعد خالص جغرافیایی ارسال (بدون شبکه/دیتابیس).
    // چرا: فیلد area_coverage در استعلام میاره فقط «مبدأ» (فروشگاه) را در محدوده‌ی میاره می‌سنجد، نه مقصد؛
    // پس آدرسی در شیراز با فروشگاه تهران هم «پوشش‌داده‌شده» برمی‌گردد و یک قیمت (مثلاً ۱۳۰ هزار) می‌دهد.
    public static class ShippingGeoRules
    {
        private const double EarthRadiusKm = 6371.0088;

        // فاصله‌ی هوایی دو نقطه (کیلومتر)، فرمول haversine
        public static double DistanceKm(double lat1, double lon1, double lat2, double lon2)
        {
            static double Rad(double deg) => deg * Math.PI / 180.0;
            var dLat = Rad(lat2 - lat1);
            var dLon = Rad(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(Rad(lat1)) * Math.Cos(Rad(lat2)) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return 2 * EarthRadiusKm * Math.Asin(Math.Min(1, Math.Sqrt(a)));
        }

        // maxKm <= 0 یعنی محدودیتی نیست
        public static bool IsWithinRange(double distanceKm, double maxKm) => maxKm <= 0 || distanceKm <= maxKm;
    }
}
