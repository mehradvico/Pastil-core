using Application.Common.Dto.Result;
using Application.Services.Accounting.PetBreedCharacteristicSrv.Iface;
using AutoMapper;
using Entities.Entities;
using System.Linq;

namespace Application.Services.Accounting.PetBreedCharacteristicSrv.Dto
{
    public class PetBreedCharacteristicSearchDto : BaseSearchDto<PetBreedCharacteristic, PetBreedCharacteristicVDto>, IPetBreedCharacteristicSearchFields
    {
        public PetBreedCharacteristicSearchDto(PetBreedCharacteristicInputDto dto, IQueryable<PetBreedCharacteristic> list, IMapper mapper) : base(dto, list, mapper)
        {
            PetBreedId = dto.PetBreedId;
        }
        public long? PetBreedId { get; set; }
    }
}
