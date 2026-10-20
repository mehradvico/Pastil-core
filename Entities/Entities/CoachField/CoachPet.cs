using Entities.Entities.CommonField;

namespace Entities.Entities.CoachField
{
    /// <summary>گونه‌های پذیرش‌شده در مربی (سگ، گربه، ...).</summary>
    public class CoachPet : Id_Field
    {
        public long CoachId { get; set; }
        public long PetId { get; set; }
        public bool Deleted { get; set; }

        public Coach Coach { get; set; }
        public Pet Pet { get; set; }
    }
}
