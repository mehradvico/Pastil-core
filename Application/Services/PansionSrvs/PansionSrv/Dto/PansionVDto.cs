using Application.Common.Dto.Field;
using Application.Services.CompanionSrvs.CompanionSrv.Dto;
using Application.Services.Filing.PictureSrv.Dto;
using Application.Services.LocationFields.CitySrv.Dto;
using Application.Services.LocationFields.StateSrv.Dto;
using Application.Services.PansionSrvs.PansionCommentSrv.Dto;
using Application.Services.PansionSrvs.PansionPetSrv.Dto;
using Application.Services.PansionSrvs.PansionPictureSrv.Dto;
using Entities.Entities;
using Entities.Entities.PansionField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PansionSrvs.PansionSrv.Dto
{
    public class PansionVDto : Name_FieldDto
    {
        public bool? IsSchool { get; set; }
        public long CompanionId { get; set; }
        public bool Active { get; set; }
        public bool ShowToSite { get; set; }
        public bool Approve { get; set; }
        public long StateId { get; set; }
        public long CityId { get; set; }
        public string Discription { get; set; }
        public string AddressValue { get; set; }
        public int CommentCount { get; set; }
        public double RateAvg { get; set; }
        public int RateCount { get; set; }
        public long? PictureId { get; set; }
        public bool Suggested { get; set; }
        public double PansionPrice { get; set; }
        public double SchoolPrice { get; set; }
        public string Regulations { get; set; }
        public string OpenHour { get; set; }
        public string CloseHour { get; set; }
        public decimal DailyCommissionPercent { get; set; }
        public decimal HourlyCommissionPercent { get; set; }
        public CompanionMinVDto Companion { get; set; }
        public StateVDto State { get; set; }
        public CityVDto City { get; set; }
        public PictureVDto Picture { get; set; }
        public List<PansionPetVDto> PansionPets { get; set; }
        public List<PansionCommentVDto> PansionComments { get; set; }
        public List<PansionPictureVDto> PansionPictures { get; set; }
    }
}
