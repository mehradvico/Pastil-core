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
using Persistence.Interface;
using System;
using System.Collections.Generic;
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
        public CompanionReserveService(IDataBaseContext _context, IPushNotificationService pushNotificationService, IMapper mapper,
            ICompanionReservePackageService companionReservePackageService, ICompanionReserveUserPetService companionReserveUserPetService,
            IWalletService walletService, IRebateService rebateService, IAdminSettingHelper adminSettingHelper, ICodeService codeService,
            IMessageSenderService messageSender, ICurrentUserHelper currentUser, INoticeService notificationService, IScoreTransactionService scoreService) : base(_context, mapper)
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

        public async Task<BaseResultDto<CompanionReserveVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.CompanionReserves.Include(s => s.State).Include(s => s.CompanionAssistanceUser).ThenInclude(s => s.CompanionAssistance).ThenInclude(s => s.Companion)
                .Include(s => s.CompanionAssistanceUser).ThenInclude(s => s.User).Include(s => s.Booker).Include(s => s.UserPets)
                .Include(s => s.CompanionAssistance).ThenInclude(s => s.Assistance).Include(s => s.CompanionAssistance).ThenInclude(s => s.Companion)
                .Include(s => s.CompanionAssistancePackages).ThenInclude(s => s.Picture).Include(s => s.CompanionAssistanceTime).ThenInclude(s => s.WeekDay).Include(s => s.CompanionAssistanceType)
                .Include(s => s.OperatorState).Include(s => s.Rebate).Include(s => s.CompanionAssistance).ThenInclude(s => s.Assistance).ThenInclude(s => s.Picture).FirstOrDefaultAsync(s => s.Id == id);
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
                model = model.Where(s => s.CompanionAssistanceUser.UserId == baseSearchDto.CompanionAssistanceUserId.Value);
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
                    var companionAssistance = await _context.CompanionAssistances.Include(s => s.Companion).ThenInclude(s => s.CompanionZones).FirstOrDefaultAsync(s => s.Id == dto.CompanionAssistanceId);

                    if (dto.AddressId.HasValue)
                    {
                        var address = await _context.Addresses.FirstOrDefaultAsync(s => s.Id == dto.AddressId);
                        if (companionAssistance.Companion.CompanionZones.Any(s => s.CityId == address.CityId))
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

                    await _context.CompanionReserves.AddAsync(item);
                    await _context.SaveChangesAsync();

                    if (dto.UserPetIds == null)
                    {
                        dto.UserPetIds = new List<long>();
                    }
                    if (!dto.UserPetIds.Any())
                    {
                        return new BaseResultDto<CompanionReserveDto>(false, Resource.Notification.SelectAtLeastOneType, dto);
                    }
                    dto.UserPetIds = dto.UserPetIds.Distinct().ToList();
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

                    var booker = _context.Users.FirstOrDefault(u => u.Id == item.BookerId);
                    var companionAssistances = _context.CompanionAssistances.Include(s => s.Assistance).Include(s => s.Companion).FirstOrDefault(a => a.Id == item.CompanionAssistanceId);
                    var companion = _context.Companions.Include(s => s.Owner).FirstOrDefault(a => a.Id == companionAssistances.CompanionId);
                    var companionAssistanceUser = _context.CompanionAssistanceUsers.Include(s => s.User).FirstOrDefault(a => a.Id == item.CompanionAssistanceUserId);
                    var adminMobile = _adminSettingHelper.BaseAdminSetting.AdminMobiles;
                    string nameText = string.Format("{0}_{1}", booker.FirstName, booker.LastName).Replace(" ", "_");

                    await _messageSender.SendMessageAsync(messageType: MessageTypeEnum.CompanionReserveForUser, mobileReceptor: booker.Mobile, emailReceptor: null, token1: companionAssistances.Assistance.Name);
                    await _messageSender.SendMessageAsync(messageType: MessageTypeEnum.CompanionReserveForCompanion, mobileReceptor: companion.Owner.Mobile, emailReceptor: null, token1: companionAssistances.Assistance.Name, token2: booker.FirstName);
                    await _messageSender.SendMessageAsync(messageType: MessageTypeEnum.CompanionReserveForAdmin, mobileReceptor: adminMobile, emailReceptor: null, token1: companionAssistances.Assistance.Name, token2: companion.Name);
                    await _pushNotificationService.SendPushAsync(pushType: PushTypeEnum.PushRegisterReserveUser, userId: booker.Id, token1: companionAssistances.Assistance.Name, token2: booker.FirstName);
                    await _pushNotificationService.SendPushAsync(pushType: PushTypeEnum.PushRegisterReserveCompanion, userId: companion.Owner.Id, token1: companionAssistances.Assistance.Name, token2: nameText);

                    if (companionAssistanceUser != null)
                    {
                        await _messageSender.SendMessageAsync(messageType: MessageTypeEnum.CompanionReserveForCompanionUser, mobileReceptor: companionAssistanceUser.User.Mobile, emailReceptor: null, token1: companionAssistances.Assistance.Name, token2: companion.Name);
                    }
                    await _notificationService.CreateAsync(new NoticeCreateDto
                    {
                        Label = NoticeTypeLabels.CompanionReserveRegistered,
                        ActorUserId = booker.Id,
                        ReferenceType = "CompanionReserve",
                        ReferenceId = item.Id,
                        DeduplicationKey = $"{NoticeTypeLabels.CompanionReserveRegistered}:{item.Id}",
                        Metadata = new Dictionary<string, string> { { "userName", $"{booker.FirstName} {booker.LastName}".Trim() }, { "companionName", companion.Name }, { "serviceName", companionAssistances.Assistance.Name }, { "mobile", booker.Mobile } }
                    });
                    return new BaseResultDto<CompanionReserveDto>(true, mapper.Map<CompanionReserveDto>(item));
                }

            }
            catch (Exception ex)
            {
                return new BaseResultDto<CompanionReserveDto>(isSuccess: false, val: ex.Message, data: dto);
            }
        }
        public async Task<BaseResultDto> UpdateAsyncDto(CompanionReserveUpdateDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<CompanionReserveUpdateDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                    return modelCheker;

                var item = await _context.CompanionReserves.AsTracking().Include(r => r.CompanionAssistance).Include(r => r.CompanionAssistancePackages).FirstOrDefaultAsync(r => r.Id == dto.Id);

                if (item == null)
                    return new BaseResultDto(isSuccess: false, val: Resource.Notification.NothingFound);

                if (item.IsCancel)
                    return new BaseResultDto(isSuccess: false, val: Resource.Notification.InvalidData);

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

                await _notificationService.CreateAsync(new NoticeCreateDto { Label = NoticeTypeLabels.CompanionReserveUpdated, ReferenceType = "CompanionReserve", ReferenceId = item.Id, DeduplicationKey = $"{NoticeTypeLabels.CompanionReserveUpdated}:{item.Id}:{DateTime.UtcNow.Ticks}" });

                return new BaseResultDto(isSuccess: true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(isSuccess: false, val: ex.Message);
            }
        }

        public async Task<BaseResultDto> CompanionReservePaymentCallback(long? reserveId, bool fromWallet = false)
        {
            try
            {
                var reserve = await _context.CompanionReserves.Include(s => s.Booker).Include(s => s.Rebate).Include(s => s.CompanionAssistance).AsTracking().FirstOrDefaultAsync(s => s.Id == reserveId);

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
                reserve.IsReserved = true;
                var prePaidStatus = await _codeService.GetIdByLabelAsync(CompanionReserveStateEnum.CompanianReserveState_PrePaid.ToString());
                reserve.StateId = prePaidStatus;

                if (reserve.CompanionAssistance != null)
                    UpdateCompanionReserveCommission(reserve);

                double scoreRatio = 10000;
                double earnedScore = Math.Floor(reserve.PaymentPrice / scoreRatio);

                if (earnedScore > 0)
                {
                    await _scoreService.AddScoreAsync(
                        userId: reserve.BookerId,
                        amount: earnedScore,
                        type: ScoreTransactionType.ScoreTransactionType_CompanionReserve,
                        referenceId: reserve.Id.ToString()
                    );
                }

                await _context.SaveChangesAsync();

                return new BaseResultDto(true, Resource.Notification.Success);
            }
            catch (Exception)
            {
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

            if (item.CompanionAssistanceUser.UserId != _currentUser.CurrentUser.UserId)
            {
                return new BaseResultDto<CompanionReserveOperatorDto>(false, Resource.Notification.ThisReserveIsNotBlongToYou, dto);
            }

            if (dto.OperatorStateId == (long)CompanionReserveOperatorStateEnum.OperatorState_InComplete)
            {
                return new BaseResultDto<CompanionReserveOperatorDto>(false, Resource.Notification.PleaseChangeTheStatus, dto);
            }

            if (item.OperatorStateId == (long)CompanionReserveOperatorStateEnum.OperatorState_Complete)
            {
                return new BaseResultDto<CompanionReserveOperatorDto>(false, Resource.Notification.ThisReserveStateHasCompletedBefore, dto);
            }

            if (dto.OperatorStateId == (long)CompanionReserveOperatorStateEnum.OperatorState_Cancelled)
            {
                if (string.IsNullOrWhiteSpace(dto.OperatorDetail))
                {
                    return new BaseResultDto<CompanionReserveOperatorDto>(false, Resource.Notification.PleaseEnterCancelDetail, dto);
                }
            }

            item.OperatorStateId = dto.OperatorStateId;
            item.OperatorDetail = dto.OperatorDetail;
            item.OperatorChangeStateDate = DateTime.Now;
            item.UserResponse = true;
            item.StateId = (long)CompanionReserveStateEnum.CompanianReserveState_Paid;
            item.OperatorWagesPrice = dto.OperatorWagesPrice;
            item.OperatorStuffPrice = dto.OperatorStuffPrice;
            item.OperatorFinalPrice = item.OperatorStuffPrice + item.OperatorWagesPrice;
            item.PaymentPrice = item.OperatorFinalPrice;

            _context.CompanionReserves.Update(item);
            await _context.SaveChangesAsync();

            await _messageSender.SendMessageAsync(
                messageType: MessageTypeEnum.UserAcceptOperatorChange,
                mobileReceptor: item.Booker.Mobile,
                emailReceptor: item.Booker.Email,
                token1: item.CompanionAssistance.Companion.Name,
                token2: item.PaymentPrice.ToString(),
                token3: item.CompanionAssistance.Assistance.Name
            );
            await _pushNotificationService.SendPushAsync(pushType: PushTypeEnum.PushCompleteReserveUser, userId: item.Booker.Id, token1: item.Booker.FirstName);

            return new BaseResultDto<CompanionReserveOperatorDto>(true, mapper.Map<CompanionReserveOperatorDto>(item));
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
                return new BaseResultDto<CompanionReserveOperatorDto>(false, Resource.Notification.ThisReserveStateHasCompletedBefore, dto);
            }

            if (dto.OperatorStateId == (long)CompanionReserveOperatorStateEnum.OperatorState_Cancelled)
            {
                if (string.IsNullOrWhiteSpace(dto.OperatorDetail))
                {
                    return new BaseResultDto<CompanionReserveOperatorDto>(false, Resource.Notification.PleaseEnterCancelDetail, dto);
                }
            }

            item.OperatorStateId = dto.OperatorStateId;
            item.OperatorDetail = dto.OperatorDetail;
            item.OperatorChangeStateDate = DateTime.Now;
            item.UserResponse = true;
            item.StateId = (long)CompanionReserveStateEnum.CompanianReserveState_Paid;
            item.OperatorWagesPrice = dto.OperatorWagesPrice;
            item.OperatorStuffPrice = dto.OperatorStuffPrice;
            item.OperatorFinalPrice = item.OperatorStuffPrice + item.OperatorWagesPrice;
            item.PaymentPrice = item.OperatorFinalPrice;

            _context.CompanionReserves.Update(item);
            await _context.SaveChangesAsync();

            await _messageSender.SendMessageAsync(
                messageType: MessageTypeEnum.UserAcceptOperatorChange,
                mobileReceptor: item.Booker.Mobile,
                emailReceptor: item.Booker.Email,
                token1: item.CompanionAssistance.Companion.Name,
                token2: item.PaymentPrice.ToString(),
                token3: item.CompanionAssistance.Assistance.Name
            );
            await _pushNotificationService.SendPushAsync(pushType: PushTypeEnum.PushCompleteReserveUser, userId: item.Booker.Id, token1: item.Booker.FirstName);

            return new BaseResultDto<CompanionReserveOperatorDto>(true, mapper.Map<CompanionReserveOperatorDto>(item));
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
            var item = await _context.CompanionReserves.AsTracking().FirstOrDefaultAsync(s =>
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
            var originalPrice = item.PrePaymentPrice + item.RebatePrice;
            var rebate = _rebateService.GetRebateByCodeAsync(
                originalPrice,
                item.BookerId,
                RebateTypeLabels.CompanionReserve,
                dto.RebateCode);
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
