using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrv.CompanionAssistanceTimeSrv.Dto;
using Application.Services.CompanionSrv.CompanionAssistanceTimeSrv.Dto.Dto;
using Entities.Entities;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrv.CompanionAssistanceTimeSrv.Iface
{
    public interface ICompanionAssistanceTimeService : ICommonSrv<CompanionAssistanceTime, CompanionAssistanceTimeDto>
    {
        CompanionAssistanceTimeSearchDto Search(CompanionAssistanceTimeInputDto baseSearchDto);
        Task<BaseResultDto<CompanionAssistanceTimeVDto>> FindAsyncVDto(long id);
        Task<BaseResultDto<CompanionAssistanceTimeUpdateListDto>> GetListAsync(long companionAssistanceId);
        Task<BaseResultDto> InsertUpdateListAsync(CompanionAssistanceTimeUpdateListDto dto, long? companionId = null);
        Task<BaseResultDto> ActiveAsync(CompanionAssistanceTimeDto dto);
        Task<BaseResultDto> GetForTomarowAsync(CompanionAssistanceTimeInputDto baseSearchDto);
    }
}
