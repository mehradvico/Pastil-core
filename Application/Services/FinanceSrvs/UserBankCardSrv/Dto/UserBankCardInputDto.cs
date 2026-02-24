using Application.Common.Dto.Input;
using Application.Services.FinanceSrvs.UserBankCardSrv.Iface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.UserBankCardSrv.Dto
{
    public class UserBankCardInputDto : BaseInputDto, IUserBankCardSearchFields
    {
        public long? UserId { get; set; }
        public long? BankCardId { get; set; }
        public bool? Approved { get; set; }
    }
}
