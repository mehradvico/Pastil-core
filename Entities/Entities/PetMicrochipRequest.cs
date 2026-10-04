using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;

namespace Entities.Entities
{
    // درخواست «جستجوی پت با میکروچیپ» (بخش ارتباط با ما). هر جستجو یک ردیف می‌سازد (حتی اگر پتی پیدا نشود: برای ممیزی/سوءاستفاده)؛
    // اگر پت پیدا شد، کاربر می‌تواند «درخواست پیگیری توسط پاستیل» بدهد و همان ردیف به صف ادمین می‌رود.
    // مشخصات تماس مالک پت هرگز به جستجوگر داده نمی‌شود؛ فقط ادمین می‌بیند. طراحی: backend/Docs/PET_MICROCHIP_LOOKUP_FA.md
    public class PetMicrochipRequest : Id_Field
    {
        // فقط رقم‌ها (بعد از نرمال‌سازی)
        public string MicrochipCode { get; set; }
        // توکن تصادفی که فقط در پاسخ جستجو برمی‌گردد؛ بدون آن نمی‌شود برای این ردیف «پیگیری» ثبت کرد
        public string PublicToken { get; set; }
        public long? UserId { get; set; }
        public string ClientIp { get; set; }
        // پتی که با این کد پیدا شد (اگر چند پت یک کد داشتند جدیدترین)؛ null = پیدا نشد
        public long? FoundUserPetId { get; set; }
        public int MatchCount { get; set; }
        public DateTime CreateDate { get; set; }

        // ---- درخواست پیگیری (اطلاعات تماس «جستجوگر»)
        public bool FollowUpRequested { get; set; }
        public DateTime? FollowUpDate { get; set; }
        public string FullName { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }

        // ---- رسیدگی ادمین (PetMicrochipRequestStatusEnum)
        public int Status { get; set; }
        public string AdminNote { get; set; }
        public long? HandledByUserId { get; set; }
        public DateTime? ClosedDate { get; set; }

        public User User { get; set; }
        public UserPet FoundUserPet { get; set; }
    }
}
