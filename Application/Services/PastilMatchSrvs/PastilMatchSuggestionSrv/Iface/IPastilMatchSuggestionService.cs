using Application.Common.Dto.Result;
using Application.Services.PastilMatchSrvs.PastilMatchSuggestionSrv.Dto;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchSuggestionSrv.Iface
{
    public interface IPastilMatchSuggestionService
    {
        Task<BaseResultDto<PastilMatchSuggestionVDto>> FindNextAsync(PastilMatchSuggestionInputDto dto);
    }
}
