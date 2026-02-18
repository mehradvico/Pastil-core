using Application.Common.Dto.Field;
using Application.Common.Dto.LocationPoint;
using Application.Services.CommonSrv.CitySrv.Dto;
using Application.Services.Dto;
using Application.Services.Filing.PictureSrv.Dto;
using Application.Services.Setting.CodeSrv.Dto;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using DocumentFormat.OpenXml.Spreadsheet;
using Entities.Entities.Security;
using System;
using System.Collections.Generic;

namespace Application.Services.ProductSrvs.StoreSrv.Dto
{
    public class StoreVDto : Seo_Full_FieldDto
    {
        public string Phone { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public long? PictureId { get; set; }
        public long? IconId { get; set; }
        public long TypeId { get; set; }
        public long CityId { get; set; }
        public DateTime CreateDate { get; set; }
        public int MaxDiscountPercent { get; set; }
        public double RateAvg { get; set; }
        public int RateCount { get; set; }
        public bool Active { get; set; }
        public decimal CommissionPercent { get; set; }
        public PointDto Location { get; set; }
        public PictureVDto Picture { get; set; }
        public PictureVDto Icon { get; set; }
        public CodeVDto Type { get; set; }
        public CityVDto City { get; set; }
        public List<UserVDto> Users { get; set; }


    }
}
