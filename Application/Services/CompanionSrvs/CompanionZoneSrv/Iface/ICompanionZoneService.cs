using Application.Common.Interface;
using Application.Services.CompanionSrvs.CompanionZoneSrv.Dto;
using Entities.Entities.CompanionField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common.Dto.Result;

namespace Application.Services.CompanionSrvs.CompanionZoneSrv.Iface
{
    public interface ICompanionZoneService : ICommonSrv<CompanionZone, CompanionZoneDto>
    {
        CompanionZoneSearchDto Search(CompanionZoneInputDto dto);
        Task<BaseResultDto<CompanionZoneDto>> FindForCompanionAsync(long id, long companionId);
        Task<BaseResultDto<CompanionZoneDto>> UpdateAsyncDto(CompanionZoneDto dto, long? companionId = null);
        Task<BaseResultDto> DeleteAsync(long id, long? companionId = null);
    }
}
