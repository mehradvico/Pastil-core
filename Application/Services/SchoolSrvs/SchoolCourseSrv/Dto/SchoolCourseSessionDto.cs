using Application.Common.Dto.Field;
using System;

namespace Application.Services.SchoolSrvs.SchoolCourseSrv.Dto
{
    public class SchoolCourseSessionDto : Id_FieldDto
    {
        public long SchoolCourseId { get; set; }
        public DateTime SessionDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string MeetingUrl { get; set; }
        public bool Active { get; set; }
    }
}
