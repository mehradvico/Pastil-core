using Entities.Entities.CommonField;

namespace Entities.Entities
{
    public class CompanionReserveMessageAttachment : Id_Field
    {
        public long CompanionReserveMessageId { get; set; }

        public string Url { get; set; }
        public string ThumbnailUrl { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }

        public long FileSize { get; set; }
        public int? Duration { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public int Order { get; set; }

        public bool Deleted { get; set; }

        public CompanionReserveMessage CompanionReserveMessage { get; set; }
    }
}
