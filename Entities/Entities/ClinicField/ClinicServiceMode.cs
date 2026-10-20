using Entities.Entities.CommonField;

namespace Entities.Entities.ClinicField
{
    /// <summary>نحوه‌ی ارائه‌ی خدمت؛ مقدارها هم‌ارز CompanionAssistanceTypeEnum (۳۷/۳۸/۳۹) تا اپ‌ها یکسان بمانند.</summary>
    public enum ClinicServiceModeEnum
    {
        Online = 37,
        InPerson = 38,
        InPlace = 39,
    }

    /// <summary>قیمت و پیش‌پرداخت یک بسته برای یک نحوه‌ی ارائه. بسته‌ی بدون هیچ ردیف، همه‌ی حالت‌ها را با قیمت پایه ارائه می‌دهد.</summary>
    public class ClinicServicePackageType : Id_Field
    {
        public long ClinicServicePackageId { get; set; }
        /// <summary>ClinicServiceModeEnum</summary>
        public int Mode { get; set; }
        public double Price { get; set; }
        public double PrePaymentPrice { get; set; }
        public bool Deleted { get; set; }

        public ClinicServicePackage ClinicServicePackage { get; set; }
    }
}
