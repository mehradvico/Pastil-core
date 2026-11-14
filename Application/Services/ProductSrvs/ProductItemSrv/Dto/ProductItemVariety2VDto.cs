using Application.Services.ProductSrvs.VarietyItemSrv.Dto;
using Application.Services.ProductSrvs.VarietySrv.Dto;
using System.Collections.Generic;

namespace Application.Services.ProductSrvs.ProductItemSrv.Dto
{
    public class ProductItemVariety2VDto
    {
        public VarietyShowVDto Variety1 { get; set; }
        public List<VarietyItemMinVDto> VarietyItems1 { get; set; } = new List<VarietyItemMinVDto>();
        public VarietyShowVDto Variety2 { get; set; }
        public List<VarietyItemMinVDto> VarietyItems2 { get; set; } = new List<VarietyItemMinVDto>();
        public long? VarietyItem1Id { get; set; }
        public long? VarietyItem2Id { get; set; }
        public List<ProductItemShowVDto> ProductItems { get; set; } = new List<ProductItemShowVDto>();
        /// <summary>فروشگاه‌های دارای «همین تنوع انتخاب‌شده» از کم به زیاد.</summary>
        public List<ProductStoreOfferVDto> Offers { get; set; } = new List<ProductStoreOfferVDto>();
        public long? PreferredStoreId { get; set; }
    }

}
