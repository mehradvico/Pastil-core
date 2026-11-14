using Application.Common.Dto.Field;

namespace Application.Services.Order.ProductOrderItemSrv.Dto
{
    public class ProductOrderItemDto : Id_FieldDto
    {
        public long ProductItemId { get; set; }
        public int Count { get; set; }
        public double Price { get; set; }
        public double BasePrice { get; set; }
        public double DiscountPrice { get; set; }
        public int DiscountPercent { get; set; }
        public double PastilDiscountUnit { get; set; }
        public long ProductOrderStoreId { get; set; }
        public string Description { get; set; }
        public bool Deleted { get; set; }
        public bool Edited { get; set; }
        public string ProductOrderId { get; set; }

    }
}
