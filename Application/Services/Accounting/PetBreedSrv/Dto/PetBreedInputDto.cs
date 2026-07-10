using Application.Common.Dto.Input;
using Application.Services.Accounting.PetBreedSrv.Iface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Accounting.PetBreedBreedSrv.Dto
{
    public class PetBreedInputDto : BaseInputDto, IPetBreedSearchFields
    {
        public long? PetId { get; set; }
    }
}
