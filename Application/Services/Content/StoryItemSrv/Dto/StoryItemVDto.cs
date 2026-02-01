using Application.Common.Dto.Field;
using Application.Services.CompanionSrvs.CompanionSrv.Dto;
using Application.Services.Content.StoryGroupSrv.Dto;
using Application.Services.Filing.PictureSrv.Dto;
using Application.Services.PansionSrvs.PansionSrv.Dto;
using Entities.Entities;
using Entities.Entities.PansionField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Content.StoryItemSrv.Dto
{
    public class StoryItemVDto : Id_FieldDto
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

        public StoryGroupVDto StoryGroup { get; set; }
        public PictureVDto Picture { get; set; }
        public CompanionMinVDto Companion { get; set; }
        public PansionMinVDto Pansion { get; set; }
        //ICollection<UserStoryLike> UserStoryLikes { get; set; }
    }
}
