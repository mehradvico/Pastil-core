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
using Application.Services.CompanionSrv.CompanionAssistanceSrv.Dto;
using Application.Services.CompanionSrv.CompanionReserveSrv.Dto;
using Application.Services.CompanionSrv.CompanionReserveSrv.Iface;
using Application.Services.CompanionSrvs.CompanionReservePackageSrv.Iface;
using Application.Services.CompanionSrvs.CompanionReserveSrv.Dto;
using Application.Services.CompanionSrvs.CompanionReserveUserPetSrv.Iface;
using Application.Services.Order.RebateSrv.Iface;
using Application.Services.Order.PaymentGatewaySrv.Iface;
using Application.Services.PastilClubSrvs.PointEventSrv.Iface;
using Application.Services.ProductSrvs.WalletSrv.Dto;
using Application.Services.ProductSrvs.WalletSrv.IFace;
using Application.Services.Setting.CodeSrv.Iface;
using Application.Services.Setting.MessageSenderSrv.Iface;
using Application.Services.Setting.NoticeSrv;
using Application.Services.Setting.NoticeSrv.Dto;
using Application.Services.Setting.NoticeSrv.Iface;
using Application.Services.TripSrv.TripSrv.Dto;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrv.CompanionReserveSrv
{
    public class CompanionReserveService : CommonSrv<CompanionReserve, CompanionReserveDto>, ICompanionReserveService
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
        private readonly ICompanionReserveUserPetService _companionReserveUserPetService;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly ICompanionReservePackageService _companionReservePackageService;
        private readonly IScoreTransactionService _scoreService;
        private readonly IClubPointIntegrationService _clubPointIntegrationService;
        private readonly ILogger<CompanionReserveService> _logger;
        private readonly IPaymentTestModeService _paymentTestModeService;
        public CompanionReserveService(IDataBaseContext _context, IPushNotificationService pushNotificationService, IMapper mapper,
            ICompanionReservePackageService companionReservePackageService, ICompanionReserveUserPetService companionReserveUserPetService,
            IWalletService walletService, IRebateService rebateService, IAdminSettingHelper adminSettingHelper, ICodeService codeService,
            IMessageSenderService messageSender, ICurrentUserHelper currentUser, INoticeService notificationService, IScoreTransactionService scoreService,
            IClubPointIntegrationService clubPointIntegrationService, ILogger<CompanionReserveService> logger,
            IPaymentTestModeService paymentTestModeService) : base(_context, mapper)
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
            this._companionReserveUserPetService = companionReserveUserPetService;
            this._companionReservePackageService = companionReservePackageService;
            this._pushNotificationService = pushNotificationService;
            this._scoreService = scoreService;
            this._clubPointIntegrationService = clubPointIntegrationService;
            this._logger = logger;
            this._paymentTestModeService = paymentTestModeService;
        }

        public async Task<BaseResultDto<CompanionReserveAdminVDto>> FindAsyncAdminVDto(long id)
        {
            var item = await _context.CompanionReserves.Include(s => s.State).Include(s => s.CompanionAssistanceUser).ThenInclude(s => s.CompanionAssistance).ThenInclude(s => s.Companion)
                .Include(s => s.CompanionAssistanceUser).ThenInclude(s => s.User).Include(s => s.Booker).Include(s => s.UserPets)
                .Include(s => s.CompanionAssistance).ThenInclude(s => s.Assistance).Include(s => s.CompanionAssistance).ThenInclude(s => s.Companion)
                .Include(s => s.CompanionAssistancePackages).ThenInclude(s => s.Picture).Include(s => s.CompanionAssistanceTime).ThenInclude(s => s.WeekDay).Include(s => s.CompanionAssistanceType)
                .Include(s => s.OperatorState).Include(s => s.Rebate).Include(s => s.CompanionAssistance).ThenInclude(s => s.Assistance).ThenInclude(s => s.Picture).FirstOrDefaultAsync(s => s.Id == id);
            if (item != null)
            {
                return new BaseResultDto<CompanionReserveAdminVDto>(true, mapper.Map<CompanionReserveAdminVDto>(item));
            }
            return new BaseResultDto<CompanionReserveAdminVDto>(false, mapper.Map<CompanionReserveAdminVDto>(item));
        }

        public async Task<BaseResultDto<CompanionReserveVDto>> FindAsyncVDto(long id, long? bookerId = null)
        {
            var query = _context.CompanionReserves.Include(s => s.State).Include(s => s.CompanionAssistanceUser).ThenInclude(s => s.CompanionAssistance).ThenInclude(s => s.Companion)
                .Include(s => s.CompanionAssistanceUser).ThenInclude(s => s.User).Include(s => s.Booker).Include(s => s.UserPets)
                .Include(s => s.CompanionAssistance).ThenInclude(s => s.Assistance).Include(s => s.CompanionAssistance).ThenInclude(s => s.Companion)
                .Include(s => s.CompanionAssistancePackages).ThenInclude(s => s.Picture).Include(s => s.CompanionAssistanceTime).ThenInclude(s => s.WeekDay).Include(s => s.CompanionAssistanceType)
                .Include(s => s.OperatorState).Include(s => s.Rebate).Include(s => s.CompanionAssistance).ThenInclude(s => s.Assistance).ThenInclude(s => s.Picture).Where(s => s.Id == id);
            if (bookerId.HasValue)
                query = query.Where(s => s.BookerId == bookerId.Value);
            var item = await query.FirstOrDefaultAsync();
            if (item != null)
            {
                return new BaseResultDto<CompanionReserveVDto>(true, mapper.Map<CompanionReserveVDto>(item));
            }
            return new BaseResultDto<CompanionReserveVDto>(false, mapper.Map<CompanionReserveVDto>(item));
        }

        public CompanionReserveSearchDto Search(CompanionReserveInputDto baseSearchDto)
        {
            var model = _context.CompanionReserves.Include(s => s.State)
                .Include(s => s.CompanionAssistanceUser).ThenInclude(s => s.CompanionAssistance).ThenInclude(s => s.Companion)
                .Include(s => s.CompanionAssistanceUser).ThenInclude(s => s.User).Include(s => s.Booker).Include(s => s.UserPets)
                .Include(s => s.CompanionAssistance).ThenInclude(s => s.Assistance).Include(s => s.CompanionAssistance).ThenInclude(s => s.Companion)
                .Include(s => s.CompanionAssistancePackages).Include(s => s.CompanionAssistanceTime).ThenInclude(s => s.WeekDay)
                .Include(s => s.CompanionAssistanceType).Include(s => s.OperatorState).AsQueryable();

            if (baseSearchDto.BookerId.HasValue)
            {
                model = model.Where(s => s.BookerId == baseSearchDto.BookerId.Value);
            }
            if (baseSearchDto.CompanionId.HasValue)
            {
                model = model.Where(s => s.CompanionAssistance.CompanionId == baseSearchDto.CompanionId.Value);
            }
            if (baseSearchDto.UserPetId.HasValue)
            {
                model = model.Where(s => s.UserPets.Any(s => s.Id == baseSearchDto.UserPetId.Value));
            }
            if (baseSearchDto.CompanionAssistanceId.HasValue)
            {
                model = model.Where(s => s.CompanionAssistanceId == baseSearchDto.CompanionAssistanceId.Value);
            }
            if (baseSearchDto.IsFemale.HasValue)
            {
                model = model.Where(s => s.IsFemale == baseSearchDto.IsFemale.Value);
            }
            if (baseSearchDto.CompanionAssistanceTimeId.HasValue)
            {
                model = model.Where(s => s.CompanionAssistanceTimeId == baseSearchDto.CompanionAssistanceTimeId.Value);
            }
            if (baseSearchDto.CompanionAssistanceUserId.HasValue)
            {
                var operatorUserId = baseSearchDto.CompanionAssistanceUserId.Value;
                model = model.Where(s =>
                    s.CompanionAssistanceUser.UserId == operatorUserId &&
                    s.IsReserved &&
                    _context.CompanionUsers.Any(companionUser =>
                        companionUser.CompanionId == s.CompanionAssistance.CompanionId &&
                        companionUser.UserId == operatorUserId &&
                        !companionUser.Deleted &&
                        companionUser.Active &&
                        companionUser.UserAccept == true));
            }

            if (baseSearchDto.ReserveState.HasValue)
            {
                if (baseSearchDto.ReserveState.Value == ReserveStateEnum.CompanionReserveState_CurrentDays)
                {
                    model = model.Where(s => s.IsReserved && !s.IsCancel && s.DoneDate == null && s.DoDate >= DateTime.Now);
                }
                if (baseSearchDto.ReserveState.Value == ReserveStateEnum.CompanionReserveState_Done)
                {
                    model = model.Where(s => s.IsReserved && !s.IsCancel && s.DoneDate.HasValue);
                }
                if (baseSearchDto.ReserveState.Value == ReserveStateEnum.CompanionReserveState_Expired)
                {
                    model = model.Where(s => s.IsReserved && !s.IsCancel && s.DoneDate == null && s.DoDate <= DateTime.Now);
                }
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
            return new CompanionReserveSearchDto(baseSearchDto, model, mapper);
        }

        public override async Task<BaseResultDto<CompanionReserveDto>> InsertAsyncDto(CompanionReserveDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<CompanionReserveDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    dto.UserPetIds = dto.UserPetIds?.Distinct().ToList() ?? new List<long>();
                    if (!dto.UserPetIds.Any())
                        return new BaseResultDto<CompanionReserveDto>(false, Resource.Notification.SelectAtLeastOneType, dto);

                    var ownedPetCount = await _context.UserPets.CountAsync(s =>
                        dto.UserPetIds.Contains(s.Id) && s.UserId == dto.BookerId);
                    if (ownedPetCount != dto.UserPetIds.Count)
                        return new BaseResultDto<CompanionReserveDto>(false, Resource.Notification.InvalidData, dto);

                    var item = mapper.Map<CompanionReserve>(dto);
                    item.IsCancel = false;
                    bool existed = await _context.CompanionReserves.AnyAsync(s => s.CompanionAssistanceId == dto.CompanionAssistanceId && s.BookerId == dto.BookerId && s.CompanionAssistanceTimeId == dto.CompanionAssistanceTimeId && s.IsReserved && !s.IsCancel);
                    if (existed)
                    {
                        return new BaseResultDto<CompanionReserveDto>(false, Resource.Notification.HaveBeenReserved, dto);
                    }
                    item.DoneDate = null;
                    item.OperatorChangeStateDate = null;
                    item.CreateDate = DateTime.Now;
                    item.OperatorStateId = (long)CompanionReserveOperatorStateEnum.OperatorState_InComplete;
                    item.UserResponse = null;
                    if (_currentUser.CurrentUser.RoleEnum == RoleEnum.Admin.ToString())
                    {
                        item.IsReserved = true;
                    }
                    else
                    {
                        item.IsReserved = false;
                    }
                    if (dto.CompanionAssistanceTypeId == (long)CompanionAssistanceTypeEnum.CompanionAssistanceType_InPlace && (dto.AddressId == null || dto.AddressId == 0))
                    {
                        return new BaseResultDto<CompanionReserveDto>(false, Resource.Notification.PleaseEnterTheAddress, dto);
                    }
                    var companionAssistance = await _context.CompanionAssistances
                        .Include(s => s.Companion)
                        .ThenInclude(s => s.CompanionZones)
                        .Include(s => s.Codes)
                        .FirstOrDefaultAsync(s =>
                            s.Id == dto.CompanionAssistanceId &&
                            s.Active &&
                            s.Approved &&
                            !s.Deleted &&
                            s.Companion.Active);
                    if (companionAssistance == null)
                        return new BaseResultDto<CompanionReserveDto>(false, Resource.Notification.NothingFound, dto);

                    if (!companionAssistance.Codes.Any(s => s.Id == dto.CompanionAssistanceTypeId))
                    {
                        return new BaseResultDto<CompanionReserveDto>(
                            false,
                            "نوع ارائه انتخاب‌شده متعلق به این خدمت نیست.",
                            dto);
                    }

                    if (ReservationScheduleValidator.IsDateInPast(dto.DoDate, DateTime.Now))
                    {
                        return new BaseResultDto<CompanionReserveDto>(
                            false,
                            "امکان رزرو خدمت در تاریخ گذشته وجود ندارد.",
                            dto);
                    }

                    CompanionAssistanceTime selectedAssistanceTime = null;
                    if (dto.CompanionAssistanceTimeId.HasValue)
                    {
                        selectedAssistanceTime = await _context.CompanionAssistanceTimes
                            .AsNoTracking()
                            .Include(s => s.WeekDay)
                            .FirstOrDefaultAsync(s =>
                                s.Id == dto.CompanionAssistanceTimeId.Value &&
                                s.CompanionAssistanceId == dto.CompanionAssistanceId &&
                                s.Active &&
                                !s.Deleted);

                        if (selectedAssistanceTime == null)
                        {
                            return new BaseResultDto<CompanionReserveDto>(
                                false,
                                "زمان انتخاب‌شده متعلق به این خدمت نیست یا فعال نیست.",
                                dto);
                        }

                        if (!ReservationScheduleValidator.IsWeekDayMatch(dto.DoDate, selectedAssistanceTime.WeekDay?.Label))
                        {
                            return new BaseResultDto<CompanionReserveDto>(
                                false,
                                "روز تاریخ رزرو با روز زمان انتخاب‌شده هماهنگ نیست.",
                                dto);
                        }

                        if (!ReservationScheduleValidator.TryGetServiceStartDateTime(
                                dto.DoDate,
                                selectedAssistanceTime.StartTime,
                                out var serviceStartDateTime))
                        {
                            return new BaseResultDto<CompanionReserveDto>(
                                false,
                                "ساعت شروع خدمت معتبر نیست.",
                                dto);
                        }

                        if (serviceStartDateTime <= DateTime.Now)
                        {
                            return new BaseResultDto<CompanionReserveDto>(
                                false,
                                "امکان رزرو خدمت در زمان گذشته وجود ندارد.",
                                dto);
                        }
                    }
                    else if (await _context.CompanionAssistanceTimes.AnyAsync(s =>
                                 s.CompanionAssistanceId == dto.CompanionAssistanceId &&
                                 s.Active &&
                                 !s.Deleted))
                    {
                        return new BaseResultDto<CompanionReserveDto>(
                            false,
                            "انتخاب زمان خدمت الزامی است.",
                            dto);
                    }

                    if (dto.CompanionAssistanceUserId.HasValue)
                    {
                        var validAssistanceUser = await _context.CompanionAssistanceUsers
                            .AsNoTracking()
                            .FirstOrDefaultAsync(s =>
                                s.Id == dto.CompanionAssistanceUserId.Value &&
                                s.CompanionAssistanceId == dto.CompanionAssistanceId &&
                                s.Active &&
                                !s.Deleted);

                        if (validAssistanceUser == null)
                        {
                            return new BaseResultDto<CompanionReserveDto>(
                                false,
                                "اپراتور انتخاب‌شده متعلق به این خدمت نیست یا فعال نیست.",
                                dto);
                        }

                        var validMembership = await _context.CompanionUsers
                            .AsNoTracking()
                            .AnyAsync(s =>
                                s.CompanionId == companionAssistance.CompanionId &&
                                s.UserId == validAssistanceUser.UserId &&
                                !s.Deleted &&
                                s.Active &&
                                s.UserAccept == true);

                        if (!validMembership)
                        {
                            return new BaseResultDto<CompanionReserveDto>(
                                false,
                                "عضویت اپراتور انتخاب‌شده در این نمایندگی فعال و تأییدشده نیست.",
                                dto);
                        }

                        if (selectedAssistanceTime != null &&
                            await HasAssigneeScheduleConflictAsync(
                                reserveId: 0,
                                dto.DoDate,
                                selectedAssistanceTime,
                                validAssistanceUser.Id))
                        {
                            return new BaseResultDto<CompanionReserveDto>(
                                false,
                                "اپراتور انتخاب‌شده در این بازه زمانی رزرو فعال دیگری دارد.",
                                dto);
                        }
                    }

                    if (dto.AddressId.HasValue)
                    {
                        var address = await _context.Addresses.FirstOrDefaultAsync(s =>
                            s.Id == dto.AddressId && s.UserId == dto.BookerId);
                        if (address == null)
                            return new BaseResultDto<CompanionReserveDto>(false, Resource.Notification.InvalidData, dto);
                        if (!companionAssistance.Companion.CompanionZones.Any(s => s.CityId == address.CityId))
                        {
                            return new BaseResultDto<CompanionReserveDto>(false, Resource.Notification.ThisCompanionHasNoActivityInYourZone, dto);
                        }

                    }

                    if (dto.CompanionAssistancePackagesIds == null)
                    {
                        dto.CompanionAssistancePackagesIds = new List<long>();
                    }
                    dto.CompanionAssistancePackagesIds = dto.CompanionAssistancePackagesIds.Distinct().ToList();
                    if (!dto.CompanionAssistancePackagesIds.Any())
                    {
                        return new BaseResultDto<CompanionReserveDto>(false, Resource.Notification.SelectAtLeastOneType, dto);
                    }
                    if (companionAssistance.IsSinglePackage && dto.CompanionAssistancePackagesIds.Count != 1)
                    {
                        return new BaseResultDto<CompanionReserveDto>(false, Resource.Notification.InvalidData, dto);
                    }
                    var packages = await _context.CompanionAssistancePackages.Where(p => dto.CompanionAssistancePackagesIds.Contains(p.Id) && p.CompanionAssistanceId == dto.CompanionAssistanceId && p.Active && !p.Deleted).ToListAsync();
                    if (packages.Count != dto.CompanionAssistancePackagesIds.Count)
                    {
                        return new BaseResultDto<CompanionReserveDto>(false, Resource.Notification.InvalidData, dto);
                    }

                    var unPaidStatus = await _codeService.GetIdByLabelAsync(CompanionReserveStateEnum.CompanianReserveState_Registered.ToString());
                    item.StateId = unPaidStatus;

                    await using var transaction = await _context.BeginTransactionAsync(IsolationLevel.Serializable);
                    await _context.CompanionReserves.AddAsync(item);
                    await _context.SaveChangesAsync();
                    await _companionReserveUserPetService.InsertOrUpdateAsync(item, dto.UserPetIds);

                    if (dto.CompanionAssistancePackagesIds == null)
                    {
                        dto.CompanionAssistancePackagesIds = new List<long>();
                    }
                    if (!dto.CompanionAssistancePackagesIds.Any())
                    {
                        return new BaseResultDto<CompanionReserveDto>(false, Resource.Notification.SelectAtLeastOneType, dto);
                    }
                    dto.CompanionAssistancePackagesIds = dto.CompanionAssistancePackagesIds.Distinct().ToList();
                    await _companionReservePackageService.InsertOrUpdateAsync(item, dto.CompanionAssistancePackagesIds);

                    var petCount = dto.UserPetIds.Count;

                    item.PackagePrice = packages.Sum(p => p.Price) * petCount;
                    item.PrePaymentPrice = packages.Sum(p => p.PrePaymentPrice) * petCount;
                    item.PaymentPrice = item.PrePaymentPrice;
                    _context.CompanionReserves.Update(item);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    await SendCreatedNotificationsAsync(item.Id);
                    return new BaseResultDto<CompanionReserveDto>(true, mapper.Map<CompanionReserveDto>(item));
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Creating companion reserve failed for assistance {CompanionAssistanceId} and booker {BookerId}.", dto?.CompanionAssistanceId, dto?.BookerId);
                return new BaseResultDto<CompanionReserveDto>(isSuccess: false, val: Resource.Notification.Unsuccess, data: dto);
            }
        }

        private async Task SendCreatedNotificationsAsync(long reserveId)
        {
            try
            {
                var reserve = await _context.CompanionReserves
                    .AsNoTracking()
                    .Include(s => s.Booker)
                    .Include(s => s.CompanionAssistance)
                    .ThenInclude(s => s.Assistance)
                    .Include(s => s.CompanionAssistance)
                    .ThenInclude(s => s.Companion)
                    .ThenInclude(s => s.Owner)
                    .Include(s => s.CompanionAssistanceUser)
                    .ThenInclude(s => s.User)
                    .FirstOrDefaultAsync(s => s.Id == reserveId);

                if (reserve?.Booker == null ||
                    reserve.CompanionAssistance?.Assistance == null ||
                    reserve.CompanionAssistance.Companion?.Owner == null)
                {
                    _logger.LogError("Notification data for companion reserve {ReserveId} is incomplete.", reserveId);
                    return;
                }

                var booker = reserve.Booker;
                var assistance = reserve.CompanionAssistance.Assistance;
                var companion = reserve.CompanionAssistance.Companion;
                var nameText = $"{booker.FirstName}_{booker.LastName}".Replace(" ", "_");

                await RunPostCommitActionAsync(
                    () => _messageSender.SendMessageAsync(
                        MessageTypeEnum.CompanionReserveForUser,
                        booker.Mobile,
                        null,
                        token1: assistance.Name),
                    reserveId,
                    "user SMS");

                await RunPostCommitActionAsync(
                    () => _messageSender.SendMessageAsync(
                        MessageTypeEnum.CompanionReserveForCompanion,
                        companion.Owner.Mobile,
                        null,
                        token1: assistance.Name,
                        token2: booker.FirstName),
                    reserveId,
                    "companion SMS");

                await RunPostCommitActionAsync(
                    () => _messageSender.SendMessageAsync(
                        MessageTypeEnum.CompanionReserveForAdmin,
                        _adminSettingHelper.BaseAdminSetting?.AdminMobiles,
                        null,
                        token1: assistance.Name,
                        token2: companion.Name),
                    reserveId,
                    "admin SMS");

                await RunPostCommitActionAsync(
                    () => _pushNotificationService.SendPushAsync(
                        PushTypeEnum.PushRegisterReserveUser,
                        booker.Id,
                        token1: companion.Name,
                        token2: booker.FirstName),
                    reserveId,
                    "user push");

                await RunPostCommitActionAsync(
                    () => _pushNotificationService.SendPushAsync(
                        PushTypeEnum.PushRegisterReserveCompanion,
                        companion.Owner.Id,
                        token1: assistance.Name,
                        token2: nameText),
                    reserveId,
                    "companion push");

                if (reserve.CompanionAssistanceUser?.User != null)
                {
                    await RunPostCommitActionAsync(
                        () => _messageSender.SendMessageAsync(
                            MessageTypeEnum.CompanionReserveForCompanionUser,
                            reserve.CompanionAssistanceUser.User.Mobile,
                            null,
                            token1: assistance.Name,
                            token2: companion.Name),
                        reserveId,
                        "operator SMS");

                }

                await RunPostCommitActionAsync(
                    () => _notificationService.CreateAsync(new NoticeCreateDto
                    {
                        Label = NoticeTypeLabels.CompanionReserveRegistered,
                        ActorUserId = booker.Id,
                        ReferenceType = "CompanionReserve",
                        ReferenceId = reserve.Id,
                        DeduplicationKey = $"{NoticeTypeLabels.CompanionReserveRegistered}:{reserve.Id}",
                        Metadata = new Dictionary<string, string>
                        {
                            { "userName", $"{booker.FirstName} {booker.LastName}".Trim() },
                            { "companionName", companion.Name },
                            { "serviceName", assistance.Name },
                            { "mobile", booker.Mobile }
                        }
                    }),
                    reserveId,
                    "admin notice");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Preparing notifications for companion reserve {ReserveId} failed.", reserveId);
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
                _logger.LogError(ex, "Post-commit action {ActionName} failed for companion reserve {ReserveId}.", actionName, reserveId);
            }
        }

        public async Task<BaseResultDto<List<CompanionReserveAssigneeVDto>>> GetCompanionReserveAssigneesAsync(
            long reserveId,
            bool adminAccess = false)
        {
            var reserve = await _context.CompanionReserves
                .AsNoTracking()
                .Include(s => s.CompanionAssistance)
                    .ThenInclude(s => s.Companion)
                .FirstOrDefaultAsync(s => s.Id == reserveId);

            if (reserve == null)
            {
                return new BaseResultDto<List<CompanionReserveAssigneeVDto>>(
                    false,
                    Resource.Notification.NothingFound,
                    null);
            }

            if (!adminAccess && reserve.CompanionAssistance.Companion.OwnerId != _currentUser.CurrentUser.UserId)
            {
                return new BaseResultDto<List<CompanionReserveAssigneeVDto>>(
                    false,
                    Resource.Notification.AccessDenied,
                    null);
            }

            var assistanceUsers = await _context.CompanionAssistanceUsers
                .AsNoTracking()
                .Include(s => s.User)
                .Where(s =>
                    s.CompanionAssistanceId == reserve.CompanionAssistanceId &&
                    s.Active &&
                    !s.Deleted &&
                    !s.User.Deleted)
                .OrderBy(s => s.User.FirstName)
                .ThenBy(s => s.User.LastName)
                .ToListAsync();

            var userIds = assistanceUsers.Select(s => s.UserId).Distinct().ToList();
            var companionUsers = await _context.CompanionUsers
                .AsNoTracking()
                .Include(s => s.Expertise)
                .Where(s =>
                    s.CompanionId == reserve.CompanionAssistance.CompanionId &&
                    userIds.Contains(s.UserId) &&
                    !s.Deleted &&
                    s.Active &&
                    s.UserAccept == true)
                .ToDictionaryAsync(s => s.UserId);

            var result = assistanceUsers
                .Where(s => companionUsers.ContainsKey(s.UserId))
                .Select(s =>
                {
                    var companionUser = companionUsers[s.UserId];
                    return new CompanionReserveAssigneeVDto
                    {
                        CompanionAssistanceUserId = s.Id,
                        UserId = s.UserId,
                        FullName = $"{s.User.FirstName} {s.User.LastName}".Trim(),
                        PictureId = s.User.PictureId,
                        IsFemale = s.User.IsFemale,
                        ExpertiseId = companionUser.ExpertiseId,
                        ExpertiseName = companionUser.Expertise?.Name,
                        IsAssigned = reserve.CompanionAssistanceUserId == s.Id
                    };
                })
                .ToList();

            return new BaseResultDto<List<CompanionReserveAssigneeVDto>>(true, result);
        }

        public async Task<BaseResultDto<CompanionReserveAdminVDto>> AssignCompanionReserveAsync(
            CompanionReserveAssignDto dto,
            bool adminAccess = false)
        {
            var modelChecker = ModelHelper<CompanionReserveAssignDto>.ModelErrors(dto);
            if (!modelChecker.IsSuccess)
            {
                return new BaseResultDto<CompanionReserveAdminVDto>(
                    false,
                    modelChecker.Messages,
                    null);
            }

            await using var transaction = await _context.BeginTransactionAsync(IsolationLevel.Serializable);

            var reserve = await _context.CompanionReserves
                .AsTracking()
                .Include(s => s.Booker)
                .Include(s => s.CompanionAssistanceTime)
                .Include(s => s.CompanionAssistance)
                    .ThenInclude(s => s.Assistance)
                .Include(s => s.CompanionAssistance)
                    .ThenInclude(s => s.Companion)
                .FirstOrDefaultAsync(s => s.Id == dto.Id);

            if (reserve == null)
            {
                return new BaseResultDto<CompanionReserveAdminVDto>(
                    false,
                    Resource.Notification.NothingFound,
                    null);
            }

            if (!adminAccess && reserve.CompanionAssistance.Companion.OwnerId != _currentUser.CurrentUser.UserId)
            {
                return new BaseResultDto<CompanionReserveAdminVDto>(
                    false,
                    Resource.Notification.AccessDenied,
                    null);
            }

            if (reserve.IsCancel)
            {
                return new BaseResultDto<CompanionReserveAdminVDto>(
                    false,
                    "فقط رزرو پرداخت‌شده و فعال قابل تخصیص است.",
                    null);
            }

            if (!reserve.IsReserved)
            {
                var reserveId = reserve.Id.ToString();
                var allowPendingTestPayment = _paymentTestModeService.IsEnabled;
                var successfulPayment = await _context.Payments
                    .AsTracking()
                    .Where(s =>
                        (s.IsSuccess == true ||
                         allowPendingTestPayment && s.IsSuccess == null) &&
                        s.UserId == reserve.BookerId &&
                        (s.CompanionReserveId == reserve.Id ||
                         s.CallBackTypeLabel == PaymentCallbackTypeEnum.CompanionReserve.ToString() &&
                         s.CallBackId == reserveId))
                    .OrderByDescending(s => s.AppliedDate.HasValue)
                    .ThenByDescending(s => s.Id)
                    .FirstOrDefaultAsync();

                if (successfulPayment == null)
                {
                    return new BaseResultDto<CompanionReserveAdminVDto>(
                        false,
                        "برای این رزرو پرداخت موفقی ثبت نشده است.",
                        null);
                }

                if (successfulPayment.IsSuccess == null)
                {
                    successfulPayment.IsSuccess = true;
                    successfulPayment.RefNumber = $"TEST-{successfulPayment.Id}";
                    successfulPayment.GatewayStatus = "TEST_SUCCESS";
                    successfulPayment.Description = "TEST_MODE_SUCCESS_RECONCILED";
                }

                if (!IsSuccessfulPaymentSnapshotValid(successfulPayment, reserve))
                {
                    return new BaseResultDto<CompanionReserveAdminVDto>(
                        false,
                        "اطلاعات مبلغ پرداخت موفق با مبلغ رزرو مطابقت ندارد و امکان تخصیص خودکار نیست.",
                        null);
                }

                var reconcileResult = await CompanionReservePaymentCallback(
                    reserve.Id,
                    successfulPayment.IsOnline);
                if (!reconcileResult.IsSuccess || !reserve.IsReserved)
                {
                    return new BaseResultDto<CompanionReserveAdminVDto>(
                        false,
                        "پرداخت موفق است اما اعمال آن روی رزرو ناموفق بود؛ گزارش Callback پرداخت را بررسی کنید.",
                        null);
                }

                if (!successfulPayment.AppliedDate.HasValue)
                {
                    successfulPayment.AppliedDate = DateTime.UtcNow;
                    successfulPayment.GatewayStatus = successfulPayment.GatewayStatus?.StartsWith("TEST_") == true
                        ? "TEST_APPLIED"
                        : successfulPayment.IsOnline
                            ? "APPLIED"
                            : "MANUAL_APPLIED";
                }
            }

            if (reserve.OperatorStateId == (long)CompanionReserveOperatorStateEnum.OperatorState_Complete)
            {
                return new BaseResultDto<CompanionReserveAdminVDto>(
                    false,
                    "رزرو انجام‌شده قابل تخصیص مجدد نیست.",
                    null);
            }

            var assignee = await _context.CompanionAssistanceUsers
                .AsNoTracking()
                .Include(s => s.User)
                .FirstOrDefaultAsync(s =>
                    s.Id == dto.CompanionAssistanceUserId &&
                    s.CompanionAssistanceId == reserve.CompanionAssistanceId &&
                    s.Active &&
                    !s.Deleted &&
                    !s.User.Deleted);

            if (assignee == null)
            {
                return new BaseResultDto<CompanionReserveAdminVDto>(
                    false,
                    "کاربر انتخاب‌شده متعلق به این خدمت نیست یا فعال نیست.",
                    null);
            }

            var validMembership = await _context.CompanionUsers
                .AsNoTracking()
                .AnyAsync(s =>
                    s.CompanionId == reserve.CompanionAssistance.CompanionId &&
                    s.UserId == assignee.UserId &&
                    !s.Deleted &&
                    s.Active &&
                    s.UserAccept == true);

            if (!validMembership)
            {
                return new BaseResultDto<CompanionReserveAdminVDto>(
                    false,
                    "عضویت کاربر انتخاب‌شده در این نمایندگی فعال و تأییدشده نیست.",
                    null);
            }

            if (reserve.CompanionAssistanceUserId == assignee.Id)
            {
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return await FindAsyncAdminVDto(reserve.Id);
            }

            if (reserve.CompanionAssistanceTime != null &&
                await HasAssigneeScheduleConflictAsync(
                    reserve.Id,
                    reserve.DoDate,
                    reserve.CompanionAssistanceTime,
                    assignee.Id))
            {
                return new BaseResultDto<CompanionReserveAdminVDto>(
                    false,
                    "کاربر انتخاب‌شده در این بازه زمانی رزرو فعال دیگری دارد.",
                    null);
            }

            reserve.CompanionAssistanceUserId = assignee.Id;
            reserve.OperatorStateId = (long)CompanionReserveOperatorStateEnum.OperatorState_InComplete;
            reserve.OperatorChangeStateDate = null;
            reserve.OperatorDetail = null;
            reserve.OperatorStuffPrice = 0;
            reserve.OperatorWagesPrice = 0;
            reserve.OperatorFinalPrice = 0;
            reserve.UserResponse = null;
            _context.CompanionReserves.Update(reserve);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var bookerName = $"{reserve.Booker.FirstName} {reserve.Booker.LastName}".Trim();
            await RunPostCommitActionAsync(
                () => _pushNotificationService.SendPushAsync(
                    PushTypeEnum.PushCompanionReserveAssigned,
                    assignee.UserId,
                    token1: reserve.CompanionAssistance.Assistance.Name,
                    token2: bookerName),
                reserve.Id,
                "assigned operator push");

            return await FindAsyncAdminVDto(reserve.Id);
        }

        private static bool IsSuccessfulPaymentSnapshotValid(
            Payment payment,
            CompanionReserve reserve)
        {
            const double tolerance = 0.01;
            var expectedGross = reserve.PrePaymentPrice + reserve.RebatePrice;
            var expectedGateway = Math.Max(0, reserve.PrePaymentPrice - reserve.WalletPrice);

            if (!payment.IsOnline)
            {
                return Math.Abs(payment.Amount - reserve.PrePaymentPrice) <= tolerance;
            }

            return Math.Abs(payment.GrossAmount - expectedGross) <= tolerance &&
                   Math.Abs(payment.RebateAmount - reserve.RebatePrice) <= tolerance &&
                   Math.Abs(payment.WalletAmount - reserve.WalletPrice) <= tolerance &&
                   Math.Abs(payment.Amount - expectedGateway) <= tolerance;
        }

        public async Task<BaseResultDto<CompanionReserveVDto>> FindAsyncOperatorVDto(long id)
        {
            var operatorUserId = _currentUser.CurrentUser.UserId;
            var item = await _context.CompanionReserves
                .AsNoTracking()
                .Include(s => s.State)
                .Include(s => s.CompanionAssistanceUser)
                    .ThenInclude(s => s.User)
                .Include(s => s.Booker)
                .Include(s => s.UserPets)
                .Include(s => s.CompanionAssistance)
                    .ThenInclude(s => s.Assistance)
                .Include(s => s.CompanionAssistance)
                    .ThenInclude(s => s.Companion)
                .Include(s => s.CompanionAssistancePackages)
                    .ThenInclude(s => s.Picture)
                .Include(s => s.CompanionAssistanceTime)
                    .ThenInclude(s => s.WeekDay)
                .Include(s => s.CompanionAssistanceType)
                .Include(s => s.OperatorState)
                .Include(s => s.Rebate)
                .FirstOrDefaultAsync(s =>
                    s.Id == id &&
                    s.IsReserved &&
                    s.CompanionAssistanceUser.UserId == operatorUserId &&
                    _context.CompanionUsers.Any(companionUser =>
                        companionUser.CompanionId == s.CompanionAssistance.CompanionId &&
                        companionUser.UserId == operatorUserId &&
                        !companionUser.Deleted &&
                        companionUser.Active &&
                        companionUser.UserAccept == true));

            if (item == null)
            {
                return new BaseResultDto<CompanionReserveVDto>(
                    false,
                    Resource.Notification.NothingFound,
                    null);
            }

            return new BaseResultDto<CompanionReserveVDto>(true, mapper.Map<CompanionReserveVDto>(item));
        }

        private async Task<bool> HasAssigneeScheduleConflictAsync(
            long reserveId,
            DateTime doDate,
            CompanionAssistanceTime assistanceTime,
            long companionAssistanceUserId)
        {
            if (!TryGetTimeRange(assistanceTime, out var targetStart, out var targetEnd))
            {
                return false;
            }

            var dayStart = doDate.Date;
            var dayEnd = dayStart.AddDays(1);
            var assignedReserves = await _context.CompanionReserves
                .AsNoTracking()
                .Include(s => s.CompanionAssistanceTime)
                .Where(s =>
                    s.Id != reserveId &&
                    s.CompanionAssistanceUserId == companionAssistanceUserId &&
                    s.IsReserved &&
                    !s.IsCancel &&
                    s.DoDate >= dayStart &&
                    s.DoDate < dayEnd)
                .ToListAsync();

            return assignedReserves.Any(s =>
                s.CompanionAssistanceTime != null &&
                TryGetTimeRange(s.CompanionAssistanceTime, out var existingStart, out var existingEnd) &&
                ReservationScheduleValidator.HasTimeRangeOverlap(
                    existingStart,
                    existingEnd,
                    targetStart,
                    targetEnd));
        }

        private static bool TryGetTimeRange(
            CompanionAssistanceTime assistanceTime,
            out TimeSpan start,
            out TimeSpan end)
        {
            return ReservationScheduleValidator.TryGetServiceTimeRange(
                assistanceTime.StartTime,
                assistanceTime.EndTime,
                out start,
                out end);
        }

        public async Task<BaseResultDto> UpdateAsyncDto(CompanionReserveUpdateDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<CompanionReserveUpdateDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                    return modelCheker;

                var item = await _context.CompanionReserves.AsTracking().Include(r => r.CompanionAssistance).Include(r => r.CompanionAssistancePackages).FirstOrDefaultAsync(r =>
                    r.Id == dto.Id &&
                    r.BookerId == _currentUser.CurrentUser.UserId &&
                    !r.IsReserved &&
                    !r.IsCancel);

                if (item == null)
                    return new BaseResultDto(isSuccess: false, val: Resource.Notification.NothingFound);

                if (item.IsCancel)
                    return new BaseResultDto(isSuccess: false, val: Resource.Notification.InvalidData);

                if (dto.CompanionAssistanceTimeId.HasValue)
                {
                    var selectedTime = await _context.CompanionAssistanceTimes
                        .AsNoTracking()
                        .Include(s => s.WeekDay)
                        .FirstOrDefaultAsync(s =>
                            s.Id == dto.CompanionAssistanceTimeId.Value &&
                            s.CompanionAssistanceId == item.CompanionAssistanceId &&
                            s.Active &&
                            !s.Deleted);

                    if (selectedTime == null)
                        return new BaseResultDto(false, "زمان انتخاب‌شده متعلق به این خدمت نیست یا فعال نیست.");

                    if (!ReservationScheduleValidator.IsWeekDayMatch(item.DoDate, selectedTime.WeekDay?.Label))
                        return new BaseResultDto(false, "روز تاریخ رزرو با روز زمان انتخاب‌شده هماهنگ نیست.");

                    if (!ReservationScheduleValidator.TryGetServiceStartDateTime(item.DoDate, selectedTime.StartTime, out var serviceStart) ||
                        serviceStart <= DateTime.Now)
                        return new BaseResultDto(false, "امکان انتخاب زمان گذشته وجود ندارد.");
                }
                else if (await _context.CompanionAssistanceTimes.AnyAsync(s =>
                             s.CompanionAssistanceId == item.CompanionAssistanceId &&
                             s.Active &&
                             !s.Deleted))
                {
                    return new BaseResultDto(false, "انتخاب زمان خدمت الزامی است.");
                }

                item.CompanionAssistanceTimeId = dto.CompanionAssistanceTimeId;
                item.IsFemale = dto.IsFemale;

                var oldPaymentPrice = item.PaymentPrice;
                if (dto.UserPetIds == null) 
                { 
                    dto.UserPetIds = new List<long>(); 
                }
                if (!dto.UserPetIds.Any())
                { 
                    return new BaseResultDto<CompanionReserveUpdateDto>(false, Resource.Notification.SelectAtLeastOneType, dto);
                }
                dto.UserPetIds = dto.UserPetIds.Distinct().ToList();
                var ownedPetCount = await _context.UserPets.CountAsync(s =>
                    dto.UserPetIds.Contains(s.Id) && s.UserId == item.BookerId);
                if (ownedPetCount != dto.UserPetIds.Count)
                    return new BaseResultDto(false, Resource.Notification.InvalidData);
                await (_context as DbContext).Database.ExecuteSqlRawAsync("DELETE FROM CompanionReserveUserPet WHERE CompanionReservesId = {0}", item.Id);

                await _companionReserveUserPetService.InsertOrUpdateAsync(item, dto.UserPetIds);

                dto.UserPetIds = dto.UserPetIds.Distinct().ToList();
                var petCount = dto.UserPetIds.Count;
                var packages = item.CompanionAssistancePackages.ToList();

                item.PackagePrice = packages.Sum(p => p.Price) * petCount;
                item.PrePaymentPrice = packages.Sum(p => p.PrePaymentPrice) * petCount;
                item.PaymentPrice = item.PrePaymentPrice;

                if (oldPaymentPrice != item.PaymentPrice)
                    UpdateCompanionReserveCommission(item);

                await _context.SaveChangesAsync();

                await RunPostCommitActionAsync(
                    () => _notificationService.CreateAsync(new NoticeCreateDto { Label = NoticeTypeLabels.CompanionReserveUpdated, ReferenceType = "CompanionReserve", ReferenceId = item.Id, DeduplicationKey = $"{NoticeTypeLabels.CompanionReserveUpdated}:{item.Id}:{DateTime.UtcNow.Ticks}" }),
                    item.Id,
                    "update notice");

                return new BaseResultDto(isSuccess: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Updating companion reserve {ReserveId} failed.", dto?.Id);
                return new BaseResultDto(isSuccess: false, val: Resource.Notification.Unsuccess);
            }
        }

        public async Task<BaseResultDto> CompanionReservePaymentCallback(long? reserveId, bool fromWallet = false)
        {
            try
            {
                var reserve = await _context.CompanionReserves
                    .Include(s => s.Booker)
                    .Include(s => s.Rebate)
                    .Include(s => s.CompanionAssistanceUser)
                    .Include(s => s.CompanionAssistance)
                        .ThenInclude(s => s.Assistance)
                    .AsTracking()
                    .FirstOrDefaultAsync(s => s.Id == reserveId);

                if (reserve == null)
                    return new BaseResultDto(false);
                if (reserve.IsReserved)
                    return new BaseResultDto(true);

                if (fromWallet && reserve.FromWallet && reserve.WalletPrice > 0)
                {
                    var walletItem = new WalletDto()
                    {
                        Painding = false,
                        Amount = reserve.WalletPrice,
                        UserId = reserve.Booker.Id,
                        CompanionReserveId = reserve.Id
                    };
                    var walletResult = await _walletService.InsertUpdateReserveAsync(walletItem, true);
                    if (!walletResult.IsSuccess)
                        return new BaseResultDto(false);
                }
                if (reserve.Rebate != null)
                {
                    _rebateService.IncreaseUseCount(reserve);
                }
                var prePaidStatus = await _context.Codes
                    .AsNoTracking()
                    .Where(s => s.Label == CompanionReserveStateEnum.CompanianReserveState_PrePaid.ToString())
                    .Select(s => (long?)s.Id)
                    .FirstOrDefaultAsync();
                reserve.IsReserved = true;
                if (prePaidStatus.HasValue)
                {
                    reserve.StateId = prePaidStatus.Value;
                }
                else
                {
                    _logger.LogError(
                        "Companion reserve prepaid state code was not found while applying payment for reserve {ReserveId}.",
                        reserve.Id);
                }

                if (reserve.CompanionAssistance != null)
                    UpdateCompanionReserveCommission(reserve);

                await _context.SaveChangesAsync();

                double scoreRatio = 10000;
                double earnedScore = Math.Floor(reserve.PaymentPrice / scoreRatio);

                if (earnedScore > 0)
                {
                    try
                    {
                        var scoreTypeExists = await _context.Codes
                            .AsNoTracking()
                            .AnyAsync(s =>
                                s.Label == ScoreTransactionType.ScoreTransactionType_CompanionReserve.ToString() &&
                                s.Active);
                        if (scoreTypeExists)
                        {
                            var scoreResult = await _scoreService.AddScoreAsync(
                                userId: reserve.BookerId,
                                amount: earnedScore,
                                type: ScoreTransactionType.ScoreTransactionType_CompanionReserve,
                                referenceId: reserve.Id.ToString());
                            if (!scoreResult.IsSuccess)
                            {
                                _logger.LogWarning(
                                    "Legacy score was not recorded for paid companion reserve {ReserveId}.",
                                    reserve.Id);
                            }
                        }
                        else
                        {
                            _logger.LogWarning(
                                "Legacy score type is missing for paid companion reserve {ReserveId}; payment application continued.",
                                reserve.Id);
                        }
                    }
                    catch (Exception exception)
                    {
                        _logger.LogError(
                            exception,
                            "Legacy score failed for paid companion reserve {ReserveId}; payment application continued.",
                            reserve.Id);
                    }
                }

                if (reserve.CompanionAssistanceUser != null)
                {
                    await RunPostCommitActionAsync(
                        () => _pushNotificationService.SendPushAsync(
                            PushTypeEnum.PushCompanionReserveAssigned,
                            reserve.CompanionAssistanceUser.UserId,
                            token1: reserve.CompanionAssistance.Assistance.Name,
                            token2: $"{reserve.Booker.FirstName} {reserve.Booker.LastName}".Trim()),
                        reserve.Id,
                        "assigned operator push after payment");
                }

                return new BaseResultDto(true, Resource.Notification.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Companion reserve payment callback failed for reserve {ReserveId}.", reserveId);
                return new BaseResultDto(false);
            }
        }
        public async Task<BaseResultDto> UpdateCancelDto(CompanionReserveCancelDto dto)
        {
            var model = await _context.CompanionReserves.FirstOrDefaultAsync(s => s.Id == dto.Id && s.IsReserved);

            if (model == null)
            {
                return new BaseResultDto<CompanionReserveCancelDto>(false, null);
            }

            if (_currentUser.CurrentUser.RoleEnum != RoleEnum.Admin.ToString())
            {
                return new BaseResultDto<CompanionReserveCancelDto>(false, null);
            }

            if ((model.StateId == (long)CompanionReserveStateEnum.CompanianReserveState_Paid || model.StateId == (long)CompanionReserveStateEnum.CompanianReserveState_Complete)
                && _currentUser.CurrentUser.CompanionId.HasValue && (_currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin || _currentUser.CurrentUser.RoleId == (long)RoleEnum.Customer))
            {
                return new BaseResultDto<CompanionReserveCancelDto>(false, Resource.Notification.YouCanNotCancelThisReservation, dto);
            }
            if (dto.IsCancel)
            {
                if (string.IsNullOrWhiteSpace(dto.CancelDetail))
                {
                    return new BaseResultDto<CompanionReserveCancelDto>(false, Resource.Notification.PleaseEnterCancelDetail, dto);
                }
                model.IsCancel = true;
                model.CancelDetail = dto.CancelDetail;
                model.CancelDate = DateTime.Now;
            }
            _context.CompanionReserves.Update(model);
            await _context.SaveChangesAsync();

            if (dto.IsCancel)
                await _clubPointIntegrationService.CompanionReserveReversedAsync(model.BookerId, model.Id);

            var booker = _context.Users.FirstOrDefault(u => u.Id == model.BookerId);
            var companionAssistances = _context.CompanionAssistances.Include(s => s.Assistance).Include(s => s.Companion).FirstOrDefault(a => a.Id == model.CompanionAssistanceId);
            var companion = _context.Companions.Include(s => s.Owner).FirstOrDefault(a => a.Id == companionAssistances.CompanionId);
            string nameText = string.Format("{0}_{1}", booker.FirstName, booker.LastName).Replace(" ", "_");

            await _pushNotificationService.SendPushAsync(pushType: PushTypeEnum.PushCancelReserveUser, userId: booker.Id, token1: booker.FirstName, token2: companionAssistances.Assistance.Name);
            await _pushNotificationService.SendPushAsync(pushType: PushTypeEnum.PushCancelReserveCompanion, userId: companion.Owner.Id, token1: companionAssistances.Assistance.Name, token2: nameText);
            return new BaseResultDto<CompanionReserveCancelDto>(true, mapper.Map<CompanionReserveCancelDto>(model));
        }

        public async Task<BaseResultDto> UpdateReserveStateDto(CompanionReserveChangeStateDto dto)
        {
            var item = await _context.CompanionReserves.FirstOrDefaultAsync(s => s.Id == dto.Id);

            if (item.PaymentPrice == 0)
            {
                return new BaseResultDto<CompanionReserveChangeStateDto>(false, Resource.Notification.TheFinalPriceHasNotYetBeenRecordedForThisReserve, dto);
            }

            item.StateId = dto.StateId;

            _context.CompanionReserves.Update(item);
            _context.SaveChanges();
            return new BaseResultDto<CompanionReserveChangeStateDto>(true, mapper.Map<CompanionReserveChangeStateDto>(item));
        }

        public async Task<BaseResultDto> CompanionReserveOperatorUpdateAsyncDto(CompanionReserveOperatorDto dto)
        {
            var item = await _context.CompanionReserves.Include(s => s.CompanionAssistance).ThenInclude(s => s.Assistance).Include(s => s.UserPets).Include(s => s.Booker).Include(s => s.CompanionAssistanceUser).Include(s => s.CompanionAssistance).ThenInclude(s => s.Companion).AsTracking().FirstOrDefaultAsync(s => s.Id == dto.Id && s.IsReserved && !s.IsCancel);

            if (item == null)
            {
                return new BaseResultDto<CompanionReserveOperatorDto>(false, Resource.Notification.NothingFound, dto);
            }

            if (item.CompanionAssistanceUser == null ||
                !item.CompanionAssistanceUser.Active ||
                item.CompanionAssistanceUser.Deleted ||
                item.CompanionAssistanceUser.UserId != _currentUser.CurrentUser.UserId)
            {
                return new BaseResultDto<CompanionReserveOperatorDto>(false, Resource.Notification.ThisReserveIsNotBlongToYou, dto);
            }

            var activeMembership = await _context.CompanionUsers
                .AsNoTracking()
                .AnyAsync(s =>
                    s.CompanionId == item.CompanionAssistance.CompanionId &&
                    s.UserId == _currentUser.CurrentUser.UserId &&
                    !s.Deleted &&
                    s.Active &&
                    s.UserAccept == true);

            if (!activeMembership)
            {
                return new BaseResultDto<CompanionReserveOperatorDto>(
                    false,
                    Resource.Notification.AccessDenied,
                    dto);
            }

            if (dto.OperatorStateId == (long)CompanionReserveOperatorStateEnum.OperatorState_InComplete)
            {
                return new BaseResultDto<CompanionReserveOperatorDto>(false, Resource.Notification.PleaseChangeTheStatus, dto);
            }

            if (item.OperatorStateId == (long)CompanionReserveOperatorStateEnum.OperatorState_Complete)
            {
                if (dto.OperatorStateId == (long)CompanionReserveOperatorStateEnum.OperatorState_Complete)
                    return new BaseResultDto<CompanionReserveOperatorDto>(
                        true,
                        mapper.Map<CompanionReserveOperatorDto>(item));

                return new BaseResultDto<CompanionReserveOperatorDto>(false, Resource.Notification.ThisReserveStateHasCompletedBefore, dto);
            }

            if (dto.OperatorStateId == (long)CompanionReserveOperatorStateEnum.OperatorState_Cancelled)
            {
                if (string.IsNullOrWhiteSpace(dto.OperatorDetail))
                {
                    return new BaseResultDto<CompanionReserveOperatorDto>(false, Resource.Notification.PleaseEnterCancelDetail, dto);
                }
            }

            return await ApplyOperatorUpdateAsync(item, dto);
        }

        public async Task<BaseResultDto> CompanionReserveCompanionUpdateAsyncDto(CompanionReserveOperatorDto dto)
        {
            var item = await _context.CompanionReserves.Include(s => s.CompanionAssistance).ThenInclude(s => s.Assistance).Include(s => s.CompanionAssistance).ThenInclude(s => s.Companion).Include(s => s.UserPets).Include(s => s.Booker).Include(s => s.CompanionAssistanceUser).Include(s => s.CompanionAssistance).ThenInclude(s => s.Companion).AsTracking().FirstOrDefaultAsync(s => s.Id == dto.Id && s.IsReserved && !s.IsCancel);

            if (item == null)
            {
                return new BaseResultDto<CompanionReserveOperatorDto>(false, Resource.Notification.NothingFound, dto);
            }

            if (item.CompanionAssistance.Companion.OwnerId != _currentUser.CurrentUser.UserId)
            {
                return new BaseResultDto<CompanionReserveOperatorDto>(false, Resource.Notification.ThisReserveIsNotBlongToYou, dto);
            }

            if (dto.OperatorStateId == (long)CompanionReserveOperatorStateEnum.OperatorState_InComplete)
            {
                return new BaseResultDto<CompanionReserveOperatorDto>(false, Resource.Notification.PleaseChangeTheStatus, dto);
            }

            if (item.OperatorStateId == (long)CompanionReserveOperatorStateEnum.OperatorState_Complete)
            {
                if (dto.OperatorStateId == (long)CompanionReserveOperatorStateEnum.OperatorState_Complete)
                    return new BaseResultDto<CompanionReserveOperatorDto>(
                        true,
                        mapper.Map<CompanionReserveOperatorDto>(item));

                return new BaseResultDto<CompanionReserveOperatorDto>(false, Resource.Notification.ThisReserveStateHasCompletedBefore, dto);
            }

            if (dto.OperatorStateId == (long)CompanionReserveOperatorStateEnum.OperatorState_Cancelled)
            {
                if (string.IsNullOrWhiteSpace(dto.OperatorDetail))
                {
                    return new BaseResultDto<CompanionReserveOperatorDto>(false, Resource.Notification.PleaseEnterCancelDetail, dto);
                }
            }

            return await ApplyOperatorUpdateAsync(item, dto);
        }

        private async Task<BaseResultDto> ApplyOperatorUpdateAsync(
            CompanionReserve item,
            CompanionReserveOperatorDto dto)
        {
            if (dto.OperatorWagesPrice < 0 || dto.OperatorStuffPrice < 0)
            {
                return new BaseResultDto<CompanionReserveOperatorDto>(
                    false,
                    Resource.Notification.InvalidData,
                    dto);
            }

            var paidStateId = await _context.Codes
                .AsNoTracking()
                .Where(s => s.Label == CompanionReserveStateEnum.CompanianReserveState_Paid.ToString())
                .Select(s => (long?)s.Id)
                .FirstOrDefaultAsync();
            if (!paidStateId.HasValue)
            {
                _logger.LogError(
                    "Companion reserve paid state code was not found while updating reserve {ReserveId}.",
                    item.Id);
                return new BaseResultDto<CompanionReserveOperatorDto>(
                    false,
                    "وضعیت پرداخت‌شده رزرو در تنظیمات سیستم پیدا نشد.",
                    dto);
            }

            try
            {
                item.OperatorStateId = dto.OperatorStateId;
                item.OperatorDetail = dto.OperatorDetail?.Trim();
                item.OperatorChangeStateDate = DateTime.Now;
                item.UserResponse = true;
                item.StateId = paidStateId.Value;
                item.OperatorWagesPrice = dto.OperatorWagesPrice;
                item.OperatorStuffPrice = dto.OperatorStuffPrice;
                item.OperatorFinalPrice = dto.OperatorStuffPrice + dto.OperatorWagesPrice;
                item.PaymentPrice = item.OperatorFinalPrice;

                _context.CompanionReserves.Update(item);
                await _context.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Saving operator state {OperatorStateId} for companion reserve {ReserveId} failed.",
                    dto.OperatorStateId,
                    item.Id);
                return new BaseResultDto<CompanionReserveOperatorDto>(
                    false,
                    Resource.Notification.Unsuccess,
                    dto);
            }

            if (dto.OperatorStateId == (long)CompanionReserveOperatorStateEnum.OperatorState_Complete)
            {
                await RunPostCommitActionAsync(
                    () => _clubPointIntegrationService.CompanionReserveCompletedAsync(item.BookerId, item.Id),
                    item.Id,
                    "club completion point");
            }
            else if (dto.OperatorStateId == (long)CompanionReserveOperatorStateEnum.OperatorState_Cancelled)
            {
                await RunPostCommitActionAsync(
                    () => _clubPointIntegrationService.CompanionReserveReversedAsync(item.BookerId, item.Id),
                    item.Id,
                    "club completion point reversal");
            }

            await RunPostCommitActionAsync(
                () => _messageSender.SendMessageAsync(
                    messageType: MessageTypeEnum.UserAcceptOperatorChange,
                    mobileReceptor: item.Booker.Mobile,
                    emailReceptor: item.Booker.Email,
                    token1: item.CompanionAssistance.Companion.Name,
                    token2: item.PaymentPrice.ToString(),
                    token3: item.CompanionAssistance.Assistance.Name),
                item.Id,
                "operator state message");

            var pushType = dto.OperatorStateId == (long)CompanionReserveOperatorStateEnum.OperatorState_Cancelled
                ? PushTypeEnum.PushCancelReserveUser
                : PushTypeEnum.PushCompleteReserveUser;
            await RunPostCommitActionAsync(
                () => _pushNotificationService.SendPushAsync(
                    pushType,
                    item.Booker.Id,
                    token1: item.Booker.FirstName,
                    token2: item.CompanionAssistance.Assistance.Name),
                item.Id,
                "operator state push");

            return new BaseResultDto<CompanionReserveOperatorDto>(
                true,
                mapper.Map<CompanionReserveOperatorDto>(item));
        }

        public async Task<BaseResultDto> CompanionReserveUserResponseAsyncDto(CompanionReserveUserResponseDto dto)
        {
            var item = await _context.CompanionReserves.Include(s => s.CompanionAssistance).ThenInclude(s => s.Assistance).Include(s => s.UserPets).Include(s => s.Booker).Include(s => s.CompanionAssistanceUser).Include(s => s.CompanionAssistance).ThenInclude(s => s.Companion).AsTracking().FirstOrDefaultAsync(s => s.Id == dto.Id);
            if (item.BookerId != _currentUser.CurrentUser.UserId)
            {
                return new BaseResultDto<CompanionReserveUserResponseDto>(false, Resource.Notification.ThisReserveIsNotBlongToYou, dto);
            }
            item.UserResponse = dto.UserResponse;
            item.StateId = (long)CompanionReserveStateEnum.CompanianReserveState_Paid;

            _context.CompanionReserves.Update(item);
            await _context.SaveChangesAsync();
            if (dto.UserResponse == false)
            {

                await _messageSender.SendMessageAsync(
        messageType: MessageTypeEnum.AdminNotifyUserResponse,
        mobileReceptor: _adminSettingHelper.BaseAdminSetting.AdminMobiles,
        emailReceptor: null,
        token1: item.Booker.Mobile,
        token2: item.CompanionAssistance.Companion.Name,
        token3: item.PaymentPrice.ToString()
    );
            }
            return new BaseResultDto<CompanionReserveUserResponseDto>(true, mapper.Map<CompanionReserveUserResponseDto>(item));
        }

        public async Task<BaseResultDto> SetRebateCodeAsyncDto(CompanionReserveSetRebateCodeDto dto)
        {
            var item = await _context.CompanionReserves.AsTracking()
                .Include(s => s.CompanionAssistancePackages)
                .FirstOrDefaultAsync(s =>
                s.Id == dto.Id &&
                s.BookerId == _currentUser.CurrentUser.UserId &&
                s.StateId == (long)CompanionReserveStateEnum.CompanianReserveState_Registered);

            if (item == null)
            {
                return new BaseResultDto<CompanionReserveSetRebateCodeDto>(false, Resource.Notification.NothingFound, dto);
            }
            if (string.IsNullOrEmpty(dto.RebateCode))
            {
                return new BaseResultDto(isSuccess: false, val: Resource.Notification.Unsuccess);
            }
            if (await HasActivePaymentAsync(item.Id))
                return new BaseResultDto(false, "پرداخت این رزرو شروع شده و اطلاعات مالی آن قابل تغییر نیست.");
            var originalPrice = item.PrePaymentPrice + item.RebatePrice;
            item.PrePaymentPrice = originalPrice;
            var rebate = _rebateService.GetRebateByCodeAsync(item, dto.RebateCode);
            if (rebate.IsSuccess)
            {
                item.Rebate = null;
                item.RebateId = rebate.Data.Id;
                item.RebatePrice = rebate.Data.FinalPrice;
                item.PrePaymentPrice = originalPrice - item.RebatePrice;
                if (item.PrePaymentPrice < 0)
                {
                    item.PrePaymentPrice = 0;
                }
                if (item.WalletPrice != 0)
                {
                    item.WalletPrice = item.PrePaymentPrice;
                }
                _context.CompanionReserves.Update(item);
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
            var item = await _context.CompanionReserves.AsTracking().FirstOrDefaultAsync(s =>
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
            item.PrePaymentPrice = item.PrePaymentPrice + item.RebatePrice;
            item.RebatePrice = 0;
            if (item.WalletPrice != 0)
            {
                item.WalletPrice = item.PrePaymentPrice;
            }
            _context.CompanionReserves.Update(item);
            await _context.SaveChangesAsync();
            return new BaseResultDto(isSuccess: true, val: Resource.Notification.Success);
        }

        public async Task<BaseResultDto> SetWalletAsyncDto(CompanionReserveSetWalletDto dto)
        {
            var item = await _context.CompanionReserves.Include(s => s.UserPets).AsTracking().FirstOrDefaultAsync(s =>
                s.Id == dto.Id && s.BookerId == _currentUser.CurrentUser.UserId);
            if (item == null)
            {
                return new BaseResultDto<CompanionReserveSetWalletDto>(false, Resource.Notification.NothingFound, dto);
            }
            if (await HasActivePaymentAsync(item.Id))
                return new BaseResultDto(false, "پرداخت این رزرو شروع شده و اطلاعات مالی آن قابل تغییر نیست.");
            if (item.StateId == (long)CompanionReserveStateEnum.CompanianReserveState_Complete)
            {
                return new BaseResultDto<CompanionReserveSetWalletDto>(false, Resource.Notification.ThisReserveIsCompleted, dto);
            }
            if (item.StateId == (long)CompanionReserveStateEnum.CompanianReserveState_Paid)
            {
                return new BaseResultDto<CompanionReserveSetWalletDto>(false, Resource.Notification.ThisReserveIsPaid, dto);
            }
            if (dto.FromWallet)
            {
                item.FromWallet = true;
                item.WalletPrice = item.PrePaymentPrice;
            }
            else
            {
                item.FromWallet = false;
                item.WalletPrice = 0;
            }
            _context.CompanionReserves.Update(item);
            await _context.SaveChangesAsync();
            return new BaseResultDto(isSuccess: true, val: Resource.Notification.Success);
        }

        private Task<bool> HasActivePaymentAsync(long reserveId)
        {
            var callbackId = reserveId.ToString();
            return _context.Payments.AsNoTracking().AnyAsync(s =>
                s.CallBackTypeLabel == PaymentCallbackTypeEnum.CompanionReserve.ToString() &&
                s.CallBackId == callbackId &&
                (s.IsSuccess == null || s.IsSuccess == true));
        }

        private void UpdateCompanionReserveCommission(CompanionReserve item)
        {
            decimal total = (decimal)item.PaymentPrice;
            decimal sharePercent = item.CompanionAssistance.CommissionPercent;
            decimal siteShare = (total * sharePercent) / 100m;
            decimal companionShare = total - siteShare;
            item.CompanionShare = (double)companionShare;
            item.SiteShare = (double)siteShare;
        }

        public async Task<BaseResultDto<int>> ReserveCountAsync(long id)
        {
            var count = await _context.CompanionReserves.Include(s => s.CompanionAssistance).CountAsync(s => s.CompanionAssistance.CompanionId == id);
            return new BaseResultDto<int>(true, count);
        }

        public async Task<BaseResultDto> UpdatePermittedAsyncDto(long id)
        {
            var item = await _context.CompanionReserves.AsTracking().FirstOrDefaultAsync(s => s.Id == id);
            item.Permitted = true;
            _context.CompanionReserves.Update(item);
            await _context.SaveChangesAsync();
            return new BaseResultDto(isSuccess: true, val: Resource.Notification.Success);
        }
    }
}
