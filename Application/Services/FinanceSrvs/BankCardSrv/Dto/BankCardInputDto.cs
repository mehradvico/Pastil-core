using Application.Common.Dto.Input;
using Application.Services.FinanceSrvs.BankCardSrv.Iface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.BankCardSrv.Dto
{
    public class BankCardInputDto : BaseInputDto, IBankCardSearchFields
    {
    }
}
