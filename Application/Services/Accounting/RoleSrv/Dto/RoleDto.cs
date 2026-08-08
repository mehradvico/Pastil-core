using Application.Common.Dto.Field;
using System.ComponentModel.DataAnnotations;

namespace Application.Services.Dto
{
    public class RoleDto : Name_FieldDto
    {
        [Display(Name = nameof(Resource.Field.Label), ResourceType = typeof(Resource.Field))]
        [Required(ErrorMessageResourceType = typeof(Resource.Pattern),
        ErrorMessageResourceName = nameof(Resource.Pattern.PleaseInsertT1))]
        public string Label { get; set; }
        public string Slug { get; set; }

    }
}
