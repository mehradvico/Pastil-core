using Application.Services.Filing.PictureSrv.Dto;

using Application.Services.LocationFields.UserCurrentLocationSrv.Dto;

namespace Application.Services.Dto
{
    public class CurrentUserDto
    {
        public long UserId { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public long RoleId { get; set; }
        public long? PictureId { get; set; }
        public string RoleName { get; set; }
        public string RoleEnum { get; set; }
        public string ClickGuid { get; set; }
        public long? CompanionId { get; set; }
        public long DriverId { get; set; }
        public long StoreId { get; set; }
        public bool IsFemale { get; set; }
        public string Expertise { get; set; }
        public bool IsCompanionUser { get; set; }
        public bool CanSetExpertise { get; set; }

        // «کار به‌جای ارائه‌دهنده» (ProviderUser، ۲۰۲۶-۱۱-۰۳): وقتی کارمند پنل با هدر X-Provider-Id و دسترسیِ همان endpoint درخواست می‌دهد،
        // ProviderPanelAccessFilter هویت این درخواست را با مالک ارائه‌دهنده جایگزین می‌کند تا همان چک‌های مالکیتِ موجود داده را محدود کنند.
        // این سه فیلد فقط در همان حالت پر هستند و کاربر واقعی (کارمند) را نگه می‌دارند؛ کار مالک‌محور (نقش/دسترسی) با ActorUserId چک می‌شود.
        public long? ActingProviderId { get; set; }
        public long? ActingProviderUserId { get; set; }
        public long? ActorUserId { get; set; }
        public bool IsActingForProvider => ActingProviderId.HasValue;

        public double WalletAmount { get; set; }
        public PictureVDto Picture { get; set; }
        public UserCurrentLocationVDto UserCurrentLocation { get; set; }

    }
}
