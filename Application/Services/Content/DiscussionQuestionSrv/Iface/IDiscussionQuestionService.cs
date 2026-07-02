using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Content.DiscussionQuestionSrv.Dto;
using Entities.Entities;
using System.Threading.Tasks;

namespace Application.Services.Content.DiscussionQuestionSrv.Iface
{
    public interface IDiscussionQuestionService : ICommonSrv<DiscussionQuestion, DiscussionQuestionDto>
    {
        DiscussionQuestionSearchDto Search(DiscussionQuestionInputDto baseSearchDto);
        Task<BaseResultDto<DiscussionQuestionVDto>> FindAsyncVDto(long id, bool visit);
        Task<BaseResultDto<DiscussionQuestionVDto>>FindAdminAsyncVDto(long id);
        BaseResultDto UpdateAnswerCountDto(DiscussionQuestionDto dto);
    }
}
