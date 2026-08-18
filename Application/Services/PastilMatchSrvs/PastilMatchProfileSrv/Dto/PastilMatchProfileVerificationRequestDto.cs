using Application.Common.Dto.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchProfileSrv.Dto
{
    public class PastilMatchProfileVerificationRequestDto : Id_FieldDto
    {
        /// <summary>
        /// شناسه پروفایل پاستیل مچ. فیلد Id برای سازگاری با کلاینت‌های قدیمی
        /// همچنان پشتیبانی می‌شود.
        /// </summary>
        public long? PastilMatchProfileId { get; set; }
    }
}
