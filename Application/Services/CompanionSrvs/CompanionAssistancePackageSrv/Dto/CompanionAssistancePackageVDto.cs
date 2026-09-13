using Application.Common.Dto.Field;
using Application.Services.CompanionSrv.CompanionAssistanceSrv.Dto;
using Application.Services.CompanionSrvs.CompanionAssistancePackagePictureSrv.Dto;
using Application.Services.Filing.PictureSrv.Dto;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Entities.Entities.CompanionField;
using System.Collections.Generic;

namespace Application.Services.CompanionSrv.CompanionAssistancePackageSrv.Dto
{
    public class CompanionAssistancePackageVDto : Name_FieldDto
    {
        public double Price { get; set; }
        public double PrePaymentPrice { get; set; }
        public long? PictureId { get; set; }
        public bool Active { get; set; }
        public string ActivationValue { get; set; }
        public long CompanionAssistanceId { get; set; }
        public string Discription { get; set; }
        public string PetSize { get; set; }
        public CompanionAssistanceVDto CompanionAssistance { get; set; }
        public PictureVDto Picture { get; set; }
        public List<CompanionAssistancePackagePictureVDto> CompanionAssistancePackagePictures { get; set; }
    }
}
