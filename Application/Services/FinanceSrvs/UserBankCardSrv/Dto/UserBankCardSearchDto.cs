using Application.Common.Dto.Result;
using Application.Services.FinanceSrvs.UserBankCardSrv.Iface;
using AutoMapper;
using Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.UserBankCardSrv.Dto
{
    public class UserBankCardSearchDto : BaseSearchDto<UserBankCard, UserBankCardVDto>, IUserBankCardSearchFields
    {
        public UserBankCardSearchDto(UserBankCardInputDto dto, IQueryable<UserBankCard> list, IMapper mapper) : base(dto, list, mapper)
        {
            this.UserId = dto.UserId;
            this.BankCardId = dto.BankCardId;
            this.Approved = dto.Approved;
        }
        public long? UserId { get; set; }
        public long? BankCardId { get; set; }
        public bool? Approved { get; set; }

    }
}
