using Application.Common.Dto.Field;
using Application.Services.Filing.PictureSrv.Dto;

namespace Application.Services.SchoolSrvs.SchoolSrv.Dto
{
    public class SearchSchoolDto : Name_FieldDto
    {
        public long CompanionId { get; set; }
        public double RateAvg { get; set; }
        public int RateCount { get; set; }
        public PictureVDto Picture { get; set; }
    }
}
