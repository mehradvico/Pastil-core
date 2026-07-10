using Application.Common.Dto.Field;
using Application.Services.CategorySrv.Dto;
using Application.Services.Filing.PictureSrv.Dto;

namespace Application.Services.Content.BannerSrv.Dto
{
    public class BannerVDto : FullName_FieldDto
    {

        public string Url { get; set; }
        public int Priority { get; set; }
        public long? PictureId { get; set; }
        public long? Picture2Id { get; set; }
        public int ClickCount { get; set; }
        public long? CategoryId { get; set; }
        public bool Active { get; set; }
        public CategoryVDto Category { get; set; }
        public PictureVDto Picture { get; set; }
        public PictureVDto Picture2 { get; set; }
    }
}
