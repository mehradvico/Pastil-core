using Entities.Entities.CommonField;
using System;

namespace Entities.Entities
{
    public class Reminder : Id_Field
    {
        public long? ReminderTypeId { get; set; }
        public long ReminderCycleId { get; set; }
        public DateTime StartDate { get; set; }
        // ساعت محلی تهران که اعلان‌های این یادآور باید ارسال شوند.
        public TimeSpan NotificationTime { get; set; }
        // توضیح دلخواه کاربر؛ نوع یادآور همچنان برای دسته‌بندی و چرخه حفظ می‌شود.
        public string CustomText { get; set; }
        public DateTime? LastChecked { get; set; }
        public long UserPetId { get; set; }
        public bool Deleted { get; set; }
        public UserPet UserPet { get; set; }
        public ReminderType ReminderType { get; set; } // null وقتی یادآور متن دلخواه دارد
        public ReminderCycle ReminderCycle { get; set; }
    }
}
