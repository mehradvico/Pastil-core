using Application.Common.Dto.Result;
using Application.Services.ConsultationSrvs.ConsultationBookingSrv.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.ConsultationSrvs.ConsultationBookingSrv.Iface
{
    // بازه‌های کاری و اسلات‌های رزرو مشاوره (طراحی: backend/Docs/ONLINE_CONSULTATION_BOOKING_FA.md)
    public interface IConsultationBookingService
    {
        // برنامه‌ی هفتگی کلینیک من (مالک)
        Task<BaseResultDto<List<ConsultationAvailabilityWindowDto>>> GetAvailabilityAsync(long companionId);

        // جایگزینی کل برنامه‌ی هفتگی؛ لیست خالی = هیچ بازه‌ای (رزرو بسته می‌شود). رزروهای قبلی دست نمی‌خورند.
        Task<BaseResultDto<List<ConsultationAvailabilityWindowDto>>> SaveAvailabilityAsync(long companionId, List<ConsultationAvailabilityWindowDto> windows);

        // اسلات‌های یک روز برای یک پکیج قابل‌رزرو (کاربر)
        Task<BaseResultDto<ConsultationSlotsVDto>> GetSlotsAsync(long packageId, DateTime date);

        // خلاصه‌ی روزهای قابل رزرو (۱۴ روز آینده) با تعداد اسلات خالی، برای انتخابگر تاریخ
        Task<BaseResultDto<ConsultationBookingDaysVDto>> GetDaysAsync(long packageId);

        // بررسی نهایی یک اسلات هنگام خرید (داخل تراکنش Serializable صدا زده می‌شود). excludePurchaseId برای خودِ خرید در حال ساخت.
        Task<ConsultationBookingRules.SlotProblem> CheckSlotAsync(long companionId, int durationMinutes, DateTime start, DateTime now);

        // آیا کاربر در همین ساعت رزرو/مشاوره‌ی دیگری دارد (با هر کلینیک)
        Task<bool> UserHasOverlapAsync(long userId, DateTime start, DateTime end, DateTime now);
    }
}
