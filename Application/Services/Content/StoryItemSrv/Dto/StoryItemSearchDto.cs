using Application.Common.Dto.Result;
using Application.Services.Content.StoryItemSrv.Dto;
using Application.Services.Content.StoryItemSrv.Iface;
using AutoMapper;
using Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Content.StoryItemSrv.Dto
{
    public class StoryItemSearchDto : BaseSearchDto<StoryItem, StoryItemVDto>, IStoryItemSearchFields
    {
        public StoryItemSearchDto(StoryItemInputDto dto, IQueryable<StoryItem> list, IMapper mapper) : base(dto, list, mapper)
        {
            this.StoreId = dto.StoreId;
            this.PansionId = dto.PansionId;
            this.CompanionId = dto.CompanionId;
            this.StoryGroupId = dto.StoryGroupId;
        }

        public long? StoryGroupId { get; set; }
        public long? CompanionId { get; set; }
        public long? PansionId { get; set; }
        public long? StoreId { get; set; }
    }
}
