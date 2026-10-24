using Entities.Entities.CommonField;

namespace Entities.Entities
{
    public class Assistance : FullName_Field
    {
        public bool IsPersonal { get; set; }
        public long? AssistanceGroupId { get; set; }
        public long? PictureId { get; set; }
        public bool Active { get; set; }
        public bool ShowToSite { get; set; }
        /// <summary>خدمت گرومینگ: فقط برای آرایشگاه‌ها (Groomer)؛ کلینیک/نماینده نه می‌بیند نه می‌سازد.</summary>
        public bool IsGrooming { get; set; }
        /// <summary>
        /// نقشِ مالکِ خدمت (ProviderRole: Clinic/Coach/Groomer/Pansion/School). خدمت فقط برای مراکزِ همین نقش قابل تعریف/نمایش است.
        /// null = دسته‌بندی‌نشده (فقط برای نماینده‌های قدیمی/Companion). IsGrooming هم‌زمان با نقش Groomer نگه داشته می‌شود.
        /// </summary>
        public long? ProviderRoleId { get; set; }
        public bool Deleted { get; set; }

        public AssistanceGroup AssistanceGroup { get; set; }
        public Picture Picture { get; set; }
        public Entities.ProviderField.ProviderRole ProviderRole { get; set; }
    }
}
