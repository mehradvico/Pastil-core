using Application.Common.Dto.Result;
using Application.Services.Content.StoryItemSrv.Dto;
using Application.Services.Content.StoryUserLikeSrv.Dto;
using Application.Services.Content.StoryUserLikeSrv.Iface;
using AutoMapper;
using Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Content.StoryUserLikeSrv.Dto
{
    public class StoryUserLikeSearchDto : BaseSearchDto<StoryItem, StoryItemVDto>, IStoryUserLikeSearchFields
    {
        public StoryUserLikeSearchDto(StoryUserLikeInputDto dto, IQueryable<StoryItem> list, IMapper mapper) : base(dto, list, mapper)
        {
            UserId = dto.UserId;
        }

        public long UserId { get; set; }
    }
}
