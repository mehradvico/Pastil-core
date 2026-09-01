using Application.Common.Dto.Field;
using Application.Common.Dto.LocationPoint;
using Entities.Entities.CommonField;

namespace Application.Services.TripSrv.PriceCalculationSrv.Dto
{
    public class TripAddressDto : Name_FieldDto
    {
        public PointDto Address { get; set; }
        public long UserId { get; set; }
        public bool IsSelected { get; set; }

    }
}
