using Application.Services.CommonSrv.SearchSrv.Dto;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.CommonSrv.SearchSrv.Iface
{
    public interface ISearchSemanticReranker
    {
        Task<List<SearchItemDto>> RerankAsync(string query, List<SearchItemDto> items, CancellationToken cancellationToken = default);
    }
}
