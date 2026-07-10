using Application.Common.Dto.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.LocationFields.ParkPictureSrv.Dto
{
    public class ParkPictureDto : Id_FieldDto
    {
        public long ParkId { get; set; }
        public long PictureId { get; set; }
        public string Label { get; set; }
    }
}
