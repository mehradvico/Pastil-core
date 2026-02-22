using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Enumerable.Message;
using Application.Common.Helpers;
using Application.Common.Helpers.Iface;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.CompanionSrvs.CompanionReserveSrv.Dto;
using Application.Services.Order.RebateSrv.Iface;
using Application.Services.PansionSrvs.PansionReserveSrv.Dto;
using Application.Services.PansionSrvs.PansionReserveSrv.Iface;
using Application.Services.ProductSrvs.WalletSrv.Dto;
using Application.Services.ProductSrvs.WalletSrv.IFace;
using Application.Services.Setting.CodeSrv.Iface;
using Application.Services.Setting.MessageSenderSrv.Iface;
using Application.Services.Setting.NoticeSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Entities.Entities.PansionField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PansionSrvs.PansionReserveSrv
{
    public class PansionReserveService : CommonSrv<PansionReserve, PansionReserveDto>, IPansionReserveService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ICodeService _codeService;
        private readonly INoticeService _notificationService;
        private readonly IMessageSenderService _messageSender;
        private readonly ICurrentUserHelper _currentUser;
        private readonly IAdminSettingHelper _adminSettingHelper;
        private readonly IRebateService _rebateService;
        private readonly IWalletService _walletService;
        private readonly IPushNotificationService _pushNotificationService;
        public PansionReserveService(IDataBaseContext _context, IPushNotificationService pushNotificationService, IMapper mapper, IWalletService walletService, IRebateService rebateService, IAdminSettingHelper adminSettingHelper, ICodeService codeService, IMessageSenderService messageSender, ICurrentUserHelper currentUser, INoticeService notificationService) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this._codeService = codeService;
            this._messageSender = messageSender;
            this._currentUser = currentUser;
            this._notificationService = notificationService;
            this._adminSettingHelper = adminSettingHelper;
            this._rebateService = rebateService;
            this._walletService = walletService;
            this._pushNotificationService = pushNotificationService;
        }
        public async Task<BaseResultDto<PansionReserveVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.PansionReserves.Include(s => s.Status).Include(s => s.Booker).Include(s => s.UserPet).Include(s => s.Rebate).FirstOrDefaultAsync(s => s.Id == id);
            if (item != null)
            {
                return new BaseResultDto<PansionReserveVDto>(true, mapper.Map<PansionReserveVDto>(item));
            }
            return new BaseResultDto<PansionReserveVDto>(false, mapper.Map<PansionReserveVDto>(item));
        }

        public PansionReserveSearchDto Search(PansionReserveInputDto baseSearchDto)
        {
            var model = _context.PansionReserves.Include(s => s.Pansion).Include(s => s.Status).Include(s => s.Booker).Include(s => s.UserPet).AsQueryable();

            if (baseSearchDto.BookerId.HasValue)
            {
                model = model.Where(s => s.BookerId == baseSearchDto.BookerId.Value);
            }
            if (baseSearchDto.PansionId.HasValue)
            {
                model = model.Where(s => s.PansionId == baseSearchDto.PansionId.Value);
            }
            if (baseSearchDto.UserPetId.HasValue)
            {
                model = model.Where(s => s.UserPetId == baseSearchDto.UserPetId.Value);
            }
            if (baseSearchDto.CompanionId.HasValue)
            {
                model = model.Where(s => s.Pansion.CompanionId == baseSearchDto.CompanionId.Value);
            }
            if (baseSearchDto.StatusId.HasValue)
            {
                model = model.Where(s => s.StatusId == baseSearchDto.StatusId.Value);
            }
            if (baseSearchDto.IsSchool.HasValue)
            {
                model = model.Where(s => s.Pansion.IsSchool == baseSearchDto.IsSchool.Value);
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
            return new PansionReserveSearchDto(baseSearchDto, model, mapper);
        }

        public override async Task<BaseResultDto<PansionReserveDto>> InsertAsyncDto(PansionReserveDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<PansionReserveDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    var item = mapper.Map<PansionReserve>(dto);
                    item.IsCancel = false;
                    //bool existed = await _context.PansionReserves.AnyAsync(s => s.PansionId == dto.PansionId && s.BookerId == dto.BookerId && s.UserPetId == dto.UserPetId && s.IsReserved && !s.IsCancel && ((s.StartTime == dto.StartTime && s.EndTime == dto.EndTime) || (s.FromDate == dto.FromDate && s.EndTime == dto.EndTime)));
                    //if (existed)
                    //{
                    //    return new BaseResultDto<PansionReserveDto>(false, Resource.Notification.HaveBeenReserved, dto);
                    //}
                    item.CreateDate = DateTime.Now;
                    if (_currentUser.CurrentUser.RoleEnum == RoleEnum.Admin.ToString())
                    {
                        item.IsReserved = true;
                    }
                    else
                    {
                        item.IsReserved = false;
                    }
                    var pansion = await _context.Pansions.FirstOrDefaultAsync(s => s.Id == dto.PansionId);
                    var hasSchoolInputs = !string.IsNullOrWhiteSpace(dto.StartTime) && !string.IsNullOrWhiteSpace(dto.EndTime) && dto.SchoolCreateDate.HasValue;
                    var hasPansionInputs = dto.FromDate.HasValue && dto.ToDate.HasValue;

                    if (!hasSchoolInputs && !hasPansionInputs)
                    {
                        return new BaseResultDto<PansionReserveDto>(false, Resource.Notification.PleaseEnterTimeRange, dto);
                    }

                    if (hasSchoolInputs)
                    {
                        item.FromDate = null;
                        item.ToDate = null;

                        if (!TimeSpan.TryParseExact(dto.StartTime, "hh\\:mm", CultureInfo.InvariantCulture, out var startTime) ||
                            !TimeSpan.TryParseExact(dto.EndTime, "hh\\:mm", CultureInfo.InvariantCulture, out var endTime))
                        {
                            return new BaseResultDto<PansionReserveDto>(false, Resource.Notification.InvalidTimeFormat, dto);
                        }

                        if (startTime >= endTime)
                        {
                            return new BaseResultDto<PansionReserveDto>(false, Resource.Notification.ToTimeMustBeBiggerThanFromTime, dto);
                        }

                        var totalHours = (endTime - startTime).TotalHours;

                        item.HourCount = (int)Math.Ceiling(totalHours);
                        item.DayCount = 0;

                        item.Price = pansion.SchoolPrice * item.HourCount;
                        item.PaymentPrice = item.Price;
                    }

                    if (hasPansionInputs)
                    {
                        item.StartTime = null;
                        item.EndTime = null;
                        item.SchoolCreateDate = null;

                        var from = dto.FromDate.Value.Date;
                        var to = dto.ToDate.Value.Date;

                        if (to < from)
                        {
                            return new BaseResultDto<PansionReserveDto>(false, Resource.Notification.PleaseEnterTimeRange, dto);
                        }

                        item.DayCount = (to - from).Days + 1;
                        item.HourCount = 0;

                        item.Price = pansion.PansionPrice * item.DayCount;
                        item.PaymentPrice = item.Price;
                    }

                    var status = await _codeService.GetIdByLabelAsync(PansionReserveStatusEnum.PansionReserveState_Registered.ToString());
                    item.StatusId = status;
                    await _context.PansionReserves.AddAsync(item);
                    await _context.SaveChangesAsync();

                    var booker = _context.Users.FirstOrDefault(u => u.Id == item.BookerId);
                    var Pansion = _context.Pansions.Include(s => s.Companion).ThenInclude(s => s.Owner).FirstOrDefault(a => a.Id == item.PansionId);
                    var adminMobile = _adminSettingHelper.BaseAdminSetting.AdminMobiles;
                    string nameText = string.Format("{0}_{1}", booker.FirstName, booker.LastName).Replace(" ", "_");

                    string dateOnly = null;
                    if (hasSchoolInputs)
                    {
                        dateOnly = item.SchoolCreateDate?.ToString("yyyy/MM/dd");
                    }
                    else if (hasPansionInputs)
                    {
                        dateOnly = item.FromDate?.ToString("yyyy/MM/dd");
                    }
                    await _messageSender.SendMessageAsync(messageType: MessageTypeEnum.PansionReserveForUser, mobileReceptor: booker.Mobile, emailReceptor: null, token1: Pansion.Name, token2: dateOnly);
                    await _messageSender.SendMessageAsync(messageType: MessageTypeEnum.PansionReserveForPansion, mobileReceptor: Pansion.Companion.Owner.Mobile, emailReceptor: null, token1: Pansion.Name, token2: booker.LastName, token3: dateOnly);
                    await _messageSender.SendMessageAsync(messageType: MessageTypeEnum.PansionReserveForAdmin, mobileReceptor: adminMobile, emailReceptor: null, token1: booker.Id.ToString(), token2: Pansion.Name);
                    await _pushNotificationService.SendPushAsync(pushType: PushTypeEnum.PushRegisterPansionUser, userId: booker.Id, token1: booker.FirstName, token2: Pansion.Name);
                    await _pushNotificationService.SendPushAsync(pushType: PushTypeEnum.PushRegisterPansionCompanion, userId: Pansion.Companion.Owner.Id, token1: nameText);

                    return new BaseResultDto<PansionReserveDto>(true, mapper.Map<PansionReserveDto>(item));
                }

            }
            catch (Exception ex) 
            {
                return new BaseResultDto<PansionReserveDto>(isSuccess: false, val: ex.Message, data: dto);
            }
        }
        public async Task<BaseResultDto> PansionReservePaymentCallback(long? reserveId, bool fromWallet = false)
        {
            try
            {
                var reserve = await _context.PansionReserves.Include(s => s.Booker).Include(s => s.Pansion).AsTracking().FirstOrDefaultAsync(s => s.Id == reserveId);

                if (reserve.FromWallet)
                {
                    var amount = await _walletService.GetAmountValueAsync(reserve.Booker.Id);
                    if (amount >= reserve.WalletPrice)
                    {
                        var walletItem = new WalletDto()
                        {
                            Painding = false,
                            Amount = reserve.WalletPrice,
                            UserId = reserve.Booker.Id,
                            PansionReserveId = reserve.Id
                        };
                        await _walletService.InsertUpdatePansionReserveAsync(walletItem, true);
                    }
                    else
                    {
                        return new BaseResultDto(false);
                    }
                }

                reserve.IsReserved = true;

                var paidStatus = await _codeService.GetIdByLabelAsync(PansionReserveStatusEnum.PansionReserveState_Paid.ToString());
                reserve.StatusId = paidStatus;

                await UpdatePansionReserveCommissionDto(reserve);

                await _context.SaveChangesAsync();

                return new BaseResultDto(true, Resource.Notification.Success);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false);
            }
        }

        public Task UpdatePansionReserveCommissionDto(PansionReserve item)
        {
            if (item == null || item.Pansion == null)
                return Task.CompletedTask;

            decimal total = (decimal)item.PaymentPrice;

            decimal hourlyPercent = item.Pansion.HourlyCommissionPercent;
            decimal dailyPercent = item.Pansion.DailyCommissionPercent;

            var hasHour = item.HourCount > 0;
            var hasDay = item.DayCount > 0;

            decimal sharePercent;

            if (hasHour && !hasDay)
            {
                sharePercent = hourlyPercent;
            }
            else
            {
                sharePercent = dailyPercent;
            }

            decimal siteShare = (total * sharePercent) / 100m;
            decimal companionShare = total - siteShare;

            item.CompanionShare = (double)companionShare;
            item.SiteShare = (double)siteShare;

            return Task.CompletedTask;
        }
        public async Task<BaseResultDto> UpdatePansionReserveCancelDto(PansionReserveCancelDto dto)
        {
            var model = await _context.PansionReserves.FirstOrDefaultAsync(s => s.Id == dto.Id && s.IsReserved);

            if (model == null)
            {
                return new BaseResultDto<PansionReserveCancelDto>(false, null);
            }

            if (_currentUser.CurrentUser.RoleEnum != RoleEnum.Admin.ToString())
            {
                return new BaseResultDto<PansionReserveCancelDto>(false, null);
            }

            if ((model.StatusId == (long)PansionReserveStatusEnum.PansionReserveState_Paid || model.StatusId == (long)PansionReserveStatusEnum.PansionReserveState_Complete)
                && (_currentUser.CurrentUser.RoleId != (long)RoleEnum.Admin || _currentUser.CurrentUser.RoleId == (long)RoleEnum.Customer))
            {
                return new BaseResultDto<PansionReserveCancelDto>(false, Resource.Notification.YouCanNotCancelThisReservation, dto);
            }
            if (dto.IsCancel)
            {
                if (string.IsNullOrWhiteSpace(dto.CancelDetail))
                {
                    return new BaseResultDto<PansionReserveCancelDto>(false, Resource.Notification.PleaseEnterCancelDetail, dto);
                }
                model.IsCancel = true;
                model.CancelDetail = dto.CancelDetail;
                model.CancelDate = DateTime.Now;
            }
            _context.PansionReserves.Update(model);
            await _context.SaveChangesAsync();

            var adminMobile = _adminSettingHelper.BaseAdminSetting.AdminMobiles;
            var booker = _context.Users.FirstOrDefault(u => u.Id == model.BookerId);

            var Pansion = _context.Pansions.Include(s => s.Companion).ThenInclude(s => s.Owner).FirstOrDefault(a => a.Id == model.PansionId);
            await _messageSender.SendMessageAsync(messageType: MessageTypeEnum.PansionReserveCancelForAdmin, mobileReceptor: adminMobile, emailReceptor: null, token1: booker.Id.ToString(), token2: Pansion.Name);
            await _notificationService.InsertNoticeAsync(model.Id, NoticeTypeEnum.NotifType_UserReserveCancell, NoticeUserTypeEnum.NoticeUserType_User);
            return new BaseResultDto<PansionReserveCancelDto>(true, mapper.Map<PansionReserveCancelDto>(model));
        }

        public async Task<BaseResultDto> UpdatePansionReserveStatusDto(PansionReserveStatusDto dto)
        {
            var item = await _context.PansionReserves.FirstOrDefaultAsync(s => s.Id == dto.Id);

            if (item.PaymentPrice == 0)
            {
                return new BaseResultDto<PansionReserveStatusDto>(false, Resource.Notification.TheFinalPriceHasNotYetBeenRecordedForThisReserve, dto);
            }
            item.StatusId = dto.StatusId;

            _context.PansionReserves.Update(item);
            _context.SaveChanges();
            return new BaseResultDto<PansionReserveStatusDto>(true, mapper.Map<PansionReserveStatusDto>(item));
        }



        public async Task<BaseResultDto> SetRebateCodeAsyncDto(PansionReserveRebateCodeDto dto)
        {
            var item = await _context.PansionReserves.AsTracking().FirstOrDefaultAsync(s => s.Id == dto.Id && s.StatusId == (long)PansionReserveStatusEnum.PansionReserveState_Registered);

            if (item == null)
            {
                return new BaseResultDto<PansionReserveRebateCodeDto>(false, Resource.Notification.NothingFound, dto);
            }
            if (string.IsNullOrEmpty(dto.RebateCode))
            {
                return new BaseResultDto(isSuccess: false, val: Resource.Notification.Unsuccess);
            }
            var rebate = _rebateService.GetRebateByCodeAsync(item, dto.RebateCode);
            if (rebate.IsSuccess)
            {
                item.Rebate = null;
                item.RebateId = rebate.Data.Id;
                item.RebatePrice = rebate.Data.FinalPrice;
                item.PaymentPrice = item.PaymentPrice - item.RebatePrice;
                if (item.PaymentPrice < 0)
                {
                    item.PaymentPrice = 0;
                }
                if (item.WalletPrice != 0)
                {
                    item.WalletPrice = item.PaymentPrice;
                }
                _context.PansionReserves.Update(item);
                await _context.SaveChangesAsync();
                return new BaseResultDto(isSuccess: true, val: Resource.Notification.Success);
            }
            else
            {
                return new BaseResultDto(isSuccess: false, messages: rebate.Messages);
            }
        }

        public async Task<BaseResultDto> ClearRebateCodeAsync(long id)
        {
            var item = await _context.PansionReserves.AsTracking().FirstOrDefaultAsync(s => s.Id == id);
            if (!item.RebateId.HasValue)
            {
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            }
            item.RebateId = null;
            item.PaymentPrice = item.PaymentPrice + item.RebatePrice;
            item.RebatePrice = 0;
            if (item.WalletPrice != 0)
            {
                item.WalletPrice = item.PaymentPrice;
            }
            _context.PansionReserves.Update(item);
            await _context.SaveChangesAsync();
            return new BaseResultDto(isSuccess: true, val: Resource.Notification.Success);
        }

        public async Task<BaseResultDto> SetWalletAsyncDto(PansionReserveWalletDto dto)
        {
            var item = await _context.PansionReserves.Include(s => s.Booker).FirstOrDefaultAsync(s => s.Id == dto.Id);
            if (item == null)
            {
                return new BaseResultDto<PansionReserveWalletDto>(false, Resource.Notification.NothingFound, dto);
            }
            if (item.StatusId == (long)PansionReserveStatusEnum.PansionReserveState_Complete)
            {
                return new BaseResultDto<PansionReserveWalletDto>(false, Resource.Notification.ThisReserveIsCompleted, dto);
            }
            if (item.StatusId == (long)PansionReserveStatusEnum.PansionReserveState_Paid)
            {
                return new BaseResultDto<PansionReserveWalletDto>(false, Resource.Notification.ThisReserveIsPaid, dto);
            }
            if (dto.FromWallet)
            {
                item.FromWallet = true;
                item.WalletPrice = item.PaymentPrice;
            }
            else
            {
                item.FromWallet = false;
                item.WalletPrice = 0;
            }
            _context.PansionReserves.Update(item);
            await _context.SaveChangesAsync();
            return new BaseResultDto(isSuccess: true, val: Resource.Notification.Success);
        }

        public async Task<BaseResultDto<int>> ReserveCountAsync(long id)
        {
            var count = await _context.PansionReserves.CountAsync(s => s.PansionId == id);
            return new BaseResultDto<int>(true, count);
        }
    }
}
