using Application.Common.Dto.Input;
using Application.Services.Setting.NoticeSrv.Iface;
using Entities.Entities;
using System;

namespace Application.Services.Setting.NoticeSrv.Dto
{
    public class NoticeInputDto : BaseInputDto, INoticeSearchFields
    {
        public long? ActorUserId { get; set; }
        public long? ReadByAdminId { get; set; }
        public long? NoticeTypeId { get; set; }
        public NoticeImportance? Importance { get; set; }
        public string ReferenceType { get; set; }
        public long? ReferenceId { get; set; }
        public bool? IsRead { get; set; }
        public bool? IsArchived { get; set; } = false;
        public DateTime? FromDateUtc { get; set; }
        public DateTime? ToDateUtc { get; set; }
    }
}
