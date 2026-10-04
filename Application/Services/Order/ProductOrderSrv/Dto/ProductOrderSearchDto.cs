using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Services.Order.ProductOrderSrv.Dto;
using Application.Services.Order.ProductOrderSrv.Iface;
using AutoMapper;
using Entities.Entities;
using System;
using System.Linq;

namespace Application.Services.Order.ProductOrderOrderSrv.Dto
{
    public class ProductOrderSearchDto : BaseSearchDto<ProductOrder, ProductOrderVDto>, IProductOrderSearchFields
    {
        public ProductOrderSearchDto(ProductOrderInputDto dto, IQueryable<ProductOrder> list, IMapper mapper) : base(dto, list, mapper)
        {
            this.DateFrom = dto.DateFrom;
            this.DateTo = dto.DateTo;
            this.ProductOrderStateEnum = dto.ProductOrderStateEnum;
            this.ProductOrderStatusEnum = dto.ProductOrderStatusEnum;
            this.UserId = dto.UserId;
            this.StoreId = dto.StoreId;
            this.TrackingCode = dto.TrackingCode;
            this.HasCancelRequestDate = dto.HasCancelRequestDate;
            this.HasParentOrderId = dto.HasParentOrderId;
            this.HasChildOrderId = dto.HasChildOrderId;
            this.HasReserveDate = dto.HasReserveDate;
            this.UserDelivery = dto.UserDelivery;
            this.HasReserveDate = dto.HasReserveDate;
        }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public ProductOrderStateEnum? ProductOrderStateEnum { get; set; }
        public ProductOrderStatusEnum? ProductOrderStatusEnum { get; set; }
        public long? UserId { get; set; }
        public long? StoreId { get; set; }
        public string TrackingCode { get; set; }
        public bool? HasCancelRequestDate { get; set; }
        public bool? HasParentOrderId { get; set; }
        public bool? HasChildOrderId { get; set; }
        public bool? HasReserveDate { get; set; }
        public int? UserDelivery { get; set; }


    }
}
