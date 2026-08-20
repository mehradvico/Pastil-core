using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.LocationFields.ParkSrv.Dto;
using Entities.Entities.LocationField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.LocationFields.ParkSrv.Iface
{
    public interface IParkService : ICommonSrv<Park, ParkDto>
    {
        BaseSearchDto<ParkVDto> Search(ParkInputDto baseSearchDto);
        Task<BaseResultDto> UpdateMainPictureAsync(ParkMainPictureDto dto);
    }
}
