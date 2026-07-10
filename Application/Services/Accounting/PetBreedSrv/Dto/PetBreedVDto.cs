using Application.Common.Dto.Field;
using Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Accounting.PetBreedSrv.Dto
{
    public class PetBreedVDto : Name_FieldDto
    {
        public long PetId { get; set; }
        public int Priority { get; set; }
        public string Label { get; set; }
        public long? PictureId { get; set; }

        public Pet Pet { get; set; }
        public Picture Picture { get; set; }
    }
}
