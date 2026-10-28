using Application.Services.Filing.PictureSrv.Dto;
using System.Collections.Generic;

namespace Application.Services.ClinicSrvs.ClinicProfileSrv.Dto
{
    // ---- پرسنل
    public class ClinicStaffAddDto
    {
        public long ClinicId { get; set; }
        /// <summary>اختیاری: نقش/تخصص‌های عضو (شناسه‌های GET api/Expertise؛ حداکثر ۱۰)</summary>
        public List<long> ExpertiseIds { get; set; } = new();
        /// <summary>کاربر باید قبلاً در پاستیل ثبت‌نام کرده باشد.</summary>
        public string Mobile { get; set; }
    }

    /// <summary>تعیین نقش/تخصص‌های یک عضو تیم (PUT …/ClinicUser/Expertise)؛ لیست خالی = پاک کردن</summary>
    public class ClinicStaffExpertiseDto
    {
        public long Id { get; set; }
        public List<long> ExpertiseIds { get; set; } = new();
    }

    public class ClinicStaffVDto
    {
        // نقش/تخصص‌ها (۲۰۲۶-۱۰-۲۸): ExpertiseId = اصلی؛ ExpertiseName = متن آماده‌ی نمایش («دامپزشک، جراح»)
        public long? ExpertiseId { get; set; }
        public List<Application.Services.CompanionSrvs.ExpertiseSrv.Dto.ExpertiseVDto> Expertises { get; set; } = new();
        public string ExpertiseName { get; set; }
        public long Id { get; set; }
        public long ClinicId { get; set; }
        public long UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Mobile { get; set; }
        public bool Active { get; set; }
        public string ActivationValue { get; set; }
        /// <summary>null = دعوت منتظر پاسخ، true = پذیرفته، false = ردشده.</summary>
        public bool? UserAccept { get; set; }
        public System.DateTime? InviteDate { get; set; }
    }

    /// <summary>دعوت‌نامه‌ی عضویت در پرسنل یک کلینیک، از دید کاربرِ دعوت‌شده.</summary>
    public class ClinicStaffInviteVDto
    {
        public long Id { get; set; }
        public long ClinicId { get; set; }
        public string ClinicName { get; set; }
        public string ClinicPhone { get; set; }
        public System.DateTime? InviteDate { get; set; }
    }

    // ---- ساعت کاری
    public class ClinicTimeInputDto
    {
        public long WeekDayId { get; set; }
        /// <summary>HH:mm</summary>
        public string StartTime { get; set; }
        /// <summary>HH:mm</summary>
        public string EndTime { get; set; }
        public int Capacity { get; set; }
        public bool Active { get; set; } = true;
    }

    public class ClinicTimeVDto : ClinicTimeInputDto
    {
        public long Id { get; set; }
        public string WeekDayName { get; set; }
    }

    // ---- محدوده‌ی پوشش
    public class ClinicZoneInputDto
    {
        public long StateId { get; set; }
        public long CityId { get; set; }
        public long? NeighborhoodId { get; set; }
    }

    public class ClinicZoneVDto : ClinicZoneInputDto
    {
        public long Id { get; set; }
        public string StateName { get; set; }
        public string CityName { get; set; }
        public string NeighborhoodName { get; set; }
    }

    // ---- گالری
    public class ClinicPictureAddDto
    {
        public long ClinicId { get; set; }
        public long PictureId { get; set; }
        public string Label { get; set; }
    }

    public class ClinicPictureVDto
    {
        public long Id { get; set; }
        public long ClinicId { get; set; }
        public long PictureId { get; set; }
        public string Label { get; set; }
        public PictureVDto Picture { get; set; }
    }

    public class ClinicProfileSetDto<T>
    {
        public long ClinicId { get; set; }
        public List<T> Items { get; set; } = new List<T>();
    }
}
