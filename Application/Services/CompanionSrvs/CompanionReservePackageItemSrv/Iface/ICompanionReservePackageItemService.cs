using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.CompanionReservePackageItemSrv.Dto;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.CompanionReservePackageItemSrv.Iface
{
    public interface ICompanionReservePackageItemService
    {
        /// <summary>نمای پکیج‌ها و وضعیتشان (بدون بررسی مالکیت؛ فراخوان باید دسترسی را تأیید کرده باشد)</summary>
        Task<CompanionReservePackageItemsVDto> GetViewAsync(long reserveId);

        Task<BaseResultDto<CompanionReservePackageItemsVDto>> GetForBookerAsync(long reserveId, long bookerId);
        Task<BaseResultDto<CompanionReservePackageItemsVDto>> GetForManagerAsync(long reserveId, long actorUserId, bool asAdmin);

        Task<BaseResultDto<CompanionReservePackageStatusResultVDto>> SetStatusAsync(long reserveId, long packageId, CompanionReservePackageStatusDto dto, long actorUserId, bool asAdmin);
        Task<BaseResultDto<CompanionReservePackageItemsVDto>> AddAsync(long reserveId, CompanionReservePackageAddDto dto, long actorUserId, bool asAdmin);
        Task<BaseResultDto<CompanionReservePackageItemsVDto>> RemoveUnpaidAsync(long reserveId, long packageId, long actorUserId, bool asAdmin);
    }
}
