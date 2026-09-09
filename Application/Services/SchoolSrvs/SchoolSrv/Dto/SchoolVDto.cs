using Application.Common.Dto.Field;
using Application.Services.CompanionSrvs.CompanionSrv.Dto;
using Application.Services.Filing.PictureSrv.Dto;
using Application.Services.LocationFields.CitySrv.Dto;
using Application.Services.LocationFields.StateSrv.Dto;
using Application.Services.SchoolSrvs.SchoolCourseSrv.Dto;
using System.Collections.Generic;

namespace Application.Services.SchoolSrvs.SchoolSrv.Dto
{
    public class SchoolVDto : Name_FieldDto
    {
        public long CompanionId { get; set; }
        public bool Active { get; set; }
        public bool ShowToSite { get; set; }
        public bool Approve { get; set; }
        public string ApprovalValue { get; set; }
        public long StateId { get; set; }
        public long CityId { get; set; }
        public string Discription { get; set; }
        public string AddressValue { get; set; }
        public int CommentCount { get; set; }
        public double RateAvg { get; set; }
        public int RateCount { get; set; }
        public long? PictureId { get; set; }
        public bool Suggested { get; set; }
        public string Regulations { get; set; }

        public CompanionMinVDto Companion { get; set; }
        public StateVDto State { get; set; }
        public CityVDto City { get; set; }
        public PictureVDto Picture { get; set; }
        public List<SchoolCourseDto> SchoolCourses { get; set; }
    }
}
