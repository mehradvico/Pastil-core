using Application.Common.Dto.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Accounting.UserBankCardSrv.Dto
{
    public class UserBankCardDto : Id_FieldDto
    {
        public long UserId { get; set; }
        public string CardNumber { get; set; }
        public string ShebaNumber { get; set; }
        public long BankCardId { get; set; }
        public string CardHolderName { get; set; }
        public bool Approved { get; set; }
    }
}
