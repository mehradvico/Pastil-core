using Entities.Entities.CommonField;
using Entities.Entities.Security;

namespace Entities.Entities
{
    public class CompanionReserveMessageReaction : Id_Field
    {
        public long CompanionReserveMessageId { get; set; }
        public long ReactorUserId { get; set; }

        public string Reaction { get; set; }
        public bool Deleted { get; set; }

        public CompanionReserveMessage CompanionReserveMessage { get; set; }
        public User ReactorUser { get; set; }
    }
}
