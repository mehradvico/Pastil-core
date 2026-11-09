using Application.Common.Dto.Field;
using Application.Services.Accounting.PetSrv.Dto;
using Application.Services.PansionSrvs.PansionSrv.Dto;
using Entities.Entities;
using Entities.Entities.PansionField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PansionSrvs.PansionPetSrv.Dto
{
    public class PansionPetVDto : Id_FieldDto
    {
        public long PansionId { get; set; }
        public long PetId { get; set; }
        public string PetSize { get; set; }
        public double? PansionPrice { get; set; }
        public double? SchoolPrice { get; set; }

        public PetVDto Pet { get; set; }
    }
}
