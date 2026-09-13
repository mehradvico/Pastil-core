using System;
using System.Collections.Generic;

namespace Application.Services.CompanionSrv.CompanionReserveSrv.Dto
{
    public class CompanionReserveBatchVDto
    {
        public long Id { get; set; }
        public DateTime CreateDate { get; set; }
        public double TotalPrePaymentPrice { get; set; }
        public double TotalPackagePrice { get; set; }
        public List<CompanionReserveVDto> Items { get; set; }
    }
}
