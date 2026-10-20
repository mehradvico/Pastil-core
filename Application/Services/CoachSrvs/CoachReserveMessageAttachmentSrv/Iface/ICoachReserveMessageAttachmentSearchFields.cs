using Entities.Entities.CoachField;
namespace Application.Services.CoachSrvs.CoachReserveMessageAttachmentSrv.Iface
{
    public interface ICoachReserveMessageAttachmentSearchFields
    {
        public long? CoachReserveMessageId { get; set; }
        public string ContentType { get; set; }
    }
}
