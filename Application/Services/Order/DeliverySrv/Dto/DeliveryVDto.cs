using Application.Common.Dto.Field;
using Application.Services.LocationFields.CitySrv.Dto;
using Application.Services.LocationFields.StateSrv.Dto;
using Application.Services.Order.DeliveryDistanceSrv.Dto;
using Application.Services.ProductSrvs.StoreSrv.Dto;
using Application.Services.Setting.CodeSrv.Dto;
using System.Collections.Generic;

namespace Application.Services.Order.DeliverySrv.Dto
{
    public class DeliveryVDto : Id_FieldDto
    {
        public long DeliveryTypeId { get; set; }
        public double BasePrice { get; set; }
        public double MinPriceForFree { get; set; }
        public int MinCountForFree { get; set; }

        public int MaxDays { get; set; }
        public long? CityId { get; set; }
        public long? StateId { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
        public long? StoreId { get; set; }
        public bool AfterRent { get; set; }

        public StoreMinVDto Store { get; set; }
        public CityVDto City { get; set; }
        public CodeVDto DeliveryType { get; set; }
        public StateVDto State { get; set; }
        public List<DeliveryDistanceVDto> DeliveryDistance { get; set; }
    }
}
