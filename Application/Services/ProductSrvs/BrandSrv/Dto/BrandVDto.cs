using Application.Common.Dto.Field;
using Application.Services.Filing.PictureSrv.Dto;

namespace Application.Services.ProductSrvs.BrandSrv.Dto
{
    public class BrandVDto : Seo_Full_FieldDto
    {
        public string Slug { get; set; }
        public long? PictureId { get; set; }
        public long? IconId { get; set; }
        public int Priority { get; set; }
        public string SecondName { get; set; }
        public bool Active { get; set; }

        public PictureVDto Picture { get; set; }
        public PictureVDto Icon { get; set; }

    }
}
