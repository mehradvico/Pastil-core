using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrvs.CompanionAssistancePackageOnlineSrv.Dto;
using Entities.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.CompanionAssistancePackageOnlineSrv.Iface
{
    public interface ICompanionAssistancePackageOnlineService : ICommonSrv<CompanionAssistancePackageOnline, CompanionAssistancePackageOnlineDto>
    {
        CompanionAssistancePackageOnlineSearchDto Search(CompanionAssistancePackageOnlineInputDto baseSearchDto);
        Task<BaseResultDto<CompanionAssistancePackageOnlineVDto>> FindAsyncVDto(long id);
        Task<List<CompanionAssistancePackageOnline>> GetListAsync(List<long> ids);
    }
}
