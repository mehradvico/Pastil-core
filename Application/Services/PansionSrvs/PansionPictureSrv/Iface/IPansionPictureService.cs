using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.PansionSrvs.PansionPetSrv.Dto;
using Application.Services.PansionSrvs.PansionPictureSrv.Dto;
using Entities.Entities;
using Entities.Entities.PansionField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PansionSrvs.PansionPictureSrv.Iface
{
    public interface IPansionPictureService : ICommonSrv<PansionPicture, PansionPictureDto>
    {
        PansionPictureSearchDto Search(PansionPictureInputDto searchDto);
        Task<BaseResultDto<PansionPictureVDto>> FindAsyncVDto(long id);
    }
}
