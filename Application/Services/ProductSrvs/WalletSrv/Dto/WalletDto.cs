using Application.Common.Dto.Field;
using System;

namespace Application.Services.ProductSrvs.WalletSrv.Dto
{
    public class WalletDto : Id_FieldDto
    {
        public string Name { get; set; }
        public bool IsIncrease { get; set; }
        public double Amount { get; set; }
        public DateTime CreateDate { get; set; }
        public long UserId { get; set; }
        public long? PaymentId { get; set; }
        public string ProductOrderId { get; set; }
        public long? CompanionReserveId { get; set; }
        public long? PansionReserveId { get; set; }
        public long? TripId { get; set; }
        public long? CargoId { get; set; }
        public long? CompanionInsurancePackageSaleId { get; set; }
        public long? PastilAiSubscriptionId { get; set; }
        public bool Painding { get; set; }
    }
}
