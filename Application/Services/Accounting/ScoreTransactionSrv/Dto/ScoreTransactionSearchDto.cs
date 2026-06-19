using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Services.Accounting.ScoreTransactionSrv.Dto;
using Application.Services.Accounting.ScoreTransactionSrv.Iface;
using AutoMapper;
using Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Accounting.ScoreTransactionSrv.Dto
{
    public class ScoreTransactionSearchDto : BaseSearchDto<ScoreTransaction, ScoreTransactionVDto>, IScoreTransactionSearchFields
    {
        public ScoreTransactionSearchDto(ScoreTransactionInputDto dto, IQueryable<ScoreTransaction> list, IMapper mapper) : base(dto, list, mapper)
        {
            this.UserId = dto.UserId;
            this.TransactionTypeId = dto.TransactionTypeId;
        }

        public long? UserId { get; set; }
        public long? TransactionTypeId { get; set; }
    }
}
