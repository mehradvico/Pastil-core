using Application.Common.Dto.LocationPoint;
using Application.Common.Dto.Result;

namespace Application.Services.CompanionSrvs.CompanionSrv.Dto
{
    public class NearbyCompanionSearchDto : BaseSearchDto<NearbyCompanionVDto>
    {
        public NearbyCompanionSearchDto(NearbyCompanionInputDto inputDto) : base(inputDto)
        {
            RadiusMeter = inputDto.RadiusMeter;
        }

        public int RadiusMeter { get; set; }
        public PointDto CenterLocation { get; set; }
        public long CityId { get; set; }
        public long? NeighborhoodId { get; set; }
    }
}
