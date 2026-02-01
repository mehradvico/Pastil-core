using Application.Common.Dto.Field;
using Entities.Entities;
using Entities.Entities.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Content.StoryUserLikeSrv.Dto
{
    public class StoryUserLikeDto : Id_FieldDto
    {
        public long StoryItemId { get; set; }
        public long UserId { get; set; }
    }
}
