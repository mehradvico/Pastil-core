using System;
using System.Collections.Generic;

namespace Application.Services.CompanionSrvs.CompanionReserveSrv.Dto
{
    // تغییر زمان رزرو توسط نماینده یا ادمین. NewDoDate فقط «روز» است (ساعت از NewCompanionTimeId می‌آید).
    public class CompanionReserveRescheduleDto
    {
        public long Id { get; set; }
        public DateTime NewDoDate { get; set; }
        public long NewCompanionTimeId { get; set; }
        /// <summary>دلیل تغییر؛ الزامی و در پیامک کاربر می‌آید.</summary>
        public string Reason { get; set; }
    }

    // یک بازه‌ی ساعتیِ قابل انتخاب در یک روز مشخص (برای انتخاب زمان جدید)
    public class CompanionReserveRescheduleSlotVDto
    {
        public long CompanionTimeId { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int Capacity { get; set; }
        public int RemainingCapacity { get; set; }
        /// <summary>اگر false باشد (ظرفیت پر، گذشته یا تداخل با نیروی تخصیص‌یافته) انتخاب نمی‌شود؛ دلیلش در Reason.</summary>
        public bool Selectable { get; set; }
        public string Reason { get; set; }
        public bool IsCurrent { get; set; }
    }

    public class CompanionReserveRescheduleSlotsVDto
    {
        public DateTime Date { get; set; }
        public List<CompanionReserveRescheduleSlotVDto> Slots { get; set; } = new();
    }

    public class CompanionReserveRescheduleVDto
    {
        public long Id { get; set; }
        public long CompanionReserveId { get; set; }
        public DateTime OldDoDate { get; set; }
        public long? OldCompanionTimeId { get; set; }
        public DateTime? OldStartAt { get; set; }
        public DateTime NewDoDate { get; set; }
        public long? NewCompanionTimeId { get; set; }
        public DateTime NewStartAt { get; set; }
        public string Reason { get; set; }
        /// <summary>1 نماینده، 2 ادمین</summary>
        public int ActorKind { get; set; }
        public long ActorUserId { get; set; }
        public string ActorName { get; set; }
        public long? LinkedTripId { get; set; }
        public bool UserNotified { get; set; }
        public bool DriverNotified { get; set; }
        public DateTime CreateDate { get; set; }
    }

    public class CompanionReserveRescheduleResultVDto
    {
        public long ReserveId { get; set; }
        public DateTime NewDoDate { get; set; }
        public long NewCompanionTimeId { get; set; }
        public DateTime NewStartAt { get; set; }
        public long? LinkedTripId { get; set; }
        public DateTime? LinkedTripDepartureAt { get; set; }
        public bool UserNotified { get; set; }
        public bool DriverNotified { get; set; }
    }
}
