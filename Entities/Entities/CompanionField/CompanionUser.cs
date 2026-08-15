using Entities.Entities.CommonField;
using Entities.Entities.Security;

namespace Entities.Entities.CompanionField
{
    public class CompanionUser : Id_Field
    {
        public long CompanionId { get; set; }
        public long UserId { get; set; }
        public long? ExpertiseId { get; set; }
        public bool? UserAccept { get; set; }
        public bool Active { get; set; }
        public string ActivationValue { get; set; }
        public bool Deleted { get; set; }
        public Companion Companion { get; set; }
        public User User { get; set; }
        public Expertise Expertise { get; set; }
    }
}
