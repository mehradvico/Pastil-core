using Application.Common.Dto.Field;
using Application.Services.Content.ContactUsItemSrv.Dto;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Application.Services.Content.ContactUsSrv.Dto
{
    public class ContactUsDto : Id_FieldDto
    {
        [Display(Name = nameof(Resource.Field.Name), ResourceType = typeof(Resource.Field))]
        [Required(ErrorMessageResourceType = typeof(Resource.Pattern),
          ErrorMessageResourceName = nameof(Resource.Pattern.PleaseInsertT1))]
        [StringLength(120)]
        public string FullName { get; set; }
        [Display(Name = nameof(Resource.Field.Mobile), ResourceType = typeof(Resource.Field))]
        [Required(ErrorMessageResourceType = typeof(Resource.Pattern),
  ErrorMessageResourceName = nameof(Resource.Pattern.PleaseInsertT1))]
        [RegularExpression(@"^09\d{9}$")]
        public string Mobile { get; set; }
        [Display(Name = nameof(Resource.Field.Email), ResourceType = typeof(Resource.Field))]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; }
        [Display(Name = nameof(Resource.Field.Title), ResourceType = typeof(Resource.Field))]
        [Required(ErrorMessageResourceType = typeof(Resource.Pattern),
  ErrorMessageResourceName = nameof(Resource.Pattern.PleaseInsertT1))]
        [StringLength(150)]
        public string Title { get; set; }
        [Display(Name = nameof(Resource.Field.Title), ResourceType = typeof(Resource.Field))]
        [Required(ErrorMessageResourceType = typeof(Resource.Pattern),
ErrorMessageResourceName = nameof(Resource.Pattern.PleaseInsertT1))]
        [StringLength(4000)]
        public string Body { get; set; }
        [Display(Name = nameof(Resource.Field.Category), ResourceType = typeof(Resource.Field))]
        [Required(ErrorMessageResourceType = typeof(Resource.Pattern),
ErrorMessageResourceName = nameof(Resource.Pattern.PleaseSelectT1))]
        public long ContactUsGroupId { get; set; }
        public string Answer { get; set; }
        public bool Status { get; set; }
        public long? FileId { get; set; }
        public long? UserId { get; set; }
        public List<ContactUsItemDto> ContactUsItems { get; set; } = new List<ContactUsItemDto>();

    }
}
