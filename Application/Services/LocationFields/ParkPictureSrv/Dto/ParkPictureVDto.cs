using Application.Common.Dto.Field;
using Application.Services.Filing.PictureSrv.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.LocationFields.ParkPictureSrv.Dto
{
    public class ParkPictureVDto : Id_FieldDto
    {
        public long ParkId { get; set; }
        public long PictureId { get; set; }
        public string Label { get; set; }

        public PictureVDto Picture { get; set; }
    }
}
