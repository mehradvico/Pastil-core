using Application.Common.Dto.Field;
using Application.Services.Filing.PictureSrv.Dto;
using Application.Services.ProductSrvs.VarietySrv.Dto;

namespace Application.Services.ProductSrvs.ProductSrv.Dto
{
    public class ProductMinVDto : Name_FieldDto
    {
        public string Slug { get; set; }
        public string SecondName { get; set; }
        public string ProductLabel { get; set; }
        public long PictureId { get; set; }

        public VarietyShowVDto Variety { get; set; }
        public VarietyShowVDto Variety2 { get; set; }
        public PictureVDto Picture { get; set; }
    }
}
