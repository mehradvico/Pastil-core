using Application.Common.Dto.Field;
using Application.Common.Dto.LocationPoint;
using Application.Services.CompanionSrvs.CompanionPetSrv.Dto;
using Application.Services.CompanionSrvs.CompanionTypeSrv.Dto;
using Application.Services.CompanionSrvs.CompanionZoneSrv.Dto;
using Application.Services.Dto;
using Application.Services.Filing.PictureSrv.Dto;
using Application.Services.LocationFields.CitySrv.Dto;
using Application.Services.LocationFields.NeighborhoodSrv.Dto;
using Application.Services.PansionSrvs.PansionSrv.Dto;
using Entities.Entities.CompanionField;
using System;
using System.Collections.Generic;

namespace Application.Services.CompanionSrvs.CompanionSrv.Dto
{
    public class CompanionVDto : Seo_Full_FieldDto
    {
        public bool IsPersonal { get; set; }
        public long OwnerId { get; set; }
        public string ReferralCode { get; set; }
        public DateTime? GoldAccountDate { get; set; }
        public DateTime? SilverAccountDate { get; set; }
        public DateTime? SilverAccountCreateDate { get; set; }

        public long? PictureId { get; set; }
        public long? BackgroundPictureId { get; set; }
        public long? IconId { get; set; }
        public bool Active { get; set; }
        public bool ShowToSite { get; set; }
        public bool Approved { get; set; }
        public string ActivationValue { get; set; }
        public string AddressValue { get; set; }
        public string Phone { get; set; }
        public long CityId { get; set; }
        public long? NeighborhoodId { get; set; }
        public int CommentCount { get; set; }
        public double RateAvg { get; set; }
        public int RateCount { get; set; }
        public bool IsSilver { get; set; }
        public bool IsGold { get; set; }
        public string SearchKey { get; set; }
        public bool HasPansion { get; set; }

        public UserMinVDto Owner { get; set; }
        public CityVDto City { get; set; }
        public NeighborhoodVDto Neighborhood { get; set; }
        public PointDto Location { get; set; }
        public PictureVDto Picture { get; set; }
        public PictureVDto BackgroundPicture { get; set; }
        public PictureVDto Icon { get; set; }
        public List<CompanionPetVDto> CompanionPets { get; set; }
        public List<CompanionTypeVDto> CompanionTypes { get; set; }
        public List<CompanionZoneVDto> CompanionZones { get; set; }
        public List<PansionVDto> Pansions { get; set; }
    }
}
