using Entities.Entities.CommonField;

namespace Entities.Entities.GroomerField
{
    /// <summary>گونه‌های پذیرش‌شده در آرایشگاه (سگ، گربه، ...).</summary>
    public class GroomerPet : Id_Field
    {
        public long GroomerId { get; set; }
        public long PetId { get; set; }
        public bool Deleted { get; set; }

        public Groomer Groomer { get; set; }
        public Pet Pet { get; set; }
    }
}
