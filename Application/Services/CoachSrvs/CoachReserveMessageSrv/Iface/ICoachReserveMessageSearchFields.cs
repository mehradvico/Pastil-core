using Entities.Entities.CoachField;
namespace Application.Services.CoachSrvs.CoachReserveMessageSrv.Iface
{
    public interface ICoachReserveMessageSearchFields
    {
        public long? CoachReserveId { get; set; }
        public long? SenderUserId { get; set; }
        public long? CoachReserveMessageTypeId { get; set; }
        public long? ReplyToMessageId { get; set; }
        public bool? IsRead { get; set; }
        public long? BeforeMessageId { get; set; }
        public long? AfterMessageId { get; set; }
    }
}
