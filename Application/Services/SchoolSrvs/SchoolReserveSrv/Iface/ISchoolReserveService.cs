using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.SchoolSrvs.SchoolReserveSrv.Dto;
using Entities.Entities.SchoolField;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.SchoolSrvs.SchoolReserveSrv.Iface
{
    public interface ISchoolReserveService : ICommonSrv<SchoolReserve, SchoolReserveDto>
    {
        SchoolReserveSearchDto Search(SchoolReserveInputDto baseSearchDto);
        Task<BaseResultDto<SchoolReserveVDto>> FindAsyncVDto(long id);
        Task<BaseResultDto> UpdateCancelDto(SchoolReserveCancelDto dto);
        Task<BaseResultDto> UpdateStatusDto(SchoolReserveStatusDto dto);
        Task<BaseResultDto> CompleteByCompanionAsync(long id, long companionId);
        Task<int> GetRemainingCapacityAsync(long schoolCourseId);
        Task<BaseResultDto> SchoolReservePaymentCallback(long? reserveId, bool fromWallet = false);
        Task<BaseResultDto> SetRebateCodeAsyncDto(SchoolReserveRebateCodeDto dto);
        Task<BaseResultDto> ClearRebateCodeAsync(long id);
        Task<BaseResultDto> SetWalletAsyncDto(SchoolReserveWalletDto dto);

        // Hangfire recurring job - هر چند دقیقه یک‌بار جلسات زنده‌ی نزدیک به زمان شروع را پیدا
        // و برای همه‌ی ثبت‌نام‌کنندگان همان دوره پوش «کلاس شروع شد» می‌فرستد.
        Task SendClassStartingPushesAsync(CancellationToken cancellationToken);
    }
}
