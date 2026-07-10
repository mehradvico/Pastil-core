using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.LocationFields.ParkPictureSrv.Dto;
using Entities.Entities.LocationField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.LocationFields.ParkPictureSrv.Iface
{
    public interface IParkPictureService : ICommonSrv<ParkPicture, ParkPictureDto>
    {
        ParkPictureSearchDto Search(ParkPictureInputDto searchDto);
        Task<BaseResultDto<ParkPictureVDto>> FindAsyncVDto(long id);
    }
}
