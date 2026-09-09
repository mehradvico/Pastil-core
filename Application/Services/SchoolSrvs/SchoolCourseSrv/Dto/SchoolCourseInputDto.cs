using Application.Common.Dto.Input;

namespace Application.Services.SchoolSrvs.SchoolCourseSrv.Dto
{
    public class SchoolCourseInputDto : BaseInputDto
    {
        public long? SchoolId { get; set; }
        public int? CourseTypeId { get; set; }
        public long? PetId { get; set; }
    }
}
