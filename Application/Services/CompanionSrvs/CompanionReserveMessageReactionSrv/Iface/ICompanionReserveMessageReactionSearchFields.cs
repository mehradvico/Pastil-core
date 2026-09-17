namespace Application.Services.CompanionSrvs.CompanionReserveMessageReactionSrv.Iface
{
    public interface ICompanionReserveMessageReactionSearchFields
    {
        public long? CompanionReserveMessageId { get; set; }
        public long? ReactorUserId { get; set; }
        public string Reaction { get; set; }
    }
}
