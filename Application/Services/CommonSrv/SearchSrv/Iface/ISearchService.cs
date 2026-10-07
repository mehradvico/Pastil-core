using Application.Common.Dto.Result;
using Application.Services.CommonSrv.SearchSrv.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Application.Services.CommonSrv.SearchSrv.Iface
{
    public interface ISearchService
    {
        public Task<BaseResultDto<SearchDto>> SearchAsync(SearchRequestDto request, CancellationToken cancellationToken = default);

        /// <summary>
        /// پرتکرارترین عبارت‌های جستجوی موفق (با حداقل یک نتیجه) برای یک مبدأ، در چند روز اخیر.
        /// </summary>
        public Task<BaseResultDto<List<string>>> GetPopularAsync(string channel, int days, int take, CancellationToken cancellationToken = default);
        public Task<BaseResultDto<List<string>>> SuggestAsync(string q, int take, CancellationToken cancellationToken = default);
    }
}
