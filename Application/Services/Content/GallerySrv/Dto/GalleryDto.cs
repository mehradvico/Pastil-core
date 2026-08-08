using Application.Common.Dto.Field;
using System.ComponentModel.DataAnnotations;

namespace Application.Services.Content.GallerySrv.Dto
{
    public class GalleryDto : Seo_Full_FieldDto
    {
        [Display(Name = nameof(Resource.Field.Label), ResourceType = typeof(Resource.Field))]
        [Required(ErrorMessageResourceType = typeof(Resource.Pattern),
        ErrorMessageResourceName = nameof(Resource.Pattern.PleaseInsertT1))]
        public string Label { get; set; }
        public string Slug { get; set; }
        public bool Active { get; set; }
        public int Priority { get; set; }
        public long? PictureId { get; set; }
        public long? CategoryId { get; set; }



    }
}
