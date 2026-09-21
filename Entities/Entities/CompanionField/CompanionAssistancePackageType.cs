using Entities.Entities.CommonField;

namespace Entities.Entities
{
    /// <summary>
    /// «نحوه ارائه» یک پکیج (آنلاین / حضوری در مرکز / در محل مشتری) همراه با قیمت و پیش‌پرداخت همان حالت.
    /// اگر پکیجی هیچ ردیفی نداشته باشد (پکیج‌های قدیمی)، همه‌ی حالت‌های خدمت با قیمت خود پکیج ارائه می‌شود.
    /// </summary>
    public class CompanionAssistancePackageType : Id_Field
    {
        public long CompanionAssistancePackageId { get; set; }
        /// <summary>شناسه‌ی Code از CompanionAssistanceTypeEnum (۳۷ آنلاین، ۳۸ حضوری در مرکز، ۳۹ در محل مشتری)</summary>
        public long CompanionAssistanceTypeId { get; set; }
        public double Price { get; set; }
        public double PrePaymentPrice { get; set; }
        public bool Deleted { get; set; }
        public CompanionAssistancePackage CompanionAssistancePackage { get; set; }
        public Code CompanionAssistanceType { get; set; }
    }
}
