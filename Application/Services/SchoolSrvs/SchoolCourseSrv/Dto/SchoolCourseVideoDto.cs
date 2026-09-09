using Application.Common.Dto.Field;

namespace Application.Services.SchoolSrvs.SchoolCourseSrv.Dto
{
    public class SchoolCourseVideoDto : Name_FieldDto
    {
        public long SchoolCourseId { get; set; }
        public long FileId { get; set; }
        public int SortOrder { get; set; }
        public int? DurationSeconds { get; set; }
        public bool Active { get; set; }
    }
}
