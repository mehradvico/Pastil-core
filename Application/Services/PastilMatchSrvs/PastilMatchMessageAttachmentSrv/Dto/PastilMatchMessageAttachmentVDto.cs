using Application.Common.Dto.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchMessageAttachmentSrv.Dto
{
    public class PastilMatchMessageAttachmentVDto : Id_FieldDto
    {
        public long PastilMatchMessageId { get; set; }

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
