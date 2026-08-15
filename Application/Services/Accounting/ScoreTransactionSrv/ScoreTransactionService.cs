using Application.Common.Dto.Result;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Services.Accounting.ScoreTransactionSrv.Dto;
using Application.Services.Accounting.ScoreTransactionSrv.Iface;
using Application.Services.Setting.CodeSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Accounting.ScoreTransactionSrv
{
    public class ScoreTransactionService : CommonSrv<ScoreTransaction, ScoreTransactionDto>, IScoreTransactionService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ICurrentUserHelper _currentUserHelper;
        private readonly ICodeService _codeService;
        public ScoreTransactionService(IDataBaseContext _context, IMapper mapper, ICurrentUserHelper currentUserHelper, ICodeService codeService) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this._currentUserHelper = currentUserHelper;
            this._codeService = codeService;
        }

        public async Task<BaseResultDto<ScoreTransactionVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.ScoreTransactions.Include(s => s.User).Include(s => s.TransactionType).FirstOrDefaultAsync(s => s.Id == id);
            if (item != null)
            {
                return new BaseResultDto<ScoreTransactionVDto>(true, mapper.Map<ScoreTransactionVDto>(item));
            }
            return new BaseResultDto<ScoreTransactionVDto>(false, mapper.Map<ScoreTransactionVDto>(item));
        }

        public ScoreTransactionSearchDto Search(ScoreTransactionInputDto baseSearchDto)
        {
            var model = _context.ScoreTransactions.Include(s => s.User).Include(s => s.TransactionType).AsQueryable();

            if (baseSearchDto.UserId.HasValue)
            {
                model = model.Where(s => s.UserId == baseSearchDto.UserId.Value);
            }
            if (baseSearchDto.TransactionTypeId.HasValue)
            {
                model = model.Where(s => s.TransactionTypeId == baseSearchDto.TransactionTypeId.Value);
            }
            switch (baseSearchDto.SortBy)
            {
                case Common.Enumerable.SortEnum.New:
                    {
                        model = model.OrderByDescending(s => s.Id);
                        break;
                    }
                case Common.Enumerable.SortEnum.Old:
                    {
                        model = model.OrderBy(s => s.Id);
                        break;
                    }
                default:
                    break;
            }
            return new ScoreTransactionSearchDto(baseSearchDto, model, mapper);
        }

        public async Task<BaseResultDto<bool>> RedeemRewardAsync(long rewardId)
        {
            var currentUser = _currentUserHelper.CurrentUser;
            if (currentUser == null)
                return new BaseResultDto<bool>(false, Resource.Notification.AccessDenied, false);

            var reward = await _context.ClubRewards.Include(s => s.Rebate).FirstOrDefaultAsync(s => s.Id == rewardId && s.Active && !s.Deleted);

            if (reward == null || reward.Rebate == null)
                return new BaseResultDto<bool>(isSuccess: false, val: Resource.Notification.NothingFound, data: false);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == currentUser.UserId && !u.Deleted);
            if (user == null)
                return new BaseResultDto<bool>(isSuccess: false, val: Resource.Notification.UserNotFound, data: false);

            if (user.CurrentScore < reward.RequiredScore)
                return new BaseResultDto<bool>(isSuccess: false, val: Resource.Notification.ScoreNotEnough, data: false);

            var spentCode = await _codeService.GetByLabelAsync(ScoreTransactionType.ScoreTransactionType_Spent.ToString());

            user.CurrentScore -= reward.RequiredScore;

            var scoreLog = new ScoreTransaction
            {
                UserId = user.Id,
                Amount = -reward.RequiredScore,
                TransactionTypeId = spentCode.Id,
                ReferenceId = reward.Id.ToString(),
                Description = $"Redeem Reward: {reward.Name}",
                CreateDate = DateTime.Now
            };
            await _context.ScoreTransactions.AddAsync(scoreLog);

            var newUserRebate = new Rebate
            {
                Name = $"هدیه پاستیل کلاب: {reward.Name}",
                UserId = user.Id,
                TypeId = reward.Rebate.TypeId,
                CodeValue = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                PriceValue = reward.Rebate.PriceValue,
                IsPriceRebate = reward.Rebate.IsPriceRebate,
                MinCartPrice = reward.Rebate.MinCartPrice,
                ProductId = reward.Rebate.ProductId,
                Active = true,
                Deleted = false,
                UseCount = 1,
                UsedCount = 0,
                MaxUsePerUser = 1,
                StartDatetime = DateTime.Now,
                EndDatetime = DateTime.Now.AddDays(reward.ValidityDays)
            };
            await _context.Rebate.AddAsync(newUserRebate);
            await _context.SaveChangesAsync();

            return new BaseResultDto<bool>(isSuccess: true, val: Resource.Notification.Success, data: true);
        }

        public async Task<BaseResultDto<bool>> AddScoreAsync(long userId, double amount, ScoreTransactionType type, string referenceId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.Deleted);
            if (user == null) return new BaseResultDto<bool>(isSuccess: false, val: Resource.Notification.UserNotFound, data: false);

            var typeCode = await _codeService.GetByLabelAsync(type.ToString());

            var transaction = new ScoreTransaction
            {
                UserId = userId,
                Amount = amount,
                TransactionTypeId = typeCode.Id,
                ReferenceId = referenceId,
                CreateDate = DateTime.Now,
                Description = $"امتیاز دریافتی برای {type.ToString().Replace("ScoreTransactionType_", "")}"
            };

            user.CurrentScore += amount;

            await _context.ScoreTransactions.AddAsync(transaction);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return new BaseResultDto<bool>(isSuccess: true, val: Resource.Notification.Success, data: true);
        }
    }
}
