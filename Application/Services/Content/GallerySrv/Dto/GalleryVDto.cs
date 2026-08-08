using Application.Common.Dto.Field;
using Application.Services.CategorySrv.Dto;
using Entities.Entities;

namespace Application.Services.Content.GallerySrv.Dto
{
    public class GalleryVDto : Seo_Full_FieldDto
    {

        public string Label { get; set; }
        public string Slug { get; set; }

        public long? PictureId { get; set; }
        public long? CategoryId { get; set; }
        public Picture Picture { get; set; }
        public CategoryVDto Category { get; set; }

    }
}
