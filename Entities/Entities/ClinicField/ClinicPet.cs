using Entities.Entities.CommonField;

namespace Entities.Entities.ClinicField
{
    /// <summary>گونه‌های پذیرش‌شده در کلینیک (سگ، گربه، ...).</summary>
    public class ClinicPet : Id_Field
    {
        public long ClinicId { get; set; }
        public long PetId { get; set; }
        public bool Deleted { get; set; }

        public Clinic Clinic { get; set; }
        public Pet Pet { get; set; }
    }
}
