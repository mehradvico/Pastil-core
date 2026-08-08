using Application.Common.Dto.Field;
using Application.Services.CompanionSrv.CompanionReserveSrv.Dto;
using Application.Services.CompanionSrvs.CompanionInsurancePackageSaleSrv.Dto;
using Application.Services.Content.CargoSrv.Dto;
using Application.Services.Dto;
using Application.Services.Order.ProductOrderSrv.Dto;
using Application.Services.Order.PaymentSrv.Dto;
using Application.Services.PansionSrvs.PansionReserveSrv.Dto;
using Application.Services.TripSrv.TripSrv.Dto;
using Entities.Entities;
using Entities.Entities.CompanionField;
using System;

namespace Application.Services.ProductSrvs.WalletSrv.Dto
{
    public class WalletVDto : Name_FieldDto
    {
        public bool IsIncrease { get; set; }
        public double Amount { get; set; }
        public DateTime CreateDate { get; set; }
        public long UserId { get; set; }
        public long? PaymentId { get; set; }
        public string ProductOrderId { get; set; }
        public long? CompanionReserveId { get; set; }
        public long? PansionReserveId { get; set; }
        public long? TripId { get; set; }
        public long? CargoId { get; set; }
        public long? CompanionInsurancePackageSaleId { get; set; }
        public long? PastilAiSubscriptionId { get; set; }
        public bool Painding { get; set; }

        public UserVDto User { get; set; }
        public PaymentVDto Payment { get; set; }
        public ProductOrderVDto ProductOrder { get; set; }
        public CompanionReserveVDto CompanionReserve { get; set; }
        public PansionReserveVDto PansionReserveVDto { get; set; }
        public TripVDto Trip { get; set; }
        public CargoVDto Cargo { get; set; }
        public CompanionInsurancePackageSaleVDto CompanionInsurancePackageSale { get; set; }
    }
}
