using Application.Common.Dto.Result;
using Application.Services.Setting.NoticeSrv.Iface;
using AutoMapper;
using Entities.Entities;
using System;
using System.Linq;

namespace Application.Services.Setting.NoticeSrv.Dto
{
    public class NoticeSearchDto : BaseSearchDto<Notice, NoticeVDto>, INoticeSearchFields
    {
        public NoticeSearchDto(NoticeInputDto dto, IQueryable<Notice> list, IMapper mapper)
            : base(dto, list, mapper)
        {
            ActorUserId = dto.ActorUserId;
            ReadByAdminId = dto.ReadByAdminId;
            NoticeTypeId = dto.NoticeTypeId;
            Importance = dto.Importance;
            ReferenceType = dto.ReferenceType;
            ReferenceId = dto.ReferenceId;
            IsRead = dto.IsRead;
            IsArchived = dto.IsArchived;
            FromDateUtc = dto.FromDateUtc;
            ToDateUtc = dto.ToDateUtc;
        }

        public long? ActorUserId { get; set; }
        public long? ReadByAdminId { get; set; }
        public long? NoticeTypeId { get; set; }
        public NoticeImportance? Importance { get; set; }
        public string ReferenceType { get; set; }
        public long? ReferenceId { get; set; }
        public bool? IsRead { get; set; }
        public bool? IsArchived { get; set; }
        public DateTime? FromDateUtc { get; set; }
        public DateTime? ToDateUtc { get; set; }
    }
}
