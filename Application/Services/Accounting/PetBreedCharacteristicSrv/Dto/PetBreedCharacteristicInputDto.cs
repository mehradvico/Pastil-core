using Application.Common.Dto.Input;
using Application.Services.Accounting.PetBreedCharacteristicSrv.Iface;

namespace Application.Services.Accounting.PetBreedCharacteristicSrv.Dto
{
    public class PetBreedCharacteristicInputDto : BaseInputDto, IPetBreedCharacteristicSearchFields
    {
        public long? PetBreedId { get; set; }
    }
}
