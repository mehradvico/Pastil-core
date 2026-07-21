using Application.Common.Dto.Field;
using Application.Common.Dto.LocationPoint;
using Application.Services.Filing.PictureSrv.Dto;

namespace Application.Services.CompanionSrvs.CompanionSrv.Dto
{
    public class NearbyCompanionVDto : Id_FieldDto
    {
        public string Name { get; set; }
        public string AddressValue { get; set; }
        public string Phone { get; set; }
        public long CityId { get; set; }
        public long? NeighborhoodId { get; set; }
        public string CityName { get; set; }
        public string NeighborhoodName { get; set; }
        public long? PictureId { get; set; }
        public PointDto Location { get; set; }
        public PictureVDto Picture { get; set; }
        public double RateAvg { get; set; }
        public int RateCount { get; set; }
        public bool IsGold { get; set; }
        public bool IsSilver { get; set; }
        public bool HasPansion { get; set; }
        public double DistanceMeter { get; set; }
        public bool HasServiceZone { get; set; }
        public bool? IsInServiceArea { get; set; }
    }
}
