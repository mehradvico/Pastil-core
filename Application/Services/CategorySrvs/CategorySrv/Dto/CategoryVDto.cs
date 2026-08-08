using Application.Common.Dto.Field;
using Application.Services.Filing.PictureSrv.Dto;

namespace Application.Services.CategorySrv.Dto
{
    public class CategoryVDto : Seo_Full_FieldDto
    {
        public long? ParentId { get; set; }
        public string Label { get; set; }
        public string Slug { get; set; }
        public int Priority { get; set; }
        public long? PictureId { get; set; }
        public long? IconId { get; set; }
        public bool Active { get; set; }
        public PictureVDto Picture { get; set; }
        public PictureVDto Icon { get; set; }

    }
}
