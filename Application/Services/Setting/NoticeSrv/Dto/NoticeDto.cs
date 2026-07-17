using Application.Common.Dto.Field;
using Application.Services.Dto;
using Entities.Entities;
using System;
using System.Collections.Generic;

namespace Application.Services.Setting.NoticeSrv.Dto
{
    public class NoticeDto : Id_FieldDto
    {
        public long NoticeTypeId { get; set; }
        public long? ActorUserId { get; set; }
        public string ReferenceType { get; set; }
        public long? ReferenceId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string NavigationUrl { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
        public DateTime CreateDateUtc { get; set; }
        public DateTime ArchiveDueAtUtc { get; set; }
        public DateTime? ArchivedAtUtc { get; set; }
        public bool IsRead { get; set; }
        public NoticeTypeVDto NoticeType { get; set; }
        public UserMinVDto ActorUser { get; set; }
        public NoticeReadDto Read { get; set; }
    }
}
