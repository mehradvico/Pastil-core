using Application.Common.Dto.Input;
using Application.Services.SchoolSrvs.SchoolCoursePetSrv.Iface;

namespace Application.Services.SchoolSrvs.SchoolCoursePetSrv.Dto
{
    public class SchoolCoursePetInputDto : BaseInputDto, ISchoolCoursePetSearchFields
    {
        public long? SchoolCourseId { get; set; }
        public long? CompanionId { get; set; }
    }
}
