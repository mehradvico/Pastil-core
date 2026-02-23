using AngleSharp.Dom;
using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Helpers;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Services.Accounting.UserBankCardSrv.Dto;
using Application.Services.Accounting.UserBankCardSrv.Iface;
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

namespace Application.Services.Accounting.UserBankCardSrv
{
    public class UserBankCardService : CommonSrv<UserBankCard, UserBankCardDto>, IUserBankCardService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ICurrentUserHelper _currentUser;
        public UserBankCardService(IDataBaseContext _context, ICurrentUserHelper currentUser, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this._currentUser = currentUser;
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

                return new BaseResultDto<UserBankCardDto>(true, mapper.Map<UserBankCardDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<UserBankCardDto>(false, ex.Message, dto);
            }
        }

        private static bool IsValidIranianSheba(string iban)
        {
            if (string.IsNullOrWhiteSpace(iban))
                return false;

            iban = iban.Trim().Replace(" ", "").ToUpperInvariant();

            if (iban.Length != 26) return false;
            if (!iban.StartsWith("IR")) return false;
            if (!iban.Skip(2).All(char.IsDigit)) return false;

            var rearranged = iban.Substring(4) + iban.Substring(0, 4);
            var sb = new StringBuilder(rearranged.Length * 2);
            foreach (var ch in rearranged)
            {
                if (char.IsDigit(ch)) sb.Append(ch);
                else sb.Append((ch - 'A' + 10).ToString());
            }
            int mod = 0;
            foreach (var ch in sb.ToString())
            {
                mod = (mod * 10 + (ch - '0')) % 97;
            }
            return mod == 1;
        }

        public override BaseResultDto UpdateDto(UserBankCardDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<UserBankCardDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                    return modelCheker;

                var isAdmin = _currentUser.CurrentUser.RoleEnum == RoleEnum.Admin.ToString();

                var Item = _context.UserBankCards.FirstOrDefault(x => x.Id == dto.Id && !x.Deleted);
                if (Item == null)
                    return new BaseResultDto(isSuccess: false, val: Resource.Notification.NothingFound);

                if (!isAdmin)
                {
                    if (Item.LastUpdateDate != Item.CreateDate)
                        return new BaseResultDto(isSuccess: false, val: Resource.Notification.EditIsNotAvailable);

                    if (dto.UserId != Item.UserId)
                        return new BaseResultDto(isSuccess: false, val: Resource.Notification.AccessDenied);

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

                    Item.ShebaNumber = shebaToStore;
                    Item.Approved = false;
                    Item.LastUpdateDate = DateTime.Now;

                    _context.SaveChanges();
                    return new BaseResultDto(isSuccess: true);
                }
                else
                {
                    var item = mapper.Map<UserBankCard>(dto);
                    item.CreateDate = Item.CreateDate;
                    item.LastUpdateDate = DateTime.Now;
                    item.Approved = false;

                    _context.Entry(Item).CurrentValues.SetValues(item);
                    _context.SaveChanges();
                    return new BaseResultDto(isSuccess: true);
                }
            }
            catch (Exception ex)
            {
                return new BaseResultDto(isSuccess: false, val: ex.Message);
            }
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
