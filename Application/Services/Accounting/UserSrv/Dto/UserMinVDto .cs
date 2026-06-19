using Application.Common.Dto.Field;

namespace Application.Services.Dto
{
    public class UserMinVDto : Id_FieldDto
    {

        public string Mobile { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string ReferralCode { get; set; }
        public bool IsFemale { get; set; }
        public string Expertise { get; set; }
        public long PictureId { get; set; }

    }
}
