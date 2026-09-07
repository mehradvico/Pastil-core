using Entities.Entities.CommonField;

namespace Entities.Entities
{
    public class PetBreedCharacteristic : Name_Field
    {
        public long PetBreedId { get; set; }
        public bool IsPositive { get; set; }
        public int Priority { get; set; }
        public bool Deleted { get; set; }

        public PetBreed PetBreed { get; set; }
    }
}
