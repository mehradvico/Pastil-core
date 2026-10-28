namespace Entities.Entities.SchoolField
{
    public class SchoolComment : Comment
    {
        public long SchoolId { get; set; }
        public long? SchoolReserveId { get; set; }
        // نظر/امتیاز برای یک دوره‌ی مشخص (اختیاری)؛ امتیاز دوره از همین‌ها محاسبه می‌شود
        public long? SchoolCourseId { get; set; }
        public bool IsReserved { get; set; }
        public School School { get; set; }
        public SchoolReserve SchoolReserve { get; set; }
        public SchoolCourse SchoolCourse { get; set; }
    }
}
