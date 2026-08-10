using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;

namespace Entities.Entities
{
    public class UserMemory : Id_Field
    {
        public long UserId { get; set; }
        public long UserPetId { get; set; }
        public long MemoryId { get; set; }
        public DateTime CreateDate { get; set; }
        public bool Deleted { get; set; }

        public User User { get; set; }
        public UserPet UserPet { get; set; }
        public Memory Memory { get; set; }
    }
}
