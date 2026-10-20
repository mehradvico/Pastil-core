using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.ProviderSrvs.ProviderSrv.Dto;
using Entities.Entities.ProviderField;
using System.Threading.Tasks;

namespace Application.Services.ProviderSrvs.ProviderSrv.Iface
{
    public interface IProviderService : ICommonSrv<Provider, ProviderDto>
    {
        ProviderSearchDto Search(ProviderInputDto input);
        Task<BaseResultDto<ProviderVDto>> FindAsyncVDto(long id);
        /// <summary>ارائه‌دهنده‌ی (غیرحذف‌شده‌ی) یک کاربر؛ برای چک مالکیت در سرویس‌های دیگر.</summary>
        Task<long?> GetProviderIdByUserAsync(long userId);
        /// <summary>
        /// ارائه‌دهنده‌ی کاربر را برمی‌گرداند و اگر نبود می‌سازد (بدون SaveChanges؛ ذخیره با فراخواننده است تا هم‌زمان با ثبت نمایندگی انجام شود).
        /// </summary>
        Task<Provider> EnsureForUserAsync(long userId, bool approved);
    }
}
