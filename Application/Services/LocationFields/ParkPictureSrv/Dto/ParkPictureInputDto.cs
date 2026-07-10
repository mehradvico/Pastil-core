using Application.Common.Dto.Input;
using Application.Services.LocationFields.ParkPictureSrv.Iface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.LocationFields.ParkPictureSrv.Dto
{
    public class ParkPictureInputDto : BaseInputDto, IParkPictureSearchFields
    {
        public long? ParkId { get; set; }
    }
}
