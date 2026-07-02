using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Content.DiscussionAnswerSrv.Dto;
using Entities.Entities;
using System.Threading.Tasks;

namespace Application.Services.Content.DiscussionAnswerSrv.Iface
{
    public interface IDiscussionAnswerService : ICommonSrv<DiscussionAnswer, DiscussionAnswerDto>
    {
        DiscussionAnswerSearchDto Search(DiscussionAnswerInputDto baseSearchDto);
        Task<BaseResultDto<DiscussionAnswerVDto>> FindAsyncVDto(long id);
        BaseResultDto DiscussionAnswerActivation(DiscussionAnswerActiveDto dto);

    }
}
