using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.SchoolSrvs.SchoolSrv.Dto;
using Entities.Entities.SchoolField;
using System.Threading.Tasks;

namespace Application.Services.SchoolSrvs.SchoolSrv.Iface
{
    public interface ISchoolService : ICommonSrv<School, SchoolDto>
    {
        SchoolSearchDto Search(SchoolInputDto baseSearchDto);
        Task<BaseResultDto<SchoolVDto>> FindAsyncVDto(long id);
        BaseResultDto UpdateSchoolActiveDto(SchoolActiveDto dto, long? companionId = null);
        Task<BaseResultDto> UpdateSchoolApproveAsyncDto(SchoolApproveDto dto);
    }
}
