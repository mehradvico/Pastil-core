namespace Application.Services.SchoolSrvs.SchoolCourseLiveSrv.Dto
{
    public class SchoolLiveStartDto
    {
        public long SchoolCourseSessionId { get; set; }
        public bool WantsRecording { get; set; }
        // فقط وقتی WantsRecording=true اجباری‌اند
        public string RecordingName { get; set; }
        public string RecordingDescription { get; set; }
    }
}
