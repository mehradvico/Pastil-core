using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Content.StoryItemSrv.Iface
{
    public interface IStoryItemSearchFields
    {
        public long? StoryGroupId { get; set; }
        public long? CompanionId { get; set; }
        public long? PansionId { get; set; }
        public long? StoreId { get; set; }
        public bool? Expired { get; set; }
    }
}
