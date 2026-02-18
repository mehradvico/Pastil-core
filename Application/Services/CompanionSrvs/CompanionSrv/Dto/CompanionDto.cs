using Application.Common.Dto.Field;
using Application.Common.Dto.LocationPoint;
using Application.Services.CompanionSrvs.CompanionPetSrv.Dto;
using Application.Services.CompanionSrvs.CompanionTypeSrv.Dto;
using Application.Services.Dto;
using Application.Services.Filing.PictureSrv.Dto;
using System;
using System.Collections.Generic;

namespace Application.Services.CompanionSrvs.CompanionSrv.Dto
{
    public class CompanionDto : Seo_Full_FieldDto
    {
        public bool IsPersonal { get; set; }
        public long OwnerId { get; set; }
        public DateTime? GoldAccountDate { get; set; }
        public DateTime? SilverAccountDate { get; set; }
        public long? PictureId { get; set; }
        public long? BackgroundPictureId { get; set; }
        public long? IconId { get; set; }
        public bool Active { get; set; }
        public bool Approved { get; set; }
        public string ActivationValue { get; set; }
        public string AddressValue { get; set; }
        public string Phone { get; set; }
        public long CityId { get; set; }
        public long? NeighborhoodId { get; set; }
        public string SearchKey { get; set; }

        public PointDto Location { get; set; }
        public PictureVDto Picture { get; set; }
        public PictureVDto BackgroundPicture { get; set; }
        public PictureVDto Icon { get; set; }
        public UserMinVDto Owner { get; set; }
        public List<CompanionPetVDto> CompanionPets { get; set; }
        public List<CompanionTypeVDto> CompanionTypes { get; set; }
    }
}
