using Application.Common.Dto.Field;
using Application.Services.Dto;
using Application.Services.Filing.PictureSrv.Dto;
using System;

namespace Application.Services.CompanionSrvs.CompanionSrv.Dto
{
    public class CompanionMinVDto : Name_FieldDto
    {
        public bool IsPersonal { get; set; }
        public long OwnerId { get; set; }
        public long? PictureId { get; set; }
        public long? BackgroundPictureId { get; set; }
        public long? IconId { get; set; }
        public bool Active { get; set; }
        public string ActivationValue { get; set; }
        public string Phone { get; set; }
        public DateTime? GoldAccountDate { get; set; }
        public DateTime? SilverAccountDate { get; set; }
        public int CommentCount { get; set; }
        public double RateAvg { get; set; }
        public int RateCount { get; set; }

        public bool IsSilver { get; set; }
        public bool IsGold { get; set; }

        public UserMinVDto Owner { get; set; }
        public PictureVDto Picture { get; set; }
        public PictureVDto BackgroundPicture { get; set; }
        public PictureVDto Icon { get; set; }
    }
}
