using Application.Common.Dto.Field;
using Application.Services.Accounting.UserPetSrv.Dto;
using Application.Services.ReminderSrvs.ReminderCycleSrv.Dto;
using Application.Services.ReminderSrvs.ReminderTypeSrv.Dto;
using System;

namespace Application.Services.ReminderSrvs.ReminderSrv.Dto
{
    public class ReminderVDto : Id_FieldDto
    {
        public long? ReminderTypeId { get; set; }
        public long ReminderCycleId { get; set; }
        public DateTime StartDate { get; set; }
        public TimeSpan NotificationTime { get; set; }
        public string CustomText { get; set; }
        public DateTime? LastChecked { get; set; }
        public long UserPetId { get; set; }
        public UserPetVDto UserPet { get; set; }
        public ReminderTypeVDto ReminderType { get; set; }
        public ReminderCycleVDto ReminderCycle { get; set; }
    }
}
