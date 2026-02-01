using Application.Common.Dto.Field;
using Entities.Entities;
using Entities.Entities.PansionField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Content.StoryItemSrv.Dto
{
    public class StoryItemDto : Id_FieldDto
    {

        public string Url { get; set; }
        public long? CompanionId { get; set; }
        public long? StoreId { get; set; }
        public long? PansionId { get; set; }
        public int Priority { get; set; }
        public long StoryGroupId { get; set; }
        public long PictureId { get; set; }
        public bool Active { get; set; }
    }
}
