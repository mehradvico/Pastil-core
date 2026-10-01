using Application.Common.Dto.Field;
using System;

namespace Application.Services.SchoolSrvs.SchoolCourseLiveSrv.Dto
{
    public class SchoolCourseLiveSessionVDto : Id_FieldDto
    {
        public long SchoolCourseSessionId { get; set; }
        public string RoomName { get; set; }
        public int StatusId { get; set; }
        public DateTime? StartedDate { get; set; }
        public DateTime? EndedDate { get; set; }
        public bool WantsRecording { get; set; }
        public string RecordingName { get; set; }
        public string RecordingDescription { get; set; }
        public int RecordingStatusId { get; set; }
        public long? SchoolCourseVideoId { get; set; }
    }
}
