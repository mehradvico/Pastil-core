using Application.Common.Dto.Result;
using AutoMapper;
using Entities.Entities.SchoolField;
using System.Linq;

namespace Application.Services.SchoolSrvs.SchoolCourseSrv.Dto
{
    public class SchoolCourseSearchDto : BaseSearchDto<SchoolCourse, SchoolCourseVDto>
    {
        public SchoolCourseSearchDto(SchoolCourseInputDto dto, IQueryable<SchoolCourse> list, IMapper mapper) : base(dto, list, mapper)
        {
            SchoolId = dto.SchoolId;
            CourseTypeId = dto.CourseTypeId;
            PetId = dto.PetId;
        }

        public long? SchoolId { get; set; }
        public int? CourseTypeId { get; set; }
        public long? PetId { get; set; }
    }
}
