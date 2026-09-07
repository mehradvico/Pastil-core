using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Accounting.PetBreedCharacteristicSrv.Dto;
using Entities.Entities;
using System.Threading.Tasks;

namespace Application.Services.Accounting.PetBreedCharacteristicSrv.Iface
{
    public interface IPetBreedCharacteristicService : ICommonSrv<PetBreedCharacteristic, PetBreedCharacteristicDto>
    {
        PetBreedCharacteristicSearchDto Search(PetBreedCharacteristicInputDto searchDto);
        Task<BaseResultDto<PetBreedCharacteristicVDto>> FindAsyncVDto(long id);
    }
}
