using Application.Common.Dto.Field;
using Application.Services.Filing.PictureSrv.Dto;
using DocumentFormat.OpenXml.Drawing;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Application.Services.Dto
{
    public class UserDto : Id_FieldDto
    {
        [Display(Name = nameof(Resource.Field.Mobile), ResourceType = typeof(Resource.Field))]
        [Required(ErrorMessageResourceType = typeof(Resource.Pattern),
        ErrorMessageResourceName = nameof(Resource.Pattern.PleaseInsertT1))]
        public string Mobile { get; set; }
        [Display(Name = nameof(Resource.Field.Email), ResourceType = typeof(Resource.Field))]

        public string Email { get; set; }
        [Display(Name = nameof(Resource.Field.FirstName), ResourceType = typeof(Resource.Field))]
        [Required(ErrorMessageResourceType = typeof(Resource.Pattern),
          ErrorMessageResourceName = nameof(Resource.Pattern.PleaseInsertT1))]
        public string FirstName { get; set; }
        [Display(Name = nameof(Resource.Field.LastName), ResourceType = typeof(Resource.Field))]
        [Required(ErrorMessageResourceType = typeof(Resource.Pattern),
       ErrorMessageResourceName = nameof(Resource.Pattern.PleaseInsertT1))]

        public string LastName { get; set; }
        [Display(Name = nameof(Resource.Field.ReferralCode), ResourceType = typeof(Resource.Field))]
        public string ReferralCode { get; set; }
        public Entities.Entities.Security.RegistrationReferralSource RegistrationReferralSource { get; set; }
        public string UsedReferralCode { get; set; }
        public long? ReferredByUserId { get; set; }
        public long? ReferredByCompanionId { get; set; }
        public long? ReferredByStoreId { get; set; }

        [Display(Name = nameof(Resource.Field.Password), ResourceType = typeof(Resource.Field))]
        public string Password { get; set; }
        [Display(Name = nameof(Resource.Field.TwoFactorEnabled), ResourceType = typeof(Resource.Field))]
        [Required(ErrorMessageResourceType = typeof(Resource.Pattern),
        ErrorMessageResourceName = nameof(Resource.Pattern.PleaseInsertT1))]
        public bool TwoFactorEnabled { get; set; }
        [Display(Name = nameof(Resource.Field.Password), ResourceType = typeof(Resource.Field))]
        [Required(ErrorMessageResourceType = typeof(Resource.Pattern),
        ErrorMessageResourceName = nameof(Resource.Pattern.PleaseInsertT1))]
        public bool Locked { get; set; }
        public long? CompanionId { get; set; }
        public long? DriverId { get; set; }
        public string Expertise { get; set; }
        public bool IsCompanion { get; set; }

        [IgnoreDataMember]
        public DateTime CreateDate { get; set; }
        public long RoleId { get; set; }
        public string ClickGuid { get; set; }
        public bool IsFemale { get; set; }
        public long? PictureId { get; set; }

        public PictureVDto Picture { get; set; }

    }
}
