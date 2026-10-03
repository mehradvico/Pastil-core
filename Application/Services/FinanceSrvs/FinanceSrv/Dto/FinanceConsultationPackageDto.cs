namespace Application.Services.FinanceSrvs.FinanceSrv.Dto
{
    public class FinanceConsultationPackageDto
    {
        public long ConsultationPackageId { get; set; }
        public decimal CommissionPercent { get; set; }
    }

    // پکیج مشاوره‌ی آنلاین یک نماینده در حسابداری: درصد سهم سایت روی هر خرید همین پکیج
    public class ConsultationPackageFinanceVDto
    {
        public long Id { get; set; }
        public long CompanionId { get; set; }
        public string Name { get; set; }
        public int ChannelId { get; set; }
        public int DurationMinutes { get; set; }
        public double Price { get; set; }
        public bool Active { get; set; }
        public bool Bookable { get; set; }
        public decimal CommissionPercent { get; set; }
    }
}
