using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;

namespace Entities.Entities
{
    public class NoticeRead : Id_Field
    {
        public long NoticeId { get; set; }

        public long AdminId { get; set; }

        public string AdminNameSnapshot { get; set; }

        public DateTime ReadAtUtc { get; set; }

        public NoticeReadMode ReadMode { get; set; }

        public Notice Notice { get; set; }

        public User Admin { get; set; }
    }
}
