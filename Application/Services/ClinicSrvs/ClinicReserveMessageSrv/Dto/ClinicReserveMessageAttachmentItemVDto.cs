using Entities.Entities.ClinicField;
using Application.Common.Dto.Field;

namespace Application.Services.ClinicSrvs.ClinicReserveMessageSrv.Dto
{
    public class ClinicReserveMessageAttachmentItemVDto : Id_FieldDto
    {
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
