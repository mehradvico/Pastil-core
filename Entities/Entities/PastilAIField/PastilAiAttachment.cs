using Entities.Entities.CommonField;

namespace Entities.Entities.PastilAIField
{
    public class PastilAiAttachment : Id_Field
    {
        public long MessageId { get; set; }
        public long? PictureId { get; set; }
        public long? FileId { get; set; }
        public PastilAiInputType Type { get; set; }
        public PastilAiMessage Message { get; set; }
        public Picture Picture { get; set; }
        public File File { get; set; }
    }
}
