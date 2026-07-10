using Application.Common.Dto.Input;
using Application.Services.LocationFields.StateSrv.Iface;
using Application.Services.StoreSrv.Iface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.LocationFields.StateSrv.Dto
{
    public class StateInputDto : BaseInputDto, IStateSerchFields
    {
        public long? CountryId { get; set; }
    }
}
