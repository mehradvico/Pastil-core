using Application.Common.Dto.Input;

namespace Application.Services.SchoolSrvs.SchoolSrv.Dto
{
    public class SchoolInputDto : BaseInputDto
    {
        public long? CompanionId { get; set; }
        public bool? Approve { get; set; }
        public bool? ShowToSite { get; set; }
        public long? StateId { get; set; }
        public long? CityId { get; set; }
        public bool? Suggested { get; set; }
    }
}
