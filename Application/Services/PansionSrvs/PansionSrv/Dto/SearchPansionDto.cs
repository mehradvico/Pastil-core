using Application.Common.Dto.Field;
using Application.Services.Filing.PictureSrv.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PansionSrvs.PansionSrv.Dto
{
    public class SearchPansionDto : Name_FieldDto
    {
        public long CityId { get; set; }
        public long StateId { get; set; }
        public double RateAvg { get; set; }
        public int RateCount { get; set; }
        public bool? IsSchool { get; set; }
        public PictureVDto Picture { get; set; }
    }
}
