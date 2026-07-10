using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;
using System.Collections.Generic;

namespace Entities.Entities
{
    public class UserPet : Name_Field
    {
        public long PetId { get; set; }
        public long UserId { get; set; }
        public long? PictureId { get; set; }
        public long? PetBreedId { get; set; }
        public long? PetBreed2Id { get; set; }
        public bool IsMixBreed { get; set; }
        public DateTime Birthday { get; set; }
        public string MicroChipCode { get; set; }
        public string Size { get; set; }
        public string Weight { get; set; }
        public bool IsMale { get; set; }
        public bool IsSterile { get; set; }
        public string SpecificDisease { get; set; }
        public string SpecificMedicene { get; set; }
        public string AddressValue { get; set; }
        public bool Deleted { get; set; }
        public bool Active { get; set; }

        public Pet Pet { get; set; }
        public User User { get; set; }
        public Picture Picture { get; set; }
        public PetBreed PetBreed { get; set; }
        public PetBreed PetBreed2 { get; set; }
        public ICollection<CompanionReserve> CompanionReserves { get; set; }
        public ICollection<UserPetRecord> UserPetRecords { get; set; }
        public ICollection<UserPetPicture> UserPetPictures { get; set; }
    }
}
