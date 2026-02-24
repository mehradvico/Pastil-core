using Application.Common.Dto.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.UserBankCardSrv.Dto
{
    public class UserBankCardApproveDto : Id_FieldDto
    {
        public bool Approved { get; set; }
        public string AdminDetail { get; set; }
    }
}
