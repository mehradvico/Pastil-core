using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.SchoolSrvs.SchoolCourseSrv.Dto;
using Application.Services.CommonSrv.SearchSrv.Dto;
using Entities.Entities.SchoolField;
using System.Collections.Generic;
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

        Task<List<SearchSchoolCourseDto>> SearchMinAsync(SearchRequestDto request);

        // فایل درس (ویدیوی دوره): فقط ادمین، نماینده‌ی صاحب مدرسه، یا ثبت‌نام‌شده‌ی پرداخت‌کرده؛ فایل غیردرسی همیشه true
        Task<bool> CanReadLessonFileAsync(long fileId, long userId, long? companionId, bool isAdmin);
    }
}
