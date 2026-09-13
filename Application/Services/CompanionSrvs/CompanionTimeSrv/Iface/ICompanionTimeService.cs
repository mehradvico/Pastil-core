using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrv.CompanionTimeSrv.Dto;
using Entities.Entities;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrv.CompanionTimeSrv.Iface
{
    public interface ICompanionTimeService : ICommonSrv<CompanionTime, CompanionTimeDto>
    {
        CompanionTimeSearchDto Search(CompanionTimeInputDto baseSearchDto);
        Task<CompanionTimeSearchDto> SearchWithAvailabilityAsync(CompanionTimeInputDto baseSearchDto);
        Task<BaseResultDto<CompanionTimeVDto>> FindAsyncVDto(long id);
        Task<BaseResultDto<CompanionTimeUpdateListDto>> GetListAsync(long companionId);
        Task<BaseResultDto> InsertUpdateListAsync(CompanionTimeUpdateListDto dto, long? companionOwnerId = null);
    }
}
