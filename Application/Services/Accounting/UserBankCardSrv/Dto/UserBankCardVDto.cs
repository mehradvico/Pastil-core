using Application.Common.Dto.Field;
using Application.Services.CommonSrv.BankCardSrv.Dto;
using Application.Services.Dto;
using Entities.Entities;
using Entities.Entities.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Accounting.UserBankCardSrv.Dto
{
    public class UserBankCardVDto : Id_FieldDto
    {
        public long UserId { get; set; }
        public string CardNumber { get; set; }
        public string ShebaNumber { get; set; }
        public long BankCardId { get; set; }
        public string CardHolderName { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public bool Approved { get; set; }
        public string AdminDetail { get; set; }

        public BankCardVDto BankCard { get; set; }
        public UserVDto User { get; set; }
    }
}
