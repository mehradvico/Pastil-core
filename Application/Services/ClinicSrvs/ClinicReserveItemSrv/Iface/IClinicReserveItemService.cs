using Application.Common.Dto.Result;
using Application.Services.ClinicSrvs.ClinicReserveItemSrv.Dto;
using System.Threading.Tasks;

namespace Application.Services.ClinicSrvs.ClinicReserveItemSrv.Iface
{
    public interface IClinicReserveItemService
    {
        /// <summary>فهرست بسته‌های رزرو برای خود رزروکننده.</summary>
        Task<BaseResultDto<ClinicReserveItemsVDto>> GetForBookerAsync(long reserveId, long bookerId);

        /// <summary>فهرست بسته‌ها برای کلینیک (مالک/پرسنل مجاز) یا ادمین.</summary>
        Task<BaseResultDto<ClinicReserveItemsVDto>> GetForManagerAsync(long reserveId, long actorUserId, bool asAdmin);

        /// <summary>تأیید یا لغوِ تکی یک بسته؛ لغو، سهم پرداختی را به کیف پول کاربر برمی‌گرداند (آخرین بسته = لغو کل رزرو).</summary>
        Task<BaseResultDto<ClinicReserveItemStatusResultVDto>> SetStatusAsync(long reserveId, long packageId, ClinicReserveItemStatusDto dto, long actorUserId, bool asAdmin);

        /// <summary>افزودن یک بسته به رزروِ پرداخت‌شده (حین ویزیت)؛ مبلغش حضوری پرداخت می‌شود.</summary>
        Task<BaseResultDto<ClinicReserveItemsVDto>> AddAsync(long reserveId, ClinicReserveItemAddDto dto, long actorUserId, bool asAdmin);
    }
}
