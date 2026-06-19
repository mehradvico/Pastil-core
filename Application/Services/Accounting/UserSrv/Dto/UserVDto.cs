using Application.Common.Dto.Field;
using Application.Services.Filing.PictureSrv.Dto;

namespace Application.Services.Dto
{
    public class UserVDto : Id_FieldDto
    {

        public string Mobile { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string ReferralCode { get; set; }
        public long? PictureId { get; set; }
        public long? CompanionId { get; set; }
        public long? DriverId { get; set; }
        public string Expertise { get; set; }
        public bool IsCompanion { get; set; }

        public long RoleId { get; set; }
        public string RoleName { get; set; }
        public bool IsFemale { get; set; }

        public PictureVDto Picture { get; set; }
    }
}
