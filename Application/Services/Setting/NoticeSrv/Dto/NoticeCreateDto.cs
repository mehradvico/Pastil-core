using System.Collections.Generic;

namespace Application.Services.Setting.NoticeSrv.Dto
{
    public class NoticeCreateDto
    {
        public string Label { get; set; }
        public long? ActorUserId { get; set; }
        public string ReferenceType { get; set; }
        public long? ReferenceId { get; set; }
        public string DeduplicationKey { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }
}
