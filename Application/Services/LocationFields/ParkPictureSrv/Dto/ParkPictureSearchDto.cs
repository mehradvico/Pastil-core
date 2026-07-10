using Application.Common.Dto.Result;
using Application.Services.LocationFields.ParkPictureSrv.Iface;
using AutoMapper;
using Entities.Entities.LocationField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.LocationFields.ParkPictureSrv.Dto
{
    public class ParkPictureSearchDto : BaseSearchDto<ParkPicture, ParkPictureVDto>, IParkPictureSearchFields
    {
        public ParkPictureSearchDto(ParkPictureInputDto dto, IQueryable<ParkPicture> list, IMapper mapper) : base(dto, list, mapper)
        {
            ParkId = dto.ParkId;
        }
        public long? ParkId { get; set; }
    }
}
