using Application.Common.Dto.Input;

namespace Application.Services.CompanionSrvs.CompanionSrv.Dto
{
    public class NearbyCompanionInputDto : BaseInputDto
    {
        public NearbyCompanionInputDto()
        {
            RadiusMeter = 10000;
            PageSize = 50;
        }

        public int RadiusMeter { get; set; }
        public bool OnlyInServiceArea { get; set; }
        public long? TypeId { get; set; }
        public long? PetId { get; set; }
        public long? AssistanceId { get; set; }
    }
}
