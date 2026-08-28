using Entities.Entities.CommonField;
using System;

namespace Entities.Entities
{
    public class PetTag : Id_Field
    {
        public string Code { get; set; }
        public long? UserPetId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? ClaimedDate { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }

        public UserPet UserPet { get; set; }
    }
}
