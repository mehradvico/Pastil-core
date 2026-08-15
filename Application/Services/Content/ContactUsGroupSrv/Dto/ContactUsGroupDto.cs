using Application.Common.Dto.Field;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Application.Services.Content.ContactUsGroupSrv.Dto
{
    public class ContactUsGroupDto : Name_FieldDto
    {
        [Display(Name = nameof(Resource.Field.Label), ResourceType = typeof(Resource.Field))]
        [Required(ErrorMessageResourceType = typeof(Resource.Pattern),
        ErrorMessageResourceName = nameof(Resource.Pattern.PleaseInsertT1))]
        public string Label { get; set; }
        [Display(Name = nameof(Resource.Field.Priority), ResourceType = typeof(Resource.Field))]
        public int Priority { get; set; }
        [Display(Name = nameof(Resource.Field.Active), ResourceType = typeof(Resource.Field))]
        public bool Active { get; set; }
        public string Roles { get; set; }
        public List<ContactUsFormFieldDto> FormFields { get; set; } = new List<ContactUsFormFieldDto>();

    }
}
