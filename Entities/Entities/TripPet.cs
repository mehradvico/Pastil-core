using Entities.Entities.CommonField;

namespace Entities.Entities
{
    public class TripPet : Id_Field
    {
        public long TripId { get; set; }
        public long UserPetId { get; set; }

        public Trip Trip { get; set; }
        public UserPet UserPet { get; set; }
    }
}
