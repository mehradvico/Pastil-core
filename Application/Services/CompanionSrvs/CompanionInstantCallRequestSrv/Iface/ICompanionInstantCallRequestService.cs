using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrvs.CompanionInstantCallRequestSrv.Dto;
using Entities.Entities;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.CompanionInstantCallRequestSrv.Iface
{
    public interface ICompanionInstantCallRequestService : ICommonSrv<CompanionInstantCallRequest, CompanionInstantCallRequestDto>
    {
        CompanionInstantCallRequestSearchDto Search(CompanionInstantCallRequestInputDto baseSearchDto);
        Task<BaseResultDto<CompanionInstantCallRequestVDto>> FindAsyncVDto(long id);
    }
}
