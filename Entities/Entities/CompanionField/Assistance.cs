using Entities.Entities.CommonField;

namespace Entities.Entities
{
    public class Assistance : FullName_Field
    {
        public bool IsPersonal { get; set; }
        public long? AssistanceGroupId { get; set; }
        public long? PictureId { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }

        public AssistanceGroup AssistanceGroup { get; set; }
        public Picture Picture { get; set; }
    }
}
