using Entities.Entities.CommonField;
using Entities.Entities.Security;

namespace Entities.Entities.ClinicField
{
    public class ClinicReserveMessageReaction : Id_Field
    {
        public long ClinicReserveMessageId { get; set; }
        public long ReactorUserId { get; set; }

        public string Reaction { get; set; }
        public bool Deleted { get; set; }

        public ClinicReserveMessage ClinicReserveMessage { get; set; }
        public User ReactorUser { get; set; }
    }
}
