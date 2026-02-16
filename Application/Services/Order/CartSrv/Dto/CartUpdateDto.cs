using Application.Common.Enumerable;

namespace Application.Services.Order.CartSrv.Dto
{
    public class CartUpdateDto
    {
        public CartUpdateEnum CartUpdateType { get; set; }
        public bool FromWallet { get; set; }
        public long? UserId { get; set; }
        public string UniqueId { get; set; }
        public long? AddressId { get; set; }
        public string BonusCode { get; set; }
        public string RebateCode { get; set; }
        public long? DeliveryId { get; set; }
        public long? MerchantId { get; set; }
        public long? ProductItemId { get; set; }
        public int Count { get; set; } = 1;
        public bool Reserve { get; set; }
        public string ParentOrderId { get; set; }
        public long StoreId { get; set; }
    }
}
