using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Enumerable.Message;
using Application.Common.Helpers;
using Application.Common.Helpers.Iface;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Services.Accounting.ScoreTransactionSrv.Iface;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.CompanionSrvs.CompanionReserveSrv.Dto;
using Application.Services.Order.RebateSrv.Iface;
using Application.Services.PansionSrvs.PansionReserveSrv.Dto;
using Application.Services.PansionSrvs.PansionReserveSrv.Iface;
using Application.Services.PastilClubSrvs.PointEventSrv.Iface;
using Application.Services.ProductSrvs.WalletSrv.Dto;
using Application.Services.ProductSrvs.WalletSrv.IFace;
using Application.Services.Setting.CodeSrv.Iface;
using Application.Services.Setting.MessageSenderSrv.Iface;
using Application.Services.Setting.NoticeSrv;
using Application.Services.Setting.NoticeSrv.Dto;
using Application.Services.Setting.NoticeSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Entities.Entities.PansionField;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Data;
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
        private readonly IScoreTransactionService _scoreService;
        private readonly IClubPointIntegrationService _clubPointIntegrationService;
        private readonly ILogger<PansionReserveService> _logger;
        public PansionReserveService(IDataBaseContext _context, IPushNotificationService pushNotificationService, IMapper mapper, IWalletService walletService,
            IRebateService rebateService, IAdminSettingHelper adminSettingHelper, ICodeService codeService, IMessageSenderService messageSender,
            ICurrentUserHelper currentUser, INoticeService notificationService, IScoreTransactionService scoreService,
            IClubPointIntegrationService clubPointIntegrationService, ILogger<PansionReserveService> logger) : base(_context, mapper)
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
            this._scoreService = scoreService;
            this._clubPointIntegrationService = clubPointIntegrationService;
            this._logger = logger;
        }
        public async Task<BaseResultDto<PansionReserveVDto>> FindAsyncVDto(long id, long? bookerId = null)
        {
            var query = _context.PansionReserves.Include(s => s.Status).Include(s => s.Booker).Include(s => s.UserPet).Include(s => s.Rebate).Where(s => s.Id == id);
            if (bookerId.HasValue)
                query = query.Where(s => s.BookerId == bookerId.Value);
            var item = await query.FirstOrDefaultAsync();
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
                    item.CreateDate = DateTime.Now;
                    if (_currentUser.CurrentUser.RoleEnum == RoleEnum.Admin.ToString())
                    {
                        item.IsReserved = true;
                    }
                    else
                    {
                        item.IsReserved = false;
                    }
                    var pansion = await _context.Pansions.FirstOrDefaultAsync(s =>
                        s.Id == dto.PansionId && s.Active && s.Approve);
                    if (pansion == null || !await _context.UserPets.AnyAsync(s =>
                            s.Id == dto.UserPetId && s.UserId == dto.BookerId))
                    {
                        return new BaseResultDto<PansionReserveDto>(false, Resource.Notification.InvalidData, dto);
                    }
                    var hasSchoolInputs = !string.IsNullOrWhiteSpace(dto.StartTime) && !string.IsNullOrWhiteSpace(dto.EndTime) && dto.SchoolCreateDate.HasValue;
                    var hasPansionInputs = dto.FromDate.HasValue && dto.ToDate.HasValue;

                    if (!ReservationScheduleValidator.IsPansionModeValid(
                            pansion.IsSchool,
                            hasSchoolInputs,
                            hasPansionInputs))
                    {
                        return new BaseResultDto<PansionReserveDto>(
                            false,
                            pansion.IsSchool == true
                                ? "این مرکز مهد ساعتی است؛ تاریخ و بازه ساعت را کامل ارسال کنید."
                                : pansion.IsSchool == false
                                    ? "این مرکز پانسیون روزانه است؛ بازه تاریخ ورود و خروج را کامل ارسال کنید."
                                    : "نوع فعالیت پانسیون مشخص نشده است.",
                            dto);
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
                        if (dto.SchoolCreateDate.Value.Date < DateTime.Today)
                            return new BaseResultDto<PansionReserveDto>(false, Resource.Notification.InvalidData, dto);

                        var schoolStartDateTime = dto.SchoolCreateDate.Value.Date.Add(startTime);
                        if (schoolStartDateTime <= DateTime.Now)
                        {
                            return new BaseResultDto<PansionReserveDto>(
                                false,
                                "امکان رزرو مهد در زمان گذشته وجود ندارد.",
                                dto);
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
                        if (from < DateTime.Today)
                            return new BaseResultDto<PansionReserveDto>(false, Resource.Notification.InvalidData, dto);

                        item.DayCount = (to - from).Days + 1;
                        item.HourCount = 0;

                        item.Price = pansion.PansionPrice * item.DayCount;
                        item.PaymentPrice = item.Price;
                    }

                    var status = await _codeService.GetIdByLabelAsync(PansionReserveStatusEnum.PansionReserveState_Registered.ToString());
                    item.StatusId = status;
                    await using var transaction = await _context.BeginTransactionAsync(IsolationLevel.Serializable);
                    var hasOverlap = hasSchoolInputs
                        ? await _context.PansionReserves.AnyAsync(s =>
                            s.PansionId == item.PansionId &&
                            s.UserPetId == item.UserPetId &&
                            s.IsReserved &&
                            !s.IsCancel &&
                            s.SchoolCreateDate == item.SchoolCreateDate &&
                            string.Compare(s.StartTime, item.EndTime) < 0 &&
                            string.Compare(s.EndTime, item.StartTime) > 0)
                        : await _context.PansionReserves.AnyAsync(s =>
                            s.PansionId == item.PansionId &&
                            s.UserPetId == item.UserPetId &&
                            s.IsReserved &&
                            !s.IsCancel &&
                            s.FromDate <= item.ToDate &&
                            s.ToDate >= item.FromDate);
                    if (hasOverlap)
                    {
                        await transaction.RollbackAsync();
                        return new BaseResultDto<PansionReserveDto>(false, Resource.Notification.HaveBeenReserved, dto);
                    }
                    await _context.PansionReserves.AddAsync(item);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    await SendCreatedNotificationsAsync(item.Id);

                    return new BaseResultDto<PansionReserveDto>(true, mapper.Map<PansionReserveDto>(item));
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Creating pansion reserve failed for pansion {PansionId} and booker {BookerId}.", dto?.PansionId, dto?.BookerId);
                return new BaseResultDto<PansionReserveDto>(isSuccess: false, val: Resource.Notification.Unsuccess, data: dto);
            }
        }

        private async Task SendCreatedNotificationsAsync(long reserveId)
        {
            try
            {
                var reserve = await _context.PansionReserves
                    .AsNoTracking()
                    .Include(s => s.Booker)
                    .Include(s => s.Pansion)
                    .ThenInclude(s => s.Companion)
                    .ThenInclude(s => s.Owner)
                    .FirstOrDefaultAsync(s => s.Id == reserveId);

                if (reserve?.Booker == null || reserve.Pansion?.Companion?.Owner == null)
                {
                    _logger.LogError("Notification data for pansion reserve {ReserveId} is incomplete.", reserveId);
                    return;
                }

                var booker = reserve.Booker;
                var pansion = reserve.Pansion;
                var dateOnly = reserve.SchoolCreateDate?.ToString("yyyy/MM/dd") ??
                               reserve.FromDate?.ToString("yyyy/MM/dd");
                var nameText = $"{booker.FirstName}_{booker.LastName}".Replace(" ", "_");

                await RunPostCommitActionAsync(
                    () => _messageSender.SendMessageAsync(
                        MessageTypeEnum.PansionReserveForUser,
                        booker.Mobile,
                        null,
                        token1: pansion.Name,
                        token2: dateOnly),
                    reserveId,
                    "user SMS");

                await RunPostCommitActionAsync(
                    () => _messageSender.SendMessageAsync(
                        MessageTypeEnum.PansionReserveForPansion,
                        pansion.Companion.Owner.Mobile,
                        null,
                        token1: pansion.Name,
                        token2: booker.LastName,
                        token3: dateOnly),
                    reserveId,
                    "pansion SMS");

                await RunPostCommitActionAsync(
                    () => _messageSender.SendMessageAsync(
                        MessageTypeEnum.PansionReserveForAdmin,
                        _adminSettingHelper.BaseAdminSetting?.AdminMobiles,
                        null,
                        token1: booker.Id.ToString(),
                        token2: pansion.Name),
                    reserveId,
                    "admin SMS");

                await RunPostCommitActionAsync(
                    () => _pushNotificationService.SendPushAsync(
                        PushTypeEnum.PushRegisterPansionUser,
                        booker.Id,
                        token1: booker.FirstName,
                        token2: pansion.Name),
                    reserveId,
                    "user push");

                await RunPostCommitActionAsync(
                    () => _pushNotificationService.SendPushAsync(
                        PushTypeEnum.PushRegisterPansionCompanion,
                        pansion.Companion.Owner.Id,
                        token1: nameText),
                    reserveId,
                    "pansion push");

                await RunPostCommitActionAsync(
                    () => _notificationService.CreateAsync(new NoticeCreateDto
                    {
                        Label = NoticeTypeLabels.PansionReserveRegistered,
                        ActorUserId = booker.Id,
                        ReferenceType = "PansionReserve",
                        ReferenceId = reserve.Id,
                        DeduplicationKey = $"{NoticeTypeLabels.PansionReserveRegistered}:{reserve.Id}",
                        Metadata = new Dictionary<string, string>
                        {
                            { "userName", $"{booker.FirstName} {booker.LastName}".Trim() },
                            { "pansionName", pansion.Name },
                            { "reserveDate", dateOnly ?? string.Empty },
                            { "mobile", booker.Mobile }
                        }
                    }),
                    reserveId,
                    "admin notice");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Preparing notifications for pansion reserve {ReserveId} failed.", reserveId);
            }
        }

        private async Task RunPostCommitActionAsync(Func<Task> action, long reserveId, string actionName)
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Post-commit action {ActionName} failed for pansion reserve {ReserveId}.", actionName, reserveId);
            }
        }

        public async Task<BaseResultDto> PansionReservePaymentCallback(long? reserveId, bool fromWallet = false)
        {
            try
            {
                await using var transaction = await _context.BeginTransactionAsync(IsolationLevel.Serializable);
                var reserve = await _context.PansionReserves.Include(s => s.Booker).Include(s => s.Pansion).Include(s => s.Rebate).AsTracking().FirstOrDefaultAsync(s => s.Id == reserveId);
                if (reserve == null)
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                if (reserve.IsReserved)
                    return new BaseResultDto(true);

                var hasOverlap = reserve.Pansion?.IsSchool == true
                    ? await _context.PansionReserves.AsNoTracking().AnyAsync(s =>
                        s.Id != reserve.Id &&
                        s.PansionId == reserve.PansionId &&
                        s.UserPetId == reserve.UserPetId &&
                        s.IsReserved &&
                        !s.IsCancel &&
                        s.SchoolCreateDate == reserve.SchoolCreateDate &&
                        string.Compare(s.StartTime, reserve.EndTime) < 0 &&
                        string.Compare(s.EndTime, reserve.StartTime) > 0)
                    : await _context.PansionReserves.AsNoTracking().AnyAsync(s =>
                        s.Id != reserve.Id &&
                        s.PansionId == reserve.PansionId &&
                        s.UserPetId == reserve.UserPetId &&
                        s.IsReserved &&
                        !s.IsCancel &&
                        s.FromDate <= reserve.ToDate &&
                        s.ToDate >= reserve.FromDate);

                if (hasOverlap)
                {
                    return new BaseResultDto(false, Resource.Notification.HaveBeenReserved);
                }

                if (fromWallet && reserve.FromWallet && reserve.WalletPrice > 0)
                {
                    var walletItem = new WalletDto()
                    {
                        Painding = false,
                        Amount = reserve.WalletPrice,
                        UserId = reserve.Booker.Id,
                        PansionReserveId = reserve.Id
                    };
                    var walletResult = await _walletService.InsertUpdatePansionReserveAsync(walletItem, true);
                    if (!walletResult.IsSuccess)
                        return new BaseResultDto(false);
                }
                if (reserve.Rebate != null)
                {
                    _rebateService.IncreaseUseCount(reserve);
                }
                reserve.IsReserved = true;

                var paidStatus = await _codeService.GetIdByLabelAsync(PansionReserveStatusEnum.PansionReserveState_Paid.ToString());
                reserve.StatusId = paidStatus;

                await UpdatePansionReserveCommissionDto(reserve);

                double scoreRatio = 10000;
                double earnedScore = Math.Floor(reserve.PaymentPrice / scoreRatio);

                if (earnedScore > 0)
                {
                    await _scoreService.AddScoreAsync(
                        userId: reserve.BookerId,
                        amount: earnedScore,
                        type: ScoreTransactionType.ScoreTransactionType_PansionReserve,
                        referenceId: reserve.Id.ToString()
                    );
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new BaseResultDto(true, Resource.Notification.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Pansion reserve payment callback failed for reserve {ReserveId}.", reserveId);
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

            if (dto.IsCancel)
                await _clubPointIntegrationService.PansionReserveReversedAsync(model.BookerId, model.Id);

            var adminMobile = _adminSettingHelper.BaseAdminSetting.AdminMobiles;
            var booker = _context.Users.FirstOrDefault(u => u.Id == model.BookerId);

            var Pansion = _context.Pansions.Include(s => s.Companion).ThenInclude(s => s.Owner).FirstOrDefault(a => a.Id == model.PansionId);
            await _messageSender.SendMessageAsync(messageType: MessageTypeEnum.PansionReserveCancelForAdmin, mobileReceptor: adminMobile, emailReceptor: null, token1: booker.Id.ToString(), token2: Pansion.Name);
            return new BaseResultDto<PansionReserveCancelDto>(true, mapper.Map<PansionReserveCancelDto>(model));
        }

        public async Task<BaseResultDto> UpdatePansionReserveStatusDto(PansionReserveStatusDto dto)
        {
            var item = await _context.PansionReserves.FirstOrDefaultAsync(s => s.Id == dto.Id);

            if (item == null)
                return new BaseResultDto<PansionReserveStatusDto>(false, Resource.Notification.NothingFound, dto);

            if (item.PaymentPrice == 0)
            {
                return new BaseResultDto<PansionReserveStatusDto>(false, Resource.Notification.TheFinalPriceHasNotYetBeenRecordedForThisReserve, dto);
            }
            item.StatusId = dto.StatusId;

            _context.PansionReserves.Update(item);
            await _context.SaveChangesAsync();

            if (dto.StatusId == (long)PansionReserveStatusEnum.PansionReserveState_Complete &&
                item.IsReserved &&
                !item.IsCancel)
            {
                await _clubPointIntegrationService.PansionReserveCompletedAsync(item.BookerId, item.Id);
            }

            return new BaseResultDto<PansionReserveStatusDto>(true, mapper.Map<PansionReserveStatusDto>(item));
        }



        public async Task<BaseResultDto> SetRebateCodeAsyncDto(PansionReserveRebateCodeDto dto)
        {
            var item = await _context.PansionReserves.AsTracking().FirstOrDefaultAsync(s =>
                s.Id == dto.Id &&
                s.BookerId == _currentUser.CurrentUser.UserId &&
                s.StatusId == (long)PansionReserveStatusEnum.PansionReserveState_Registered);

            if (item == null)
            {
                return new BaseResultDto<PansionReserveRebateCodeDto>(false, Resource.Notification.NothingFound, dto);
            }
            if (string.IsNullOrEmpty(dto.RebateCode))
            {
                return new BaseResultDto(isSuccess: false, val: Resource.Notification.Unsuccess);
            }
            if (await HasActivePaymentAsync(item.Id))
                return new BaseResultDto(false, "پرداخت این رزرو شروع شده و اطلاعات مالی آن قابل تغییر نیست.");
            var originalPrice = item.PaymentPrice + item.RebatePrice;
            item.Price = originalPrice;
            var rebate = _rebateService.GetRebateByCodeAsync(item, dto.RebateCode);
            if (rebate.IsSuccess)
            {
                item.Rebate = null;
                item.RebateId = rebate.Data.Id;
                item.RebatePrice = rebate.Data.FinalPrice;
                item.PaymentPrice = originalPrice - item.RebatePrice;
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
            var item = await _context.PansionReserves.AsTracking().FirstOrDefaultAsync(s =>
                s.Id == id && s.BookerId == _currentUser.CurrentUser.UserId && !s.IsReserved);
            if (item == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (await HasActivePaymentAsync(item.Id))
                return new BaseResultDto(false, "پرداخت این رزرو شروع شده و اطلاعات مالی آن قابل تغییر نیست.");
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
            var item = await _context.PansionReserves.Include(s => s.Booker).FirstOrDefaultAsync(s =>
                s.Id == dto.Id && s.BookerId == _currentUser.CurrentUser.UserId);
            if (item == null)
            {
                return new BaseResultDto<PansionReserveWalletDto>(false, Resource.Notification.NothingFound, dto);
            }
            if (await HasActivePaymentAsync(item.Id))
                return new BaseResultDto(false, "پرداخت این رزرو شروع شده و اطلاعات مالی آن قابل تغییر نیست.");
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

        private Task<bool> HasActivePaymentAsync(long reserveId)
        {
            var callbackId = reserveId.ToString();
            return _context.Payments.AsNoTracking().AnyAsync(s =>
                s.CallBackTypeLabel == PaymentCallbackTypeEnum.PansionReserve.ToString() &&
                s.CallBackId == callbackId &&
                (s.IsSuccess == null || s.IsSuccess == true));
        }

        public async Task<BaseResultDto<int>> ReserveCountAsync(long id)
        {
            var count = await _context.PansionReserves.CountAsync(s => s.PansionId == id);
            return new BaseResultDto<int>(true, count);
        }

        public async Task<BaseResultDto> UpdatePermittedAsyncDto(long id)
        {
            var item = await _context.PansionReserves.AsTracking().FirstOrDefaultAsync(s => s.Id == id);
            item.Permitted = true;
            _context.PansionReserves.Update(item);
            await _context.SaveChangesAsync();
            return new BaseResultDto(isSuccess: true, val: Resource.Notification.Success);
        }
    }
}
