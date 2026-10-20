using Application.Common.Dto.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Application.Services.TripSrv.PetResanServiceSrv.Dto
{
    /// <summary>
    /// جستجوی سرویس‌های هفتگی در پنل. Available = فقط فعال‌ها (true) / فقط متوقف‌شده‌ها (false) / همه (null)؛
    /// Q روی نام، موبایل و آدرس‌ها.
    /// </summary>
    public class PetResanServiceAdminSearchDto : BaseInputDto
    {
    }

    public class PetResanServiceAdminVDto
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string UserFullName { get; set; }
        public string UserMobile { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? TotalWeeks { get; set; }
        public bool Active { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? CancelDate { get; set; }
        public double PricePerOccurrence { get; set; }
        public List<PetResanServiceAdminScheduleVDto> Schedules { get; set; } = new List<PetResanServiceAdminScheduleVDto>();
        /// <summary>نوبت‌های «درخواست‌شده» (منتظر راننده)</summary>
        public int RequestedOccurrences { get; set; }
        /// <summary>نوبت‌های آینده‌ی پذیرفته‌شده توسط راننده</summary>
        public int AcceptedUpcomingOccurrences { get; set; }
        public int CompletedOccurrences { get; set; }
        public int CanceledOccurrences { get; set; }
    }

    public class PetResanServiceAdminScheduleVDto
    {
        public int WeekDayNumber { get; set; }
        public string WeekDayName { get; set; }
        public string Time { get; set; }
        public string ReturnTime { get; set; }
    }

    /// <summary>لغو گروهی سرویس‌های هفتگی از پنل (هرکدام: غیرفعال + لغو نوبت‌های شروع‌نشده با برگشت پول).</summary>
    public class PetResanServiceBulkCancelDto
    {
        [Required]
        [MinLength(1)]
        [MaxLength(500)]
        public List<long> Ids { get; set; } = new List<long>();

        [MaxLength(500)]
        public string Detail { get; set; }
    }

    public class PetResanServiceBulkCancelResultDto
    {
        /// <summary>سرویس‌هایی که متوقف شدند (شامل آن‌هایی که از قبل غیرفعال بودند)</summary>
        public int Services { get; set; }
        /// <summary>نوبت‌های شروع‌نشده‌ای که لغو شدند</summary>
        public int Occurrences { get; set; }
        public int NotFound { get; set; }
    }
}
