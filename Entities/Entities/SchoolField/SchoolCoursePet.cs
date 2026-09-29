using Entities.Entities.CommonField;

namespace Entities.Entities.SchoolField
{
    // یک نوع پت مورد پذیرش برای یک دوره‌ی مدرسه (چندبه‌چند: هر دوره می‌تواند چند نوع/نژاد پت بپذیرد).
    // PetBreedId اختیاری است: پر نشدنش یعنی همه‌ی نژادهای همان Pet پذیرفته می‌شوند.
    public class SchoolCoursePet : Id_Field
    {
        public long SchoolCourseId { get; set; }
        public long PetId { get; set; }
        public long? PetBreedId { get; set; }
        public bool Deleted { get; set; }

        public SchoolCourse SchoolCourse { get; set; }
        public Pet Pet { get; set; }
        public PetBreed PetBreed { get; set; }
    }
}
