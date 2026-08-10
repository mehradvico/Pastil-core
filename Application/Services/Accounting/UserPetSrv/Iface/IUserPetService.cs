using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Accounting.UserPetSrv.Dto;
using Entities.Entities;
using System.Threading.Tasks;

namespace Application.Services.Accounting.UserPetSrv.Iface
{
    public interface IUserPetService : ICommonSrv<UserPet, UserPetDto>
    {
        UserPetSearchDto Search(UserPetInputDto baseSearchDto);
        Task<BaseResultDto<UserPetVDto>> FindAsyncVDto(long id);
        Task<BaseResultDto<UserPetDto>> UpdateAsyncDto(UserPetDto dto);

    }
}
