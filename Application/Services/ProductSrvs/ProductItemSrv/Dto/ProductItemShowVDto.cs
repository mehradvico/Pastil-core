    using Application.Common.Dto.Field;
    using Application.Services.ProductSrvs.DiscountGroupSrv.Dto;
    using Application.Services.ProductSrvs.StoreSrv.Dto;
    using System;

    namespace Application.Services.ProductSrvs.ProductItemSrv.Dto
    {
        public class ProductItemShowVDto : Id_FieldDto
        {
            public long BasePrice { get; set; }
            public long Price { get; set; }
            public int DiscountPercent { get; set; }
            public long? DiscountGroupId { get; set; }
            public DateTime? DiscountEndDate { get; set; }
            public int Quantity { get; set; }
            public bool SystemActive { get; set; }
            public string Warranty { get; set; }
            public StoreMinVDto Store { get; set; }
            public DiscountGroupVDto DiscountGroup { get; set; }

        }
    }
