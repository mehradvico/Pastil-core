using Entities.Entities.Security;
using Entities.Entities.CommonField;

namespace Entities.Entities.ClinicField
{
    /// <summary>گرومرهایی که مجاز به انجام یک خدمت هستند (معادل CompanionAssistanceUser).</summary>
    public class ClinicServiceUser : Id_Field
    {
        public long ClinicServiceId { get; set; }
        public long UserId { get; set; }
        public bool Active { get; set; }
        public string ActivationValue { get; set; }
        public bool Deleted { get; set; }

        public ClinicService ClinicService { get; set; }
        public User User { get; set; }
    }
}
