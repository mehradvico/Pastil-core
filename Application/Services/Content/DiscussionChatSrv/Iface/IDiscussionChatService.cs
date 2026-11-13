using Application.Common.Dto.Result;
using Application.Services.Content.DiscussionAnswerSrv.Dto;
using Application.Services.Content.DiscussionChatSrv.Dto;
using Application.Services.Content.DiscussionQuestionSrv.Dto;
using System.Threading.Tasks;

namespace Application.Services.Content.DiscussionChatSrv.Iface
{
    public interface IDiscussionChatService
    {
        Task<BaseResultDto<DiscussionQuestionDto>> SubmitQuestionAsync(long userId, DiscussionQuestionDto dto);
        Task<BaseResultDto<DiscussionAnswerDto>> SubmitAnswerAsync(long userId, DiscussionAnswerDto dto);
        Task<BaseResultDto> EditAsync(long userId, DiscussionChatEditDto dto);
        Task<BaseResultDto> DeleteAsync(long userId, DiscussionChatKind kind, long id);
        Task<DiscussionMyStatusVDto> MyStatusAsync(long userId, long productId);
    }
}
