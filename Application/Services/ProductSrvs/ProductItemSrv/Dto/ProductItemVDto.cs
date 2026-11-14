using Application.Common.Dto.Field;
using Application.Services.ProductSrvs.ProductSrv.Dto;
using Application.Services.ProductSrvs.StoreSrv.Dto;
using Application.Services.ProductSrvs.VarietyItemSrv.Dto;
using System;

namespace Application.Services.ProductSrvs.ProductItemSrv.Dto
{
    public class ProductItemVDto : Id_FieldDto
    {
        public long BasePrice { get; set; }
        public long Price { get; set; }
        public int DiscountPercent { get; set; }
        /// <summary>درصد تخفیفِ پاستیل (روی قیمتِ بعد از تخفیف فروشگاه). تخفیف فروشگاه = ردیف‌های Discount با FundedByPastil=false.</summary>
        public int PastilDiscountPercent { get; set; }
        public double PastilDiscountAmount { get; set; }
        public long? DiscountGroupId { get; set; }
        public DateTime? DiscountEndDate { get; set; }
        public long? VarietyItemId { get; set; }
        public long? VarietyItem2Id { get; set; }
        public long ProductId { get; set; }
        public long StoreId { get; set; }
        public int Quantity { get; set; }
        public string Warranty { get; set; }
        public bool Active { get; set; }
        public bool SystemActive { get; set; }

        public long? CategoryId { get; set; }
        public string ProductName { get; set; }
        public string VarietyName { get; set; }
        public string StoreName { get; set; }

        public ProductVDto Product { get; set; }
        public VarietyItemVDto VarietyItem { get; set; }
        public VarietyItemVDto VarietyItem2 { get; set; }
        public StoreMinVDto Store { get; set; }
    }
}
