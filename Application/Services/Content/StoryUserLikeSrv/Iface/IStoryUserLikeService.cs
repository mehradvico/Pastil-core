using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Content.StoryUserLikeSrv.Dto;
using Application.Services.ProductSrvs.ProductLikeSrv.Dto;
using Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Content.StoryUserLikeSrv.Iface
{
    public interface IStoryUserLikeService : ICommonSrv<StoryUserLike, StoryUserLikeDto>
    {
        Task ToggleLikeAsync(long storyItemId, long userId);
        StoryUserLikeSearchDto SearchDto(StoryUserLikeInputDto dto);
    }
}
