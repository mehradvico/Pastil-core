using Entities.Entities.CoachField;
namespace Application.Services.CoachSrvs.CoachReserveMessageReactionSrv.Iface
{
    public interface ICoachReserveMessageReactionSearchFields
    {
        public long? CoachReserveMessageId { get; set; }
        public long? ReactorUserId { get; set; }
        public string Reaction { get; set; }
    }
}
