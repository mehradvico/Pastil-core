using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.SchoolSrvs.SchoolCoursePetSrv.Dto;
using Entities.Entities.SchoolField;
using System.Threading.Tasks;

namespace Application.Services.SchoolSrvs.SchoolCoursePetSrv.Iface
{
    public interface ISchoolCoursePetService : ICommonSrv<SchoolCoursePet, SchoolCoursePetDto>
    {
        SchoolCoursePetSearchDto Search(SchoolCoursePetInputDto baseSearchDto);
        Task<BaseResultDto<SchoolCoursePetVDto>> FindAsyncVDto(long id);
    }
}
