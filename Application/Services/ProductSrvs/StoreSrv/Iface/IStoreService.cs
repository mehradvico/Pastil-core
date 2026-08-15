using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CommonSrv.SearchSrv.Dto;
using Application.Services.ProductSrvs.StoreSrv.Dto;
using Entities.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.StoreSrv.Iface
{
    public interface IStoreService : ICommonSrv<Store, StoreDto>
    {
        StoreSearchDto Search(StoreInputDto baseSearchDto);
        Task<BaseResultDto<StoreVDto>> FindAsyncVDto(long id);
        Task<BaseResultDto> UpdateSiteVisibilityAsync(long id, bool showToSite);
        Task SetMaxDiscountAsync(long storeId, int maxDiscount);
        void UpdateStoreCommentCount(long storeId);
        Task UpdateStoreCommentRateAsync(long Id);
        Task<List<SearchStoreDto>> SearchMinAsync(SearchRequestDto request);
        Task<BaseResultDto<StoreVDto>> FindRequestAsync(long id, long userId);
        Task<BaseResultDto<StoreDto>> InsertRequestAsync(StoreDto dto, long userId);
        Task<BaseResultDto> ResubmitRequestAsync(StoreDto dto, long userId);
        Task<BaseResultDto> UpdateApprovalAsync(StoreApprovalDto dto);


    }
}
