using Application.Common.Dto.Field;

namespace Application.Services.ProviderSrvs.ProviderRoleSrv.Dto
{
    public class ProviderRoleDto : Name_FieldDto
    {
        /// <summary>کلید ثابت (Store, Clinic, Coach, Groomer, Pansion, School)؛ فقط هنگام ساخت نقش جدید ثبت می‌شود و بعداً تغییر نمی‌کند.</summary>
        public string Label { get; set; }
        public bool Active { get; set; }
    }
}
