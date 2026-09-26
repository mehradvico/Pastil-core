using Application.Common.Dto.Field;
using System;

namespace Application.Services.CompanionSrvs.CompanionReserveSrv.Dto
{
    public class CompanionReserveOperatorDto : Id_FieldDto
    {
        public long OperatorStateId { get; set; }
        public DateTime? OperatorChangeStateDate { get; set; }
        public string OperatorDetail { get; set; }
        public double OperatorStuffPrice { get; set; }
        public double OperatorWagesPrice { get; set; }
        // کاربر هزینه‌ی نهایی را (هنوز) پرداخت نکرده است؛ فقط با وضعیت «کامل‌شده» و مبلغ > ۰ معنی دارد
        public bool OperatorUnpaid { get; set; }
    }
}
