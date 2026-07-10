using Application.Common.Dto.Field;
using Application.Services.Content.StoryItemSrv.Dto;
using Application.Services.Filing.PictureSrv.Dto;
using Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Content.StoryGroupSrv.Dto
{
    public class StoryGroupVDto : Name_FieldDto
    {
        public int Priority { get; set; }
        public long PictureId { get; set; }
        public bool Active { get; set; }

        public PictureVDto Picture { get; set; }
    }
}
