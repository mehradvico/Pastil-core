using Application.Common.Dto.Field;
using System;

namespace Application.Services.Accounting.PetTagSrv.Dto
{
    public class PetTagVDto : Id_FieldDto
    {
        public string Code { get; set; }
        public string Url { get; set; }
        public long? UserPetId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? ClaimedDate { get; set; }
        public bool Active { get; set; }
        public bool Claimed { get; set; }

        public string PetName { get; set; }
        public string OwnerFullName { get; set; }
        public string OwnerMobile { get; set; }
    }
}
