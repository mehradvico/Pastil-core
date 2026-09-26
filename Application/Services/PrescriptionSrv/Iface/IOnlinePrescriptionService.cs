using Application.Common.Dto.Result;
using Application.Services.PrescriptionSrv.Dto;
using System.Threading.Tasks;

namespace Application.Services.PrescriptionSrv.Iface
{
    public interface IOnlinePrescriptionService
    {
        // نماینده/همکار: ثبت یا ویرایش نسخه (upsert)
        Task<BaseResultDto<PrescriptionVDto>> UpsertAsync(long userId, PrescriptionUpsertDto dto);
        // نماینده/همکار: خواندن نسخه‌ی یک هدف (data = null یعنی هنوز نسخه‌ای ثبت نشده)
        Task<BaseResultDto<PrescriptionVDto>> GetForCompanionAsync(long userId, PrescriptionTargetDto target);
        // کاربر رزروکننده/خریدار: فقط خواندن نسخه‌ی خودش
        Task<BaseResultDto<PrescriptionVDto>> GetForBookerAsync(long userId, PrescriptionTargetDto target);
    }
}
