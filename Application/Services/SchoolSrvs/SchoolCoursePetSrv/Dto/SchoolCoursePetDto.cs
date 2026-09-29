using Application.Common.Dto.Field;

namespace Application.Services.SchoolSrvs.SchoolCoursePetSrv.Dto
{
    public class SchoolCoursePetDto : Id_FieldDto
    {
        public long SchoolCourseId { get; set; }
        public long PetId { get; set; }
        public long? PetBreedId { get; set; }
    }
}
