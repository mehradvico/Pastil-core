using Entities.Entities.CommonField;
using Entities.Entities.PansionField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Entities
{
    public class StoryItem : Id_Field
    {

        public string Url { get; set; }
        public long? CompanionId { get; set; }
        public long? StoreId { get; set; }
        public long? PansionId { get; set; }
        public int Priority { get; set; }
        public long StoryGroupId { get; set; }
        public long PictureId { get; set; }
        public int ViewCount { get; set; }
        public int LikeCount { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }

        public StoryGroup StoryGroup { get; set; }
        public Picture Picture { get; set; }
        public Companion Companion { get; set; }
        public Pansion Pansion { get; set; }
        public Store Store { get; set; }
        ICollection<StoryUserLike> StoryUserLikes { get; set; }
    }
}
