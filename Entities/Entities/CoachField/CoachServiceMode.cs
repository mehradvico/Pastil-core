using Entities.Entities.CommonField;

namespace Entities.Entities.CoachField
{
    /// <summary>
    /// نحوه‌ی ارائه‌ی خدمت مربی؛ مقدارها هم‌ارز CompanionAssistanceTypeEnum (۳۷/۳۸/۳۹) تا اپ‌ها یکسان بمانند.
    /// ۳۸ برای مربی = «پانسیون آموزشی (در مرکز)»: پت به تعداد روزِ تعیین‌شده‌ی بسته پیش مربی می‌ماند.
    /// </summary>
    public enum CoachServiceModeEnum
    {
        Online = 37,
        /// <summary>پانسیون آموزشی (در مرکز).</summary>
        InPerson = 38,
        /// <summary>در محل مشتری.</summary>
        InPlace = 39,
    }

    /// <summary>قیمت و پیش‌پرداخت یک بسته برای یک نحوه‌ی ارائه. بسته‌ی بدون هیچ ردیف، همه‌ی حالت‌ها را با قیمت پایه ارائه می‌دهد.</summary>
    public class CoachServicePackageType : Id_Field
    {
        public long CoachServicePackageId { get; set; }
        /// <summary>CoachServiceModeEnum</summary>
        public int Mode { get; set; }
        public double Price { get; set; }
        public double PrePaymentPrice { get; set; }
        /// <summary>فقط Mode=۳۸ (پانسیون آموزشی): تعداد روز پانسیون که مربی تعیین می‌کند (۱ تا ۳۶۵). پر = رزرو اقامت چندروزه.</summary>
        public int? StayDays { get; set; }
        /// <summary>تعداد جلسات (Mode=۳۹ در محل مشتری)؛ در رزرو جمع بسته‌ها حساب می‌شود.</summary>
        public int? SessionCount { get; set; }
        /// <summary>
        /// فقط Mode=۳۸: هزینه‌ی پانسیون روزانه. Price = هزینه‌ی دوره، PrePaymentPrice = پیش‌پرداخت دوره.
        /// کل = Price + DailyPrice × StayDays؛ آنلاین = فقط PrePaymentPrice؛ در مرکز = کل − پیش‌پرداخت.
        /// </summary>
        public double? DailyPrice { get; set; }
        public bool Deleted { get; set; }

        public CoachServicePackage CoachServicePackage { get; set; }
    }
}
