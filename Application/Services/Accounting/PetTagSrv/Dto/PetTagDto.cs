using Application.Common.Dto.Field;
using System;

namespace Application.Services.Accounting.PetTagSrv.Dto
{
    public class PetTagDto : Id_FieldDto
    {
        public string Code { get; set; }
        public long? UserPetId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? ClaimedDate { get; set; }
        public bool Active { get; set; }
    }
}
