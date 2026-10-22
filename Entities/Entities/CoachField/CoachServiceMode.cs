using Entities.Entities.CommonField;

namespace Entities.Entities.CoachField
{
    /// <summary>نحوه‌ی ارائه‌ی خدمت؛ مقدارها هم‌ارز CompanionAssistanceTypeEnum (۳۷/۳۸/۳۹) تا اپ‌ها یکسان بمانند.</summary>
    public enum CoachServiceModeEnum
    {
        Online = 37,
        InPerson = 38,
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
        /// <summary>مدت اقامت/دوره به روز (مثلاً پانسیون آموزشی در Mode=۳۸)؛ در رزرو بیشینه‌ی بسته‌ها حساب می‌شود.</summary>
        public int? StayDays { get; set; }
        /// <summary>تعداد جلسات (مثلاً آموزش در منزل، Mode=۳۹)؛ در رزرو جمع بسته‌ها حساب می‌شود.</summary>
        public int? SessionCount { get; set; }
        public bool Deleted { get; set; }

        public CoachServicePackage CoachServicePackage { get; set; }
    }
}
