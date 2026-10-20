using Application.Services.Filing.PictureSrv.Dto;
using System.Collections.Generic;

namespace Application.Services.GroomerSrvs.GroomerProfileSrv.Dto
{
    // ---- پرسنل
    public class GroomerStaffAddDto
    {
        public long GroomerId { get; set; }
        /// <summary>کاربر باید قبلاً در پاستیل ثبت‌نام کرده باشد.</summary>
        public string Mobile { get; set; }
    }

    public class GroomerStaffVDto
    {
        public long Id { get; set; }
        public long GroomerId { get; set; }
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

    /// <summary>دعوت‌نامه‌ی عضویت در پرسنل یک آرایشگاه، از دید کاربرِ دعوت‌شده.</summary>
    public class GroomerStaffInviteVDto
    {
        public long Id { get; set; }
        public long GroomerId { get; set; }
        public string GroomerName { get; set; }
        public string GroomerPhone { get; set; }
        public System.DateTime? InviteDate { get; set; }
    }

    // ---- ساعت کاری
    public class GroomerTimeInputDto
    {
        public long WeekDayId { get; set; }
        /// <summary>HH:mm</summary>
        public string StartTime { get; set; }
        /// <summary>HH:mm</summary>
        public string EndTime { get; set; }
        public int Capacity { get; set; }
        public bool Active { get; set; } = true;
    }

    public class GroomerTimeVDto : GroomerTimeInputDto
    {
        public long Id { get; set; }
        public string WeekDayName { get; set; }
    }

    // ---- محدوده‌ی پوشش
    public class GroomerZoneInputDto
    {
        public long StateId { get; set; }
        public long CityId { get; set; }
        public long? NeighborhoodId { get; set; }
    }

    public class GroomerZoneVDto : GroomerZoneInputDto
    {
        public long Id { get; set; }
        public string StateName { get; set; }
        public string CityName { get; set; }
        public string NeighborhoodName { get; set; }
    }

    // ---- گالری
    public class GroomerPictureAddDto
    {
        public long GroomerId { get; set; }
        public long PictureId { get; set; }
        public string Label { get; set; }
    }

    public class GroomerPictureVDto
    {
        public long Id { get; set; }
        public long GroomerId { get; set; }
        public long PictureId { get; set; }
        public string Label { get; set; }
        public PictureVDto Picture { get; set; }
    }

    public class GroomerProfileSetDto<T>
    {
        public long GroomerId { get; set; }
        public List<T> Items { get; set; } = new List<T>();
    }
}
