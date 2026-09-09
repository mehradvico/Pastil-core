using Application.Common.Dto.Field;

namespace Application.Services.SchoolSrvs.SchoolCourseSrv.Dto
{
    public class SchoolCourseDto : Name_FieldDto
    {
        public long SchoolId { get; set; }
        public string Discription { get; set; }
        public int CourseTypeId { get; set; }
        public double Price { get; set; }
        public int SessionCount { get; set; }
        public int SessionDurationMinutes { get; set; }
        public int Capacity { get; set; }
        public long? PetId { get; set; }
        public long? PetBreedId { get; set; }
        public decimal CommissionPercent { get; set; }
        public bool Active { get; set; }
    }
}
