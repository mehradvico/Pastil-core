using Application.Common.Dto.Result;
using Application.Services.SchoolSrvs.SchoolCoursePetSrv.Iface;
using AutoMapper;
using Entities.Entities.SchoolField;
using System.Linq;

namespace Application.Services.SchoolSrvs.SchoolCoursePetSrv.Dto
{
    public class SchoolCoursePetSearchDto : BaseSearchDto<SchoolCoursePet, SchoolCoursePetVDto>, ISchoolCoursePetSearchFields
    {
        public SchoolCoursePetSearchDto(SchoolCoursePetInputDto dto, IQueryable<SchoolCoursePet> list, IMapper mapper) : base(dto, list, mapper)
        {
            SchoolCourseId = dto.SchoolCourseId;
            CompanionId = dto.CompanionId;
        }
        public long? SchoolCourseId { get; set; }
        public long? CompanionId { get; set; }
    }
}
