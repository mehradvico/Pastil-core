using Application.Common.Dto.Field;
using Application.Common.Dto.LocationPoint;
using Application.Services.Dto;

namespace Application.Services.TripSrv.PriceCalculationSrv.Dto
{
    public class TripAddressVDto : Name_FieldDto
    {
        public PointDto Address { get; set; }
        public long UserId { get; set; }
        public bool IsSelected { get; set; }

        public UserMinVDto User { get; set; }

    }
}
