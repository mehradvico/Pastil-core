using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;

namespace Entities.Entities
{
    public class Payment : Id_Field
    {
        public long? MerchantId { get; set; }
        public string ProductOrderId { get; set; }
        public long? CompanionReserveId { get; set; }
        public long? TripId { get; set; }
        public long? CargoId { get; set; }
        public long? CompanionInsurancePackageSaleId { get; set; }
        public string RefNumber { get; set; }
        public double Amount { get; set; }
        public DateTime CreateDate { get; set; }
        public string Description { get; set; }
        public bool? IsSuccess { get; set; }
        public bool IsOnline { get; set; }
        public long? FileId { get; set; }
        public long UserId { get; set; }
        public long TypeId { get; set; }
        public string CallBackTypeLabel { get; set; }
        public string CallBackId { get; set; }
        public string Token { get; set; }
        public string GatewayStatus { get; set; }
        public Merchant Merchant { get; set; }
        public File File { get; set; }
        public ProductOrder ProductOrder { get; set; }
        public Wallet Wallet { get; set; }
        public Code Type { get; set; }
        public User User { get; set; }

    }
}
