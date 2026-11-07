using System;
using System.Collections.Generic;

namespace Application.Services.ProviderSrvs.ProviderUserSrv.Dto
{
    /// <summary>یک دسترسی قابل انتخاب در کاتالوگ (برای فرم ساخت نقش).</summary>
    public class ProviderPermissionVDto
    {
        public string Key { get; set; }
        public string Level { get; set; }
        public string Name { get; set; }
    }

    public class ProviderPermissionSectionVDto
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public List<ProviderPermissionVDto> Permissions { get; set; } = new();
    }

    /// <summary>دسترسیِ مادر = یک نوع نمایندگی (فروشگاه، کلینیک، ...)؛ فقط نمایندگی‌هایی که ارائه‌دهنده دارد برمی‌گردند.</summary>
    public class ProviderPermissionScopeVDto
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public List<ProviderPermissionSectionVDto> Sections { get; set; } = new();
    }

    public class ProviderUserRoleInputDto
    {
        /// <summary>۰ = نقش جدید</summary>
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> PermissionKeys { get; set; } = new();
    }

    public class ProviderUserRoleVDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> PermissionKeys { get; set; } = new();
        public int UserCount { get; set; }
        public DateTime CreateDate { get; set; }
        /// <summary>نقش آماده‌ی ادمین: مالک فقط می‌تواند آن را به کارمند بدهد، ویرایش/حذفش نمی‌کند.</summary>
        public bool IsTemplate { get; set; }
        /// <summary>فقط نقش آماده: نمایندگی (Store/Clinic/...) و نام فارسی آن.</summary>
        public string Scope { get; set; }
        public string ScopeName { get; set; }
        public int Priority { get; set; }
    }

    /// <summary>ورودی ادمین برای نقش آماده (قالب سراسری یک نمایندگی).</summary>
    public class ProviderUserRoleTemplateInputDto
    {
        /// <summary>۰ = نقش جدید</summary>
        public long Id { get; set; }
        public string Scope { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Priority { get; set; }
        public List<string> PermissionKeys { get; set; } = new();
    }

    public class ProviderUserInviteDto
    {
        public string Mobile { get; set; }
        public long ProviderUserRoleId { get; set; }
    }

    public class ProviderUserRoleChangeDto
    {
        public long Id { get; set; }
        public long ProviderUserRoleId { get; set; }
    }

    public class ProviderUserVDto
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Mobile { get; set; }
        public long ProviderUserRoleId { get; set; }
        public string RoleName { get; set; }
        public bool? UserAccept { get; set; }
        public DateTime? InviteDate { get; set; }
        public bool Active { get; set; }
    }

    /// <summary>دعوت‌نامه‌ی منتظر پاسخ برای خود کاربر.</summary>
    public class ProviderUserInviteVDto
    {
        public long Id { get; set; }
        public long ProviderId { get; set; }
        public string ProviderName { get; set; }
        public string RoleName { get; set; }
        public DateTime? InviteDate { get; set; }
    }

    /// <summary>
    /// ارائه‌دهنده‌ای که کاربر کارمند فعال آن است + کلیدهای دسترسی‌اش. کلاینت منوی پنل را فقط از روی همین Permissions/Scopes می‌سازد
    /// (بخشی که در لیست نیست نباید نمایش داده شود) و برای هر درخواست پنل هدر X-Provider-Id = ProviderId می‌فرستد.
    /// </summary>
    public class ProviderMembershipVDto
    {
        public long ProviderUserId { get; set; }
        public long ProviderId { get; set; }
        public string ProviderName { get; set; }
        public long ProviderUserRoleId { get; set; }
        public string RoleName { get; set; }
        public List<string> Permissions { get; set; } = new();
        public List<string> Scopes { get; set; } = new();
        /// <summary>
        /// فروشگاهی که درخواست‌های Seller این کارمند (با هدر X-Provider-Id) روی آن اجرا می‌شوند؛ فقط اگر دسترسی فروشگاه دارد، وگرنه null.
        /// معادل بی‌شناسه: GET api/Seller/Store/Current.
        /// </summary>
        public long? StoreId { get; set; }
    }

    /// <summary>نتیجه‌ی اعتبارسنجی «کار کردن به‌جای ارائه‌دهنده» برای یک درخواست پنل.</summary>
    public sealed class ProviderActingAccess
    {
        public long ProviderId { get; init; }
        public long ProviderUserId { get; init; }
        public long OwnerUserId { get; init; }
        public long ProviderUserRoleId { get; init; }
        public IReadOnlyCollection<string> Permissions { get; init; } = Array.Empty<string>();
    }
}
