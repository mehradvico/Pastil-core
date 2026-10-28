using Application.Common.Dto.Field;
using System;

namespace Application.Services.SchoolSrvs.SchoolCourseSrv.Dto
{
    /// <summary>تعیین یک‌جای روزهای دوره (PUT SchoolCourseSession/Schedule)</summary>
    public class SchoolCourseScheduleDto
    {
        public long SchoolCourseId { get; set; }
        public System.Collections.Generic.List<SchoolCourseScheduleItemDto> Sessions { get; set; } = new();
    }

    public class SchoolCourseScheduleItemDto
    {
        public DateTime SessionDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }

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
