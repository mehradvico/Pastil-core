using Application.Common.Dto.Field;

namespace Application.Services.CompanionSrvs.CompanionReserveMessageAttachmentSrv.Dto
{
    public class CompanionReserveMessageAttachmentVDto : Id_FieldDto
    {
        public long CompanionReserveMessageId { get; set; }

        public string Url { get; set; }
        public string ThumbnailUrl { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }

        public long FileSize { get; set; }
        public int? Duration { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public int Order { get; set; }
    }
}
