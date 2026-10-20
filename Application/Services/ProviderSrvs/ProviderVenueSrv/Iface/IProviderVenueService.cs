using Application.Common.Dto.Result;
using Application.Services.ProviderSrvs.ProviderVenueSrv.Dto;
using System.Threading.Tasks;

namespace Application.Services.ProviderSrvs.ProviderVenueSrv.Iface
{
    /// <summary>نمایندگی‌های (نماینده/فروشگاه) یک ارائه‌دهنده و اتصال دستی آن‌ها توسط ادمین.</summary>
    public interface IProviderVenueService
    {
        Task<BaseResultDto<ProviderVenuesVDto>> GetVenuesAsync(long providerId);
        Task<BaseResultDto> AssignCompanionAsync(ProviderVenueAssignDto dto);
        Task<BaseResultDto> AssignStoreAsync(ProviderVenueAssignDto dto);
        Task<BaseResultDto> AssignGroomerAsync(ProviderVenueAssignDto dto);
        Task<BaseResultDto> AssignClinicAsync(ProviderVenueAssignDto dto);
        Task<BaseResultDto> AssignCoachAsync(ProviderVenueAssignDto dto);
    }
}
