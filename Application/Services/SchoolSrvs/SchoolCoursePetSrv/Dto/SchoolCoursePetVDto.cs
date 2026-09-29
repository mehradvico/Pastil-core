using Application.Common.Dto.Field;
using Application.Services.Accounting.PetBreedSrv.Dto;
using Application.Services.Accounting.PetSrv.Dto;

namespace Application.Services.SchoolSrvs.SchoolCoursePetSrv.Dto
{
    public class SchoolCoursePetVDto : Id_FieldDto
    {
        public long SchoolCourseId { get; set; }
        public long PetId { get; set; }
        public long? PetBreedId { get; set; }

        public PetVDto Pet { get; set; }
        public PetBreedVDto PetBreed { get; set; }
    }
}
