using Application.Common.Dto.Result;
using AutoMapper;
using Entities.Entities.SchoolField;
using System.Linq;

namespace Application.Services.SchoolSrvs.SchoolSrv.Dto
{
    public class SchoolSearchDto : BaseSearchDto<School, SchoolVDto>
    {
        public SchoolSearchDto(SchoolInputDto dto, IQueryable<School> list, IMapper mapper) : base(dto, list, mapper)
        {
            CompanionId = dto.CompanionId;
            Approve = dto.Approve;
            ShowToSite = dto.ShowToSite;
            StateId = dto.StateId;
            CityId = dto.CityId;
            Suggested = dto.Suggested;
        }

        public long? CompanionId { get; set; }
        public bool? Approve { get; set; }
        public bool? ShowToSite { get; set; }
        public long? StateId { get; set; }
        public long? CityId { get; set; }
        public bool? Suggested { get; set; }
    }
}
