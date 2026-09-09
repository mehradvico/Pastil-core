using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.SchoolSrvs.SchoolCourseSrv.Dto;
using Entities.Entities.SchoolField;
using System.Threading.Tasks;

namespace Application.Services.SchoolSrvs.SchoolCourseSrv.Iface
{
    public interface ISchoolCourseService : ICommonSrv<SchoolCourse, SchoolCourseDto>
    {
        SchoolCourseSearchDto Search(SchoolCourseInputDto baseSearchDto);
        Task<BaseResultDto<SchoolCourseVDto>> FindAsyncVDto(long id);
        Task<BaseResultDto> UpdateActiveAsync(long id, bool active, long? companionId = null);

        Task<BaseResultDto<SchoolCourseSessionDto>> UpsertSessionAsync(SchoolCourseSessionDto dto, long? companionId = null);
        Task<BaseResultDto> DeleteSessionAsync(long id, long? companionId = null);

        Task<BaseResultDto<SchoolCourseVideoDto>> UpsertVideoAsync(SchoolCourseVideoDto dto, long? companionId = null);
        Task<BaseResultDto> DeleteVideoAsync(long id, long? companionId = null);
    }
}
