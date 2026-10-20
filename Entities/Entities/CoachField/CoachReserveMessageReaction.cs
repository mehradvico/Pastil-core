using Entities.Entities.CommonField;
using Entities.Entities.Security;

namespace Entities.Entities.CoachField
{
    public class CoachReserveMessageReaction : Id_Field
    {
        public long CoachReserveMessageId { get; set; }
        public long ReactorUserId { get; set; }

        public string Reaction { get; set; }
        public bool Deleted { get; set; }

        public CoachReserveMessage CoachReserveMessage { get; set; }
        public User ReactorUser { get; set; }
    }
}
