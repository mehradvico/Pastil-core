using Application.Common.Dto.Result;
using Application.Common.Enumerable.Code;
using Application.Common.Interface;
using Application.Services.Accounting.ScoreTransactionSrv.Dto;
using Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Accounting.ScoreTransactionSrv.Iface
{
    public interface IScoreTransactionService : ICommonSrv<ScoreTransaction, ScoreTransactionDto>
    {
        ScoreTransactionSearchDto Search(ScoreTransactionInputDto baseSearchDto);
        Task<BaseResultDto<ScoreTransactionVDto>> FindAsyncVDto(long id);
        Task<BaseResultDto<bool>> RedeemRewardAsync(long rewardId);
        Task<BaseResultDto<bool>> AddScoreAsync(long userId, double amount, ScoreTransactionType type, string referenceId);
    }
}
