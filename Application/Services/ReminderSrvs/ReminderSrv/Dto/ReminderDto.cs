using Application.Common.Dto.Field;
using System;

namespace Application.Services.ReminderSrvs.ReminderSrv.Dto
{
    public class ReminderDto : Id_FieldDto
    {
        public long? ReminderTypeId { get; set; }
        public long ReminderCycleId { get; set; }
        public DateTime StartDate { get; set; }
        public TimeSpan NotificationTime { get; set; }
        public string CustomText { get; set; }
        public DateTime? LastChecked { get; set; }
        public long UserPetId { get; set; }
    }
}
