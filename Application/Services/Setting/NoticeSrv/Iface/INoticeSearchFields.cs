using Entities.Entities;
using System;

namespace Application.Services.Setting.NoticeSrv.Iface
{
    public interface INoticeSearchFields
    {
        long? ActorUserId { get; set; }
        long? ReadByAdminId { get; set; }
        long? NoticeTypeId { get; set; }
        NoticeImportance? Importance { get; set; }
        string ReferenceType { get; set; }
        long? ReferenceId { get; set; }
        bool? IsRead { get; set; }
        bool? IsArchived { get; set; }
        DateTime? FromDateUtc { get; set; }
        DateTime? ToDateUtc { get; set; }
    }
}
