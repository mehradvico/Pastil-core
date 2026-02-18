using Application.Common.Dto.Input;
using Application.Services.Accounting.FinanceSrv.Iface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Accounting.FinanceSrv.Dto
{
    public class FinanceInputDto : BaseInputDto, IFinanceSearchFields
    {
        public bool? IsCompanion { get; set; }
    }
}
