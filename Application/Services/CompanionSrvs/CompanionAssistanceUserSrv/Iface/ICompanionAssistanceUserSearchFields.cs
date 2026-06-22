namespace Application.Services.CompanionSrv.CompanionAssistanceUserSrv.Iface
{
    public interface ICompanionAssistanceUserSearchFields
    {
        public long? CompanionAssistanceId { get; set; }
        public long? UserId { get; set; }
        public long? CompanionId { get; set; }
    }
}
