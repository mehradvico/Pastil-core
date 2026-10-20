using Application.Common.Dto.Field;
using Application.Services.Dto;

namespace Application.Services.ProviderSrvs.ProviderSrv.Dto
{
    /// <summary>نمای خلاصه‌ی ارائه‌دهنده برای لیست‌هایی مثل تسویه‌ها (نیازمند Include کاربر).</summary>
    public class ProviderMinVDto : Id_FieldDto
    {
        public long UserId { get; set; }
        public bool Active { get; set; }
        public UserMinVDto User { get; set; }
    }
}
