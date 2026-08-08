using Application.Common.Dto.Field;
using Application.Services.Filing.PictureSrv.Dto;

namespace Application.Services.CategorySrv.Dto
{
    public class SearchCategoryDto : Name_FieldDto
    {
        public string Label { get; set; }
        public string Slug { get; set; }
        public PictureVDto Icon { get; set; }

    }
}
