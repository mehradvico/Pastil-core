using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.Order.PaymentSrv;
using Application.Services.Order.RebateSrv.Iface;
using Application.Services.ProductSrvs.WalletSrv.Dto;
using Application.Services.ProductSrvs.WalletSrv.IFace;
using Application.Services.SchoolSrvs.SchoolReserveSrv.Dto;
using Application.Services.SchoolSrvs.SchoolReserveSrv.Iface;
using AutoMapper;
using Entities.Entities.SchoolField;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.SchoolSrvs.SchoolReserveSrv
{
    public class SchoolReserveService : CommonSrv<SchoolReserve, SchoolReserveDto>, ISchoolReserveService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ICurrentUserHelper _currentUser;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly IRebateService _rebateService;
        private readonly IWalletService _walletService;
        private readonly ILogger<SchoolReserveService> _logger;

        public SchoolReserveService(
            IDataBaseContext _context,
            IMapper mapper,
            ICurrentUserHelper currentUser,
            IPushNotificationService pushNotificationService,
            IRebateService rebateService,
            IWalletService walletService,
            ILogger<SchoolReserveService> logger) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this._currentUser = currentUser;
            this._pushNotificationService = pushNotificationService;
            this._rebateService = rebateService;
            this._walletService = walletService;
            this._logger = logger;
        }

        public async Task<int> GetRemainingCapacityAsync(long schoolCourseId)
        {
            var course = await _context.SchoolCourses.AsNoTracking().FirstOrDefaultAsync(c => c.Id == schoolCourseId);
            if (course == null) return 0;
            var taken = await _context.SchoolReserves.CountAsync(r => r.SchoolCourseId == schoolCourseId && !r.IsCancel);
            return Math.Max(course.Capacity - taken, 0);
        }

        public async Task<BaseResultDto<SchoolReserveVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.SchoolReserves
                .Include(r => r.SchoolCourse).ThenInclude(c => c.Pet)
                .Include(r => r.SchoolCourse).ThenInclude(c => c.PetBreed)
                .Include(r => r.SchoolCourse).ThenInclude(c => c.SchoolCourseSessions)
                .Include(r => r.SchoolCourse).ThenInclude(c => c.School).ThenInclude(s => s.Companion)
                .Include(r => r.UserPet)
                .Include(r => r.Booker)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (item != null)
                return new BaseResultDto<SchoolReserveVDto>(true, mapper.Map<SchoolReserveVDto>(item));
            return new BaseResultDto<SchoolReserveVDto>(false, mapper.Map<SchoolReserveVDto>(item));
        }

        public SchoolReserveSearchDto Search(SchoolReserveInputDto baseSearchDto)
        {
            var model = _context.SchoolReserves
                .Include(r => r.SchoolCourse).ThenInclude(c => c.School).ThenInclude(s => s.Companion)
                .Include(r => r.Booker)
                .Include(r => r.UserPet)
                .AsQueryable();

            if (baseSearchDto.BookerId.HasValue)
                model = model.Where(r => r.BookerId == baseSearchDto.BookerId.Value);
            if (baseSearchDto.SchoolCourseId.HasValue)
                model = model.Where(r => r.SchoolCourseId == baseSearchDto.SchoolCourseId.Value);
            if (baseSearchDto.SchoolId.HasValue)
                model = model.Where(r => r.SchoolCourse.SchoolId == baseSearchDto.SchoolId.Value);
            if (baseSearchDto.CompanionId.HasValue)
                model = model.Where(r => r.SchoolCourse.School.CompanionId == baseSearchDto.CompanionId.Value);
            if (baseSearchDto.StatusId.HasValue)
                model = model.Where(r => r.StatusId == baseSearchDto.StatusId.Value);

            model = baseSearchDto.SortBy switch
            {
                Common.Enumerable.SortEnum.Old => model.OrderBy(r => r.Id),
                _ => model.OrderByDescending(r => r.Id),
            };

            return new SchoolReserveSearchDto(baseSearchDto, model, mapper);
        }

        // ثبت‌نام پت در یک دوره - ظرفیت داخل یک تراکنش Serializable چک می‌شود تا در صورت
        // درخواست هم‌زمان چند کاربر برای آخرین جای خالی، رزرو اضافه بر ظرفیت ثبت نشود.
        public override async Task<BaseResultDto<SchoolReserveDto>> InsertAsyncDto(SchoolReserveDto dto)
        {
            if (dto == null)
                return new BaseResultDto<SchoolReserveDto>(false, Resource.Notification.InvalidData, dto);

            await using var transaction = await _context.BeginTransactionAsync(IsolationLevel.Serializable);
            try
            {
                var course = await _context.SchoolCourses.Include(c => c.School)
                    .FirstOrDefaultAsync(c => c.Id == dto.SchoolCourseId && !c.Deleted && c.Active);
                if (course == null)
                {
                    return new BaseResultDto<SchoolReserveDto>(false, Resource.Notification.NothingFound, dto);
                }

                var ownedPet = await _context.UserPets.AnyAsync(p => p.Id == dto.UserPetId && p.UserId == dto.BookerId);
                if (!ownedPet)
                {
                    return new BaseResultDto<SchoolReserveDto>(false, Resource.Notification.InvalidData, dto);
                }

                var alreadyEnrolled = await _context.SchoolReserves.AnyAsync(r =>
                    r.SchoolCourseId == dto.SchoolCourseId && r.UserPetId == dto.UserPetId && !r.IsCancel);
                if (alreadyEnrolled)
                {
                    return new BaseResultDto<SchoolReserveDto>(false, Resource.Notification.HaveBeenReserved, dto);
                }

                var takenCount = await _context.SchoolReserves.CountAsync(r => r.SchoolCourseId == dto.SchoolCourseId && !r.IsCancel);
                if (takenCount >= course.Capacity)
                {
                    return new BaseResultDto<SchoolReserveDto>(false, Resource.Notification.SchoolCourseCapacityFull, dto);
                }

                var item = mapper.Map<SchoolReserve>(dto);
                item.Price = course.Price;
                item.PaymentPrice = course.Price;
                item.FromWallet = false;
                item.WalletPrice = 0;
                item.IsCancel = false;
                item.IsReserved = false;
                item.CreateDate = DateTime.Now;
                item.StatusId = (int)SchoolReserveStatusEnum.Registered;
                item.ReserveCode = PaymentCodeGenerator.Create(
                    PaymentCallbackTypeEnum.SchoolReserve,
                    item.CreateDate,
                    await _context.GetNextBusinessCodeNumberAsync());

                await _context.SchoolReserves.AddAsync(item);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new BaseResultDto<SchoolReserveDto>(true, mapper.Map<SchoolReserveDto>(item));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Enrolling pet {UserPetId} in school course {SchoolCourseId} failed.", dto.UserPetId, dto.SchoolCourseId);
                return new BaseResultDto<SchoolReserveDto>(false, Resource.Notification.Unsuccess, dto);
            }
        }

        // این متد همیشه از داخل PaymentService.ApplyAndMapPaymentAsync صدا زده می‌شود که خودش از
        // قبل یک تراکنش Serializable باز کرده - باز کردن یک تراکنش تو در تو روی همان DbContext
        // در حالت تست (پرداخت آنی درون همان درخواست) خطا می‌داد، پس فقط وقتی تراکنش فعالی از
        // بیرون نیامده، خودمان یکی باز می‌کنیم.
        public async Task<BaseResultDto> SchoolReservePaymentCallback(long? reserveId, bool fromWallet = false)
        {
            if (_context.CurrentTransaction != null)
                return await SchoolReservePaymentCallbackCoreAsync(reserveId, fromWallet);

            await using var transaction = await _context.BeginTransactionAsync(IsolationLevel.Serializable);
            var result = await SchoolReservePaymentCallbackCoreAsync(reserveId, fromWallet);
            if (result.IsSuccess)
                await transaction.CommitAsync();
            else
                await transaction.RollbackAsync();
            return result;
        }

        private async Task<BaseResultDto> SchoolReservePaymentCallbackCoreAsync(long? reserveId, bool fromWallet)
        {
            try
            {
                var reserve = await _context.SchoolReserves.Include(r => r.SchoolCourse).Include(r => r.Booker).Include(r => r.Rebate)
                    .AsTracking().FirstOrDefaultAsync(r => r.Id == reserveId);
                if (reserve == null)
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                if (reserve.IsReserved)
                    return new BaseResultDto(true);
                if (reserve.IsCancel)
                    return new BaseResultDto(false, Resource.Notification.InvalidData);

                if (fromWallet && reserve.FromWallet && reserve.WalletPrice > 0)
                {
                    var walletItem = new WalletDto()
                    {
                        Painding = false,
                        Amount = reserve.WalletPrice,
                        UserId = reserve.Booker.Id,
                        SchoolReserveId = reserve.Id
                    };
                    var walletResult = await _walletService.InsertUpdateSchoolReserveAsync(walletItem, true);
                    if (!walletResult.IsSuccess)
                        return new BaseResultDto(false);
                }
                if (reserve.Rebate != null)
                {
                    _rebateService.IncreaseUseCount(reserve.Rebate, reserve.BookerId, reserve.RebatePrice);
                }

                reserve.IsReserved = true;
                reserve.StatusId = (int)SchoolReserveStatusEnum.Paid;

                if (reserve.SchoolCourse != null)
                {
                    var commissionPercent = reserve.SchoolCourse.CommissionPercent;
                    var siteShare = (double)((decimal)reserve.PaymentPrice * commissionPercent / 100m);
                    reserve.SiteShare = siteShare;
                    reserve.CompanionShare = reserve.PaymentPrice - siteShare;
                }

                await _context.SaveChangesAsync();

                return new BaseResultDto(true, Resource.Notification.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "School reserve payment callback failed for reserve {ReserveId}.", reserveId);
                return new BaseResultDto(false);
            }
        }

        public async Task<BaseResultDto> SetRebateCodeAsyncDto(SchoolReserveRebateCodeDto dto)
        {
            var item = await _context.SchoolReserves.AsTracking().FirstOrDefaultAsync(s =>
                s.Id == dto.Id &&
                s.BookerId == _currentUser.CurrentUser.UserId &&
                s.StatusId == (int)SchoolReserveStatusEnum.Registered);

            if (item == null)
            {
                return new BaseResultDto<SchoolReserveRebateCodeDto>(false, Resource.Notification.NothingFound, dto);
            }
            if (string.IsNullOrEmpty(dto.RebateCode))
            {
                return new BaseResultDto(isSuccess: false, val: Resource.Notification.Unsuccess);
            }
            if (await HasActivePaymentAsync(item.Id))
                return new BaseResultDto(false, Resource.Notification.PansionReservePaymentAlreadyStartedCannotChangeFinancials);

            var originalPrice = item.PaymentPrice + item.RebatePrice;
            item.Price = originalPrice;
            var rebate = _rebateService.GetRebateByCodeAsync(originalPrice, item.BookerId, RebateTypeLabels.SchoolReserve, dto.RebateCode);
            if (rebate.IsSuccess)
            {
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
                _context.SchoolReserves.Update(item);
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
            var item = await _context.SchoolReserves.AsTracking().FirstOrDefaultAsync(s =>
                s.Id == id && s.BookerId == _currentUser.CurrentUser.UserId && !s.IsReserved);
            if (item == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (await HasActivePaymentAsync(item.Id))
                return new BaseResultDto(false, Resource.Notification.PansionReservePaymentAlreadyStartedCannotChangeFinancials);
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
            _context.SchoolReserves.Update(item);
            await _context.SaveChangesAsync();
            return new BaseResultDto(isSuccess: true, val: Resource.Notification.Success);
        }

        public async Task<BaseResultDto> SetWalletAsyncDto(SchoolReserveWalletDto dto)
        {
            var item = await _context.SchoolReserves.Include(s => s.Booker).AsTracking().FirstOrDefaultAsync(s =>
                s.Id == dto.Id && s.BookerId == _currentUser.CurrentUser.UserId);
            if (item == null)
            {
                return new BaseResultDto<SchoolReserveWalletDto>(false, Resource.Notification.NothingFound, dto);
            }
            if (await HasActivePaymentAsync(item.Id))
                return new BaseResultDto(false, Resource.Notification.PansionReservePaymentAlreadyStartedCannotChangeFinancials);
            if (item.IsReserved)
            {
                return new BaseResultDto<SchoolReserveWalletDto>(false, Resource.Notification.ThisReserveIsPaid, dto);
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
            _context.SchoolReserves.Update(item);
            await _context.SaveChangesAsync();
            return new BaseResultDto(isSuccess: true, val: Resource.Notification.Success);
        }

        private Task<bool> HasActivePaymentAsync(long reserveId)
        {
            var callbackId = reserveId.ToString();
            return _context.Payments.AsNoTracking().AnyAsync(s =>
                s.CallBackTypeLabel == PaymentCallbackTypeEnum.SchoolReserve.ToString() &&
                s.CallBackId == callbackId &&
                (s.IsSuccess == null || s.IsSuccess == true));
        }

        public async Task<BaseResultDto> UpdateCancelDto(SchoolReserveCancelDto dto)
        {
            var model = await _context.SchoolReserves.AsTracking().FirstOrDefaultAsync(s => s.Id == dto.Id && s.IsReserved);
            if (model == null)
                return new BaseResultDto(false, Resource.Notification.InvalidData);

            var isAdmin = _currentUser.CurrentUser.RoleEnum == RoleEnum.Admin.ToString() ||
                await _context.Roles
                    .Where(r => r.Id == _currentUser.CurrentUser.RoleId)
                    .SelectMany(r => r.Permissions)
                    .AnyAsync(p => !p.Deleted && p.Area == "Admin");
            var isOwner = model.BookerId == _currentUser.CurrentUser.UserId;

            if (!isAdmin && !isOwner)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);

            if (dto.IsCancel)
            {
                if (string.IsNullOrWhiteSpace(dto.CancelDetail))
                    return new BaseResultDto(false, Resource.Notification.PleaseEnterCancelDetail);

                model.IsCancel = true;
                model.CancelDetail = dto.CancelDetail;
                model.CancelDate = DateTime.Now;
                model.StatusId = (int)SchoolReserveStatusEnum.Cancelled;
            }

            await _context.SaveChangesAsync();
            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto> UpdateStatusDto(SchoolReserveStatusDto dto)
        {
            var model = await _context.SchoolReserves.AsTracking().FirstOrDefaultAsync(s => s.Id == dto.Id && s.IsReserved);
            if (model == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);

            model.StatusId = dto.StatusId;
            await _context.SaveChangesAsync();
            return new BaseResultDto(true);
        }

        public async Task SendClassStartingPushesAsync(CancellationToken cancellationToken)
        {
            var now = DateTime.Now;
            var windowEnd = now.AddMinutes(10);

            // فقط جلسات دوره‌های زنده (Live) که در ۱۰ دقیقه‌ی آینده شروع می‌شوند و هنوز پوششان نرفته.
            var dueSessions = await _context.SchoolCourseSessions
                .Include(s => s.SchoolCourse)
                .AsTracking()
                .Where(s =>
                    !s.Deleted &&
                    s.Active &&
                    s.StartingPushSentDate == null &&
                    s.SchoolCourse.CourseTypeId != (int)SchoolCourseTypeEnum.Video &&
                    s.SessionDate.Date == now.Date)
                .ToListAsync(cancellationToken);

            foreach (var session in dueSessions)
            {
                if (!TimeSpan.TryParse(session.StartTime, out var startTime))
                    continue;

                var sessionStart = session.SessionDate.Date + startTime;
                if (sessionStart < now || sessionStart > windowEnd)
                    continue;

                var enrolledBookerIds = await _context.SchoolReserves
                    .Where(r => r.SchoolCourseId == session.SchoolCourseId && !r.IsCancel)
                    .Select(r => r.BookerId)
                    .Distinct()
                    .ToListAsync(cancellationToken);

                foreach (var bookerId in enrolledBookerIds)
                {
                    try
                    {
                        await _pushNotificationService.SendPushAsync(
                            PushTypeEnum.PushSchoolClassStarting,
                            bookerId,
                            token1: session.SchoolCourse.Name,
                            token2: session.Id.ToString());
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send class-starting push for session {SessionId} to booker {BookerId}.", session.Id, bookerId);
                    }
                }

                session.StartingPushSentDate = now;
            }

            if (dueSessions.Count > 0)
                await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
