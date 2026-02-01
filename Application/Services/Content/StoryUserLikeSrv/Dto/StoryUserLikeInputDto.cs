using Application.Common.Dto.Input;
using Application.Services.Content.StoryUserLikeSrv.Iface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Content.StoryUserLikeSrv.Dto
{
    public class StoryUserLikeInputDto : BaseInputDto, IStoryUserLikeSearchFields
    {
        public long UserId { get; set; }
    }
}
