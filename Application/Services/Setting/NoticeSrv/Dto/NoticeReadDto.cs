using Application.Common.Dto.Field;
using Entities.Entities;
using System;

namespace Application.Services.Setting.NoticeSrv.Dto
{
    public class NoticeReadDto : Id_FieldDto
    {
        public long NoticeId { get; set; }
        public long AdminId { get; set; }
        public string AdminNameSnapshot { get; set; }
        public DateTime ReadAtUtc { get; set; }
        public NoticeReadMode ReadMode { get; set; }
    }
}
