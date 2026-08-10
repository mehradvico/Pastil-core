using Application.Common.Dto.Field;

namespace Application.Services.Content.BannerSrv.Dto
{
    public class BannerDto : FullName_FieldDto
    {
        public string Label { get; set; }
        public string Slug { get; set; }

        public string Url { get; set; }
        public int Priority { get; set; }
        public long? PictureId { get; set; }
        public long? Picture2Id { get; set; }
        public int ClickCount { get; set; }
        public bool Active { get; set; }
        public bool ShowToApp { get; set; }
        public bool ShowToSite { get; set; }
        public long? CategoryId { get; set; }
    }
}
