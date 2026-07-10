using Application.Common.Dto.Field;
using Application.Common.Dto.LocationPoint;
using Application.Services.Accounting.DriverSrv.Dto;
using Application.Services.Accounting.UserPetSrv.Dto;
using Application.Services.Dto;
using Application.Services.LocationFields.CitySrv.Dto;
using Application.Services.Order.RebateSrv.Dto;
using Application.Services.ProductSrvs.WalletSrv.Dto;
using Application.Services.Setting.CodeSrv.Dto;
using Application.Services.TripSrv.TripOptionSrv.Dto;
using Application.Services.TripSrv.TripStopSrv.Dto;
using Entities.Entities;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;

namespace Application.Services.TripSrv.TripSrv.Dto
{
    public class TripVDto : Id_FieldDto
    {

        public PointDto Origin { get; set; }
        public PointDto Destination { get; set; }
        public PointDto SecondDestination { get; set; }
        public long RouteLength { get; set; }
        public long? FromCityId { get; set; }
        public int StopTime { get; set; }
        public bool RoundTrip { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public long? DriverId { get; set; }
        public long PassengerId { get; set; }
        public double Price { get; set; }
        public string UserDetail { get; set; }
        public string UserComment { get; set; }
        public int UserRate { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? TripStartDateTime { get; set; }
        public bool IsOnline { get; set; }
        public long DriverStatusId { get; set; }
        public long TripStatusId { get; set; }
        public bool IsPaid { get; set; }
        public DateTime? ManualPayDate { get; set; }
        public string ConnectionId { get; set; }
        public string UserToken { get; set; }
        public long UserPetId { get; set; }
        public long UserId { get; set; }
        public double DriverShare { get; set; }
        public double SiteShare { get; set; }
        public double Discount { get; set; }
        public double PaymentPrice { get; set; }
        public long? RebateId { get; set; }
        public double RebatePrice { get; set; }
        public bool FromWallet { get; set; }
        public double WalletPrice { get; set; }
        public CodeVDto DriverStatus { get; set; }
        public CodeVDto TripStatus { get; set; }
        public UserPetVDto UserPet { get; set; }
        public DriverVDto Driver { get; set; }
        public UserVDto User { get; set; }
        public CityVDto FromCity { get; set; }
        public RebateVDto Rebate { get; set; }
        public WalletVDto Wallet { get; set; }
        public TripStopVDto TripStop { get; set; }
        public List<TripOptionVDto> TripOptions { get; set; }
    }
}
