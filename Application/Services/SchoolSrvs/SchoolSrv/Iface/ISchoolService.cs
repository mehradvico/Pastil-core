using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.SchoolSrvs.SchoolSrv.Dto;
using Application.Services.CommonSrv.SearchSrv.Dto;
using Entities.Entities.SchoolField;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.SchoolSrvs.SchoolSrv.Iface
{
    public interface ISchoolService : ICommonSrv<School, SchoolDto>
    {
        SchoolSearchDto Search(SchoolInputDto baseSearchDto, bool onlyDeleted = false);
        Task<BaseResultDto<SchoolVDto>> FindAsyncVDto(long id, bool includeDeleted = false);
        // حذف نرم (companionId = مالک کلینیک؛ null = ادمین) و بازگردانی (ادمین)
        Task<BaseResultDto> SoftDeleteAsync(long id, long? companionId, long actorUserId);
        Task<BaseResultDto> RestoreAsync(long id);
        BaseResultDto UpdateSchoolActiveDto(SchoolActiveDto dto, long? companionId = null);
        Task<BaseResultDto> UpdateSchoolApproveAsyncDto(SchoolApproveDto dto);
        Task<BaseResultDto> UpdateSiteVisibilityAsync(long id, bool showToSite);
        Task<List<SearchSchoolDto>> SearchMinAsync(SearchRequestDto request);
    }
}
