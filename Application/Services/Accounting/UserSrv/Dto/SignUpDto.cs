namespace Application.Services.Dto
{
    public class SignUpDto
    {
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public long? PictureId { get; set; }
        public bool IsFemail { get; set; }
        public string Password { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public string Code { get; set; }
        public string CartCode { get; set; }
        public Entities.Entities.Security.RegistrationReferralSource ReferralSource { get; set; }
        public string ReferralCode { get; set; }
    }
}
