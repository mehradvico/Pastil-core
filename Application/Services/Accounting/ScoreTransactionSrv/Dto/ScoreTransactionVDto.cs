using Application.Common.Dto.Field;
using Application.Services.Accounting.UserPetPictureSrv.Dto;
using Application.Services.Dto;
using Application.Services.Order.RebateSrv.Dto;
using Application.Services.Setting.CodeSrv.Dto;
using Entities.Entities;
using Entities.Entities.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Accounting.ScoreTransactionSrv.Dto
{
    public class ScoreTransactionVDto : Id_FieldDto
    {
        public long UserId { get; set; }
        public double Amount { get; set; }
        public string Description { get; set; }
        public DateTime CreateDate { get; set; }
        public long TransactionTypeId { get; set; }
        public string ReferenceId { get; set; }

        public UserMinVDto User { get; set; }
        public CodeVDto TransactionType { get; set; }
    }
}
