namespace Application.Services.CompanionSrv.CompanionAssistancePackageSrv.Dto
{
    /// <summary>قیمت و پیش‌پرداخت یک پکیج در یک «نحوه ارائه» (۳۷ آنلاین، ۳۸ حضوری در مرکز، ۳۹ در محل مشتری).</summary>
    public class CompanionAssistancePackageTypeDto
    {
        public long Id { get; set; }
        public long CompanionAssistanceTypeId { get; set; }
        public string CompanionAssistanceTypeName { get; set; }
        public double Price { get; set; }
        public double PrePaymentPrice { get; set; }
    }
}
