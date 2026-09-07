using Application.Common.Dto.Field;

namespace Application.Services.Accounting.PetBreedCharacteristicSrv.Dto
{
    public class PetBreedCharacteristicVDto : Name_FieldDto
    {
        public long PetBreedId { get; set; }
        public bool IsPositive { get; set; }
        public int Priority { get; set; }
    }
}
