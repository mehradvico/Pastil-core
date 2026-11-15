using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Order.RebateSrv.Dto;
using Entities.Entities;
using Entities.Entities.CompanionField;
using Entities.Entities.PansionField;
using System.Threading.Tasks;

namespace Application.Services.Order.RebateSrv.Iface
{
    public interface IRebateService : ICommonSrv<Rebate, RebateDto>
    {
        BaseSearchDto<RebateDto> Search(BaseInputDto baseSearchDto);
        /// <summary>لیست کدهای تخفیف با فیلتر هدف (فروشگاه/...) و کاربر؛ جدیدترین اول.</summary>
        BaseSearchDto<RebateListVDto> SearchByTarget(RebateInputDto input);
        /// <summary>ساخت کد تخفیف سبد خرید که فقط برای همین فروشگاه معتبر است (نوع و هدف سمت سرور ست می‌شود).</summary>
        Task<BaseResultDto<RebateDto>> InsertForStoreAsync(RebateDto dto, long storeId);
        /// <summary>ساخت کد مخصوص یک نمایندگی (Store/Coach/Clinic/Groomer/Pansion)؛ نوع کد از روی هدف تعیین می‌شود.</summary>
        Task<BaseResultDto<RebateDto>> InsertForTargetAsync(RebateDto dto, Entities.Entities.PastilClubField.ClubRewardTargetTypeEnum targetType, long targetId);
        /// <summary>فعال/غیرفعال کردن کد؛ با storeId فقط کدِ همان فروشگاه.</summary>
        Task<BaseResultDto> SetActiveAsync(long id, bool active, long? storeId = null);
        /// <summary>حذف نرم کد؛ با storeId فقط کدِ همان فروشگاه.</summary>
        Task<BaseResultDto> DeleteForStoreAsync(long id, long? storeId);
        BaseResultDto<RebateVDto> GetRebateByCodeAsync(Cart cart, string Code);
        BaseResultDto<RebateVDto> GetRebateByCodeAsync(CompanionReserve companionReserve, string Code);
        BaseResultDto<RebateVDto> GetRebateByCodeAsync(Cargo cargo, string Code);
        BaseResultDto<RebateVDto> GetRebateByCodeAsync(CompanionInsurancePackageSale insurance, string Code);
        BaseResultDto<RebateVDto> GetRebateByCodeAsync(Trip trip, string Code);
        BaseResultDto<RebateVDto> GetRebateByCodeAsync(PansionReserve pansion, string Code);
        /// <summary>کد تخفیف رزرو آرایشگاه؛ پایه‌ی تخفیف مبلغ آنلاین رزرو است و هدف کوپن باشگاه با آرایشگاه/خدمت چک می‌شود.</summary>
        BaseResultDto<RebateVDto> GetRebateByCodeAsync(Entities.Entities.GroomerField.GroomerReserve groomerReserve, long assistanceId, string Code);
        BaseResultDto<RebateVDto> GetRebateByCodeAsync(Entities.Entities.ClinicField.ClinicReserve clinicReserve, long assistanceId, string Code);
        BaseResultDto<RebateVDto> GetRebateByCodeAsync(Entities.Entities.CoachField.CoachReserve coachReserve, long assistanceId, string Code);
        BaseResultDto<RebateVDto> GetRebateByCodeAsync(double basePrice, long userId, string typeLabel, string code, long? targetId = null);
        void IncreaseUseCount(ProductOrder order);
        void IncreaseUseCount(CompanionReserve reserve);
        void IncreaseUseCount(Cargo cargo);
        void IncreaseUseCount(Trip trip);
        void IncreaseUseCount(CompanionInsurancePackageSale insurance);
        void IncreaseUseCount(PansionReserve pansion);
        void IncreaseUseCount(Entities.Entities.GroomerField.GroomerReserve groomerReserve);
        void IncreaseUseCount(Entities.Entities.ClinicField.ClinicReserve clinicReserve);
        void IncreaseUseCount(Entities.Entities.CoachField.CoachReserve coachReserve);
        void IncreaseUseCount(Rebate rebate, long userId, double fundedValue = 0);

    }
}
