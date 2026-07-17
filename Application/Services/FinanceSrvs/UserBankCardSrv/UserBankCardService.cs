using AngleSharp.Dom;
using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Helpers;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Services.FinanceSrvs.UserBankCardSrv.Dto;
using Application.Services.FinanceSrvs.UserBankCardSrv.Iface;
using Application.Services.Setting.NoticeSrv;
using Application.Services.Setting.NoticeSrv.Dto;
using Application.Services.Setting.NoticeSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.UserBankCardSrv
{
    public class UserBankCardService : CommonSrv<UserBankCard, UserBankCardDto>, IUserBankCardService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ICurrentUserHelper _currentUser;
        private readonly INoticeService _noticeService;
        public UserBankCardService(IDataBaseContext _context, ICurrentUserHelper currentUser, IMapper mapper, INoticeService noticeService) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this._currentUser = currentUser;
            this._noticeService = noticeService;
        }

        public async Task<BaseResultDto<UserBankCardVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.UserBankCards.Include(s => s.User).Include(s => s.BankCard).Where(s => !s.Deleted).FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);
            if (item != null)
                return new BaseResultDto<UserBankCardVDto>(true, mapper.Map<UserBankCardVDto>(item));
            return new BaseResultDto<UserBankCardVDto>(false, mapper.Map<UserBankCardVDto>(item));
        }

        public UserBankCardSearchDto Search(UserBankCardInputDto baseSearchDto)
        {
            var model = _context.UserBankCards.Include(s => s.User).Include(s => s.BankCard).AsQueryable().Where(s => !s.Deleted);
            if (baseSearchDto.UserId.HasValue)
            {
                model = model.Where(s => s.UserId == baseSearchDto.UserId);
            }
            if (baseSearchDto.Approved.HasValue)
            {
                model = model.Where(s => s.Approved == baseSearchDto.Approved.Value);
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
            return new UserBankCardSearchDto(baseSearchDto, model, mapper);
        }

        public override async Task<BaseResultDto<UserBankCardDto>> InsertAsyncDto(UserBankCardDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<UserBankCardDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                    return modelCheker;

                if (string.IsNullOrWhiteSpace(dto.CardHolderName))
                    return new BaseResultDto<UserBankCardDto>(isSuccess: false, val: Resource.Notification.PleaseEnterCardHolderName, data: dto);

                if (string.IsNullOrWhiteSpace(dto.CardNumber))
                    return new BaseResultDto<UserBankCardDto>(isSuccess: false, val: Resource.Notification.PleaseEnterCardNumber, data: dto);

                var cardNumber = new string(dto.CardNumber.Where(char.IsDigit).ToArray());

                if (cardNumber.Length != 16)
                    return new BaseResultDto<UserBankCardDto>(isSuccess: false, val: Resource.Notification.CartNumberMustBe16Digit, data: dto);

                var bin = cardNumber.Substring(0, 6);

                var bankCard = await _context.BankCards.AsNoTracking().FirstOrDefaultAsync(x => x.CardPrefix == bin);

                if (bankCard == null)
                    return new BaseResultDto<UserBankCardDto>(isSuccess: false, val: Resource.Notification.BankCartIsNotAvailable, data: dto);

                string shebaToStore = null;

                if (!string.IsNullOrWhiteSpace(dto.ShebaNumber))
                {
                    var shebaRaw = dto.ShebaNumber.Trim();

                    if (shebaRaw.StartsWith("IR", StringComparison.OrdinalIgnoreCase))
                        shebaRaw = shebaRaw.Substring(2);

                    shebaRaw = new string(shebaRaw.Where(char.IsDigit).ToArray());

                    if (shebaRaw.Length != 24)
                        return new BaseResultDto<UserBankCardDto>(isSuccess: false, val: Resource.Notification.ShebaNumberMustBe24Digit, data: dto);

                    shebaToStore = "IR" + shebaRaw;
                }

                var duplicate = await _context.UserBankCards.AnyAsync(x => !x.Deleted && x.CardNumber == cardNumber);

                if (duplicate)
                    return new BaseResultDto<UserBankCardDto>(isSuccess: false, val: Resource.Notification.DuplicateValue, data: dto);

                var item = mapper.Map<UserBankCard>(dto);
                item.CardNumber = cardNumber;
                item.ShebaNumber = shebaToStore;
                item.BankCardId = bankCard.Id;

                item.CreateDate = DateTime.Now;
                item.LastUpdateDate = DateTime.Now;
                item.Approved = false;
                item.AdminDetail = null;

                await _context.UserBankCards.AddAsync(item);
                await _context.SaveChangesAsync();
                await _noticeService.CreateAsync(new NoticeCreateDto { Label = NoticeTypeLabels.UserBankCardSubmitted, ActorUserId = item.UserId, ReferenceType = "UserBankCard", ReferenceId = item.Id, DeduplicationKey = $"{NoticeTypeLabels.UserBankCardSubmitted}:{item.Id}" });

                return new BaseResultDto<UserBankCardDto>(true, mapper.Map<UserBankCardDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<UserBankCardDto>(false, ex.Message, dto);
            }
        }

        public override BaseResultDto UpdateDto(UserBankCardDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<UserBankCardDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                    return modelCheker;

                var isAdmin = _currentUser.CurrentUser.RoleEnum == RoleEnum.Admin.ToString();
                var item = _context.UserBankCards.FirstOrDefault(x => x.Id == dto.Id && !x.Deleted);

                if (item == null)
                    return new BaseResultDto(isSuccess: false, val: Resource.Notification.NothingFound);

                if (!isAdmin)
                {
                    if (item.UserId != _currentUser.CurrentUser.UserId)
                        return new BaseResultDto(isSuccess: false, val: Resource.Notification.AccessDenied);

                    if (item.LastUpdateDate != item.CreateDate)
                        return new BaseResultDto(isSuccess: false, val: Resource.Notification.EditIsNotAvailable);
                }

                string shebaToStore = null;

                if (!string.IsNullOrWhiteSpace(dto.ShebaNumber))
                {
                    var shebaRaw = dto.ShebaNumber.Trim();

                    if (shebaRaw.StartsWith("IR", StringComparison.OrdinalIgnoreCase))
                        shebaRaw = shebaRaw.Substring(2);

                    shebaRaw = new string(shebaRaw.Where(char.IsDigit).ToArray());

                    if (shebaRaw.Length != 24)
                        return new BaseResultDto(isSuccess: false, val: Resource.Notification.ShebaNumberMustBe24Digit);

                    shebaToStore = "IR" + shebaRaw;
                }

                var createDate = item.CreateDate;
                var userId = item.UserId;
                var deleted = item.Deleted;

                mapper.Map(dto, item);

                item.UserId = userId;
                item.CreateDate = createDate;
                item.LastUpdateDate = DateTime.Now;
                item.ShebaNumber = shebaToStore;
                item.Approved = false;
                item.Deleted = deleted;

                _context.UserBankCards.Update(item);
                _context.SaveChanges();
                return new BaseResultDto(isSuccess: true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(isSuccess: false, val: ex.Message);
            }
        }

        public async Task<BaseResultDto> UpdateAsyncDto(UserBankCardDto dto)
        {
            var result = UpdateDto(dto);
            if (result.IsSuccess)
                await _noticeService.CreateAsync(new NoticeCreateDto { Label = NoticeTypeLabels.UserBankCardUpdated, ActorUserId = _currentUser.CurrentUser.UserId, ReferenceType = "UserBankCard", ReferenceId = dto.Id, DeduplicationKey = $"{NoticeTypeLabels.UserBankCardUpdated}:{dto.Id}:{DateTime.UtcNow.Ticks}" });
            return result;
        }

        public async Task<BaseResultDto> UpdateUserBankCardApproveAsyncDto(UserBankCardApproveDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<UserBankCardApproveDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                    return modelCheker;

                var item = await _context.UserBankCards.FirstOrDefaultAsync(x => x.Id == dto.Id && !x.Deleted);

                if (item == null)
                    return new BaseResultDto(isSuccess: false, val: Resource.Notification.NothingFound);

                if (!dto.Approved && string.IsNullOrWhiteSpace(dto.AdminDetail))
                    return new BaseResultDto(isSuccess: false, val: Resource.Notification.PleaseEnterTheAdminDetail);

                item.Approved = dto.Approved;
                item.AdminDetail = dto.Approved ? null : dto.AdminDetail.Trim();

                _context.UserBankCards.Update(item);
                await _context.SaveChangesAsync();

                return new BaseResultDto(isSuccess: true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(isSuccess: false, val: ex.Message);
            }
        }
    }
}
