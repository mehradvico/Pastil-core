using Application.Common.Enumerable;

namespace Application.Services.CompanionSrv.CompanionReserveSrv.Iface
{
    public interface ICompanionReserveSearchFields
    {
        public long? BookerId { get; set; }
        public long? UserPetId { get; set; }
        public long? CompanionAssistanceId { get; set; }
        public long? CompanionAssistanceTimeId { get; set; }
        public long? CompanionTimeId { get; set; }
        public long? CompanionAssistanceUserId { get; set; }
        public bool? IsFemale { get; set; }
        public long? CompanionId { get; set; }
        public ReserveStateEnum? ReserveState { get; set; }
    }
}
