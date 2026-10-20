using Entities.Entities.Security;
using Entities.Entities.CommonField;

namespace Entities.Entities.CoachField
{
    /// <summary>گرومرهایی که مجاز به انجام یک خدمت هستند (معادل CompanionAssistanceUser).</summary>
    public class CoachServiceUser : Id_Field
    {
        public long CoachServiceId { get; set; }
        public long UserId { get; set; }
        public bool Active { get; set; }
        public string ActivationValue { get; set; }
        public bool Deleted { get; set; }

        public CoachService CoachService { get; set; }
        public User User { get; set; }
    }
}
