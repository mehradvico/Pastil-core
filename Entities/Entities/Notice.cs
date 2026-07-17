using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;
using System.Collections.Generic;

namespace Entities.Entities
{
    public class Notice : Id_Field
    {
        public long NoticeTypeId { get; set; }

        public long? ActorUserId { get; set; }

        public string ReferenceType { get; set; }

        public long? ReferenceId { get; set; }

        public string Title { get; set; }

        public string Message { get; set; }

        public string NavigationUrl { get; set; }

        public string MetadataJson { get; set; }

        public string DeduplicationKey { get; set; }

        public DateTime CreateDateUtc { get; set; }

        public DateTime ArchiveDueAtUtc { get; set; }

        public DateTime? ArchivedAtUtc { get; set; }

        public NoticeType NoticeType { get; set; }

        public User ActorUser { get; set; }

        public NoticeRead Read { get; set; }

        public ICollection<PushNotification> PushNotifications { get; set; } = new List<PushNotification>();
    }
}
