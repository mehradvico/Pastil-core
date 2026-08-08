using Application.Common.Dto.Field;
using Application.Services.Filing.PictureSrv.Dto;
using System.ComponentModel.DataAnnotations;

namespace Application.Services.CategorySrv.Dto
{
    public class CategoryDto : Seo_Full_FieldDto
    {

        public long? ParentId { get; set; }
        [Display(Name = nameof(Resource.Field.Label), ResourceType = typeof(Resource.Field))]
        [Required(ErrorMessageResourceType = typeof(Resource.Pattern),
          ErrorMessageResourceName = nameof(Resource.Pattern.PleaseInsertT1))]
        public string Label { get; set; }
        public string Slug { get; set; }
        [Display(Name = nameof(Resource.Field.Priority), ResourceType = typeof(Resource.Field))]
        public int Priority { get; set; }
        [Display(Name = nameof(Resource.Field.Active), ResourceType = typeof(Resource.Field))]
        [Required(ErrorMessageResourceType = typeof(Resource.Pattern),
        ErrorMessageResourceName = nameof(Resource.Pattern.PleaseInsertT1))]
        public bool Active { get; set; }
        public long? PictureId { get; set; }
        public long? IconId { get; set; }
        public PictureVDto Picture { get; set; }
        public PictureVDto Icon { get; set; }

    }
}
