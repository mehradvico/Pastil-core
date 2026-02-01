using Application.Common.Dto.Input;
using Application.Services.Content.StoryItemSrv.Iface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Content.StoryItemSrv.Dto
{
    public class StoryItemInputDto : BaseInputDto, IStoryItemSearchFields
    {
        public long? StoryGroupId { get; set; }
        public long? CompanionId { get; set; }
        public long? PansionId { get; set; }
        public long? StoreId { get; set; }
    }
}
