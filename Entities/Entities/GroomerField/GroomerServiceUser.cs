using Entities.Entities.Security;
using Entities.Entities.CommonField;

namespace Entities.Entities.GroomerField
{
    /// <summary>گرومرهایی که مجاز به انجام یک خدمت هستند (معادل CompanionAssistanceUser).</summary>
    public class GroomerServiceUser : Id_Field
    {
        public long GroomerServiceId { get; set; }
        public long UserId { get; set; }
        public bool Active { get; set; }
        public string ActivationValue { get; set; }
        public bool Deleted { get; set; }

        public GroomerService GroomerService { get; set; }
        public User User { get; set; }
    }
}
