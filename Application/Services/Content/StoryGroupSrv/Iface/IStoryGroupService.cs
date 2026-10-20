using Application.Common.Dto.Result;
using Application.Services.Content.StoryGroupSrv.Dto;
using Application.Services.Content.StaticPageSrv.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common.Interface;
using Entities.Entities;

namespace Application.Services.Content.StoryGroupSrv.Iface
{
    public interface IStoryGroupService : ICommonSrv<StoryGroup, StoryGroupDto>
    {
        Task<BaseResultDto<StoryGroupVDto>> FindAsyncVDto(long id);
        BaseSearchDto<StoryGroupVDto> Search(StoryGroupInputDto searchDto);
        Task<List<StoryGroupHomeVDto>> HomeAsync();
    }
}
