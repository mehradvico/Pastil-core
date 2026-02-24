using Application.Common.Dto.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.BankCardSrv.Dto
{
    public class BankCardVDto : Id_FieldDto
    {
        public string BankName { get; set; }
        public string CardPrefix { get; set; }
    }
}
