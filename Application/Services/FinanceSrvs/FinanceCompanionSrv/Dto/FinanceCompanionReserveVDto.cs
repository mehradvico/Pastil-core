using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.FinanceCompanionSrv.Dto
{
    public class FinanceCompanionReserveVDto
    {
        public string ReserveId { get; set; }
        public string ReserveCode { get; set; }
        public string BookerFullName { get; set; }
        public double PaymentPrice { get; set; }
        public decimal CommissionPercent { get; set; }
        public double CompanionShare { get; set; }
        public double SiteShare { get; set; }
        public string StatusLabel { get; set; }
        public bool IsPansion { get; set; }
        // true وقتی این ردیف یک «مشاوره آنلاین» (پکیج کانال × مدت) است؛ ReserveId در این حالت شناسه‌ی خرید مشاوره است
        public bool IsConsultation { get; set; }
        // true وقتی ردیف «ثبت‌نام دوره‌ی مدرسه» است؛ PackageName نام دوره است
        public bool IsSchool { get; set; }
        public string PackageName { get; set; }
        public int DurationMinutes { get; set; }
        public int ChannelId { get; set; }
        public bool Permitted { get; set; }
    }
}
