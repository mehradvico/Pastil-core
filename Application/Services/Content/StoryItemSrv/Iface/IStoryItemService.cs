using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Content.StoryItemSrv.Dto;
using Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Content.StoryItemSrv.Iface
{
    public interface IStoryItemService : ICommonSrv<StoryItem, StoryItemDto>
    {
        Task<BaseResultDto<StoryItemVDto>> FindAsyncVDto(long id, bool view = true);
        BaseSearchDto<StoryItemVDto> Search(StoryItemInputDto searchDto);
    }
}
