namespace Application.Services.FinanceSrvs.SettlementSrv.Dto
{
    /// <summary>یک ردیف از ریز تسویه: چیزی که داخل این تسویه پرداخت شده است.</summary>
    public class SettlementItemVDto
    {
        /// <summary>Reserve | Pansion | Consultation | School | Order | Groomer | Clinic</summary>
        public string Type { get; set; }
        public string Code { get; set; }
        public string VenueName { get; set; }
        public string BookerFullName { get; set; }
        public double PaymentPrice { get; set; }
        public double SiteShare { get; set; }
        public double ProviderShare { get; set; }
    }
}
