using Application.Common.Dto.Result;
using Application.Services.ConsultationSrvs.ConsultationSessionSrv.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.ConsultationSrvs.ConsultationSessionSrv.Iface
{
    public interface IConsultationSessionService
    {
        // خریدهای «پرداخت‌شده/در جریان» کلینیک‌هایی که این کاربر نماینده‌ی مجازشان است (مالک یا عضو تخصیص‌یافته روی خدمت ۱۵)
        Task<BaseResultDto<List<ConsultationAgentItemVDto>>> GetForAgentAsync(long agentUserId);

        // «شروع» مشاوره توسط نماینده: Paid → Active (اتمی)، پنجره‌ی زمانی از همین لحظه، ساخت جلسه‌ی آنلاین
        Task<BaseResultDto<ConsultationSessionInfoVDto>> StartAsync(long agentUserId, long purchaseId);

        // ورود دوباره‌ی نماینده‌ی شروع‌کننده (یا مالک) به مشاوره‌ی جاری در پنجره
        Task<BaseResultDto<ConsultationSessionInfoVDto>> EnterAsync(long agentUserId, long purchaseId);

        // «تکمیل» زودهنگام مشاوره‌ی در جریان توسط نماینده‌ی شروع‌کننده (یا مالک): Active → Completed (اتمی) و بستن جلسه‌ی آنلاین.
        // بدون این، وضعیت فقط با job پایان پنجره «پایان‌یافته» می‌شد. data = اطلاعات جلسه (برای بستن تماس همان لحظه)
        Task<BaseResultDto<ConsultationSessionInfoVDto>> CompleteAsync(long agentUserId, long purchaseId);

        // تخصیص مشاوره‌ی «پرداخت‌شده/شروع‌نشده» به یک نماینده‌ی مشخص (فقط مالک کلینیک). targetUserId = null ⇒ برداشتن تخصیص.
        // نماینده‌ی تخصیص‌یافته پوش می‌گیرد و فقط او (و مالک) می‌تواند شروع کند
        Task<BaseResultDto<bool>> AssignAsync(long ownerUserId, long purchaseId, long? targetUserId);

        // پنجره‌های فعال (شروع‌شده و منقضی‌نشده) کاربر برای نوار بازگشت
        Task<BaseResultDto<List<ConsultationActiveWindowVDto>>> GetActiveWindowsAsync(long userId);

        // پایان پنجره: Active با ExpireDate گذشته ⇒ Completed و بستن جلسه (job زمان‌بندی‌شده)؛ تعداد را برمی‌گرداند
        Task<int> CompleteExpiredAsync();
    }
}
