using Application.Common.Dto.Field;
using Application.Services.Filing.PictureSrv.Dto;
using Application.Services.LocationFields.NeighborhoodSrv.Dto;
using Application.Services.LocationFields.ParkPictureSrv.Dto;
using Application.Services.PansionSrvs.PansionPictureSrv.Dto;
using Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.LocationFields.ParkSrv.Dto
{
    public class ParkVDto : Name_FieldDto
    {
        public long NeighborhoodId { get; set; }
        public bool Suggested { get; set; }
        public long? PictureId { get; set; }

        public NeighborhoodVDto Neighborhood { get; set; }
        public PictureVDto Picture { get; set; }
        public List<ParkPictureVDto> ParkPictures { get; set; }
    }
}
