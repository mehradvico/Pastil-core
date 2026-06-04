using Application.Common.Dto.Field;
using System;

namespace Application.Services.CompanionSrvs.CompanionReserveSrv.Dto
{
    public class CompanionReserveExcelVDto : Id_FieldDto
    {
        public string ReserveType { get; set; }

        public string BookerName { get; set; }
        public string PetType { get; set; }
        public string CompanionName { get; set; }
        public string AssistanceName { get; set; }
        public string OperatorName { get; set; }
        public double PrePaymentPrice { get; set; }
        public double PackagePrice { get; set; }
        public double OperatorWagesPrice { get; set; }
        public double OperatorStuffPrice { get; set; }
        public double OperatorFinalPrice { get; set; }
        public double Discount { get; set; }
        public double RebatePrice { get; set; }
        public decimal ComissionPercent { get; set; }
        public double CompanionShare { get; set; }
        public double SiteShare { get; set; }
        public double PaymentPrice { get; set; }
        public string OperatorDetail { get; set; }
        public string StateId { get; set; }
        public DateTime CreateDate { get; set; }
    }
}