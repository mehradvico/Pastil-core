using Application.Common.Dto.Field;
using Application.Services.Filing.FileSrv.Dto;
using System;

namespace Application.Services.Accounting.TicketItemSrv.Dto
{
    public class TicketItemMinVDto : Id_FieldDto
    {
        public string Body { get; set; }
        public long UserId { get; set; }
        public long? FileId { get; set; }
        public DateTime CreateDate { get; set; }
        public bool IsSeen { get; set; }
        public bool IsMine { get; set; }
        public bool IsFromSupport { get; set; }
        public FileVDto File { get; set; }
    }
}