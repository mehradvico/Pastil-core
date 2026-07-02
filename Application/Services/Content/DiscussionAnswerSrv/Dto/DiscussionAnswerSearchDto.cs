using Application.Common.Dto.Result;
using Application.Services.Content.DiscussionAnswerSrv.Iface;
using AutoMapper;
using Entities.Entities;
using System.Linq;

namespace Application.Services.Content.DiscussionAnswerSrv.Dto
{
    public class DiscussionAnswerSearchDto : BaseSearchDto<DiscussionAnswer, DiscussionAnswerVDto>, IDiscussionAnswerSearchFields
    {
        public DiscussionAnswerSearchDto(DiscussionAnswerInputDto dto, IQueryable<DiscussionAnswer> list, IMapper mapper) : base(dto, list, mapper)
        {
            this.UserId = dto.UserId;
            this.DiscussionQuestionId = dto.DiscussionQuestionId;

        }
        public long? UserId { get; set; }
        public long? DiscussionQuestionId { get; set; }

    }
}
