using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Common.Service;
using Application.Services.CommonSrv.SearchSrv.Dto;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.Filing.PictureSrv.Dto;
using Application.Services.ProductSrvs.ProductSrv.Iface;
using Application.Services.ProductSrvs.StoreSrv.Dto;
using Application.Services.StoreSrv.Iface;
using Application.Services.Setting.NoticeSrv;
using Application.Services.Setting.NoticeSrv.Dto;
using Application.Services.Setting.NoticeSrv.Iface;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Dapper;
using Entities.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Application.Services.ProductSrvs.StoreSrv
{
    public class StoreService : CommonSrv<Store, StoreDto>, IStoreService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly IProductService _productService;
        private readonly INoticeService _noticeService;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly ILogger<StoreService> _logger;
        private readonly string connectionString;

        public StoreService(IDataBaseContext _context, IMapper mapper, IConfiguration config, IProductService productService, INoticeService noticeService, IPushNotificationService pushNotificationService, ILogger<StoreService> logger) : base(_context: _context, mapper: mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this._productService = productService;
            this._noticeService = noticeService;
            this._pushNotificationService = pushNotificationService;
            this._logger = logger;
            this.connectionString = config.GetValue<string>(
            "connection");
        }
        public async Task<BaseResultDto<StoreVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.Stores
                .Include(s => s.City).ThenInclude(p => p.State)
                .Include(s => s.Picture)
                .Include(s => s.Icon)
                .Include(s => s.Type)
                .Include(s => s.Users)
                .FirstOrDefaultAsync(s => s.Id == id && s.Deleted == false); 
            if (item == null)
                return new BaseResultDto<StoreVDto>(false, null);
            return new BaseResultDto<StoreVDto>(true, mapper.Map<StoreVDto>(item));
        }

        public async Task<BaseResultDto> UpdateSiteVisibilityAsync(long id, bool showToSite)
        {
            var affectedRows = await _context.Stores
                .Where(x => x.Id == id && !x.Deleted)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.ShowToSite, showToSite));

            if (affectedRows == 0)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);

            return new BaseResultDto(true);
        }

        public StoreSearchDto Search(StoreInputDto baseSearchDto)
        {
            var model = _context.Stores.Include(s => s.City).ThenInclude(p => p.State).Include(s => s.Type).Include(s => s.Picture).Include(s => s.Users).Where(s => s.Deleted == false).AsQueryable();
            if (baseSearchDto.Available.HasValue)
            {
                model = model.Where(s => s.Active == baseSearchDto.Available);
            }
            if (baseSearchDto.ShowToSite.HasValue)
            {
                model = model.Where(s => s.ShowToSite == baseSearchDto.ShowToSite.Value);
            }
            if (baseSearchDto.UserId.HasValue)
            {
                model = model.Where(s => s.Users.Any(u => u.Id == baseSearchDto.UserId.Value));
            }
            if (baseSearchDto.Approved.HasValue)
            {
                model = model.Where(s => s.Approved == baseSearchDto.Approved.Value);
            }
            if (baseSearchDto.TypeId.HasValue)
            {
                model = model.Where(s => s.TypeId == baseSearchDto.TypeId);
            }
            if (baseSearchDto.CityId.HasValue)
            {
                model = model.Where(s => s.CityId == baseSearchDto.CityId);
            }
            if (baseSearchDto.StateId.HasValue)
            {
                model = model.Where(s => s.City.StateId == baseSearchDto.StateId);
            }
            if (!string.IsNullOrEmpty(baseSearchDto.Q))
            {
                model = model.Where(s => s.Name.Contains(baseSearchDto.Q)).OrderByDescending(o => o.Id);
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

                case Common.Enumerable.SortEnum.MoreSell:
                    {
                        model = model.OrderByDescending(s => s.RateAvg).ThenByDescending(ad => ad.Id);
                        break;
                    }
                case Common.Enumerable.SortEnum.LessSell:
                    {
                        model = model.OrderBy(s => s.RateAvg).ThenByDescending(ad => ad.Id);
                        break;
                    }
                default:
                    break;
            }

            return new StoreSearchDto(baseSearchDto, model, mapper);
        }

        public override async Task<BaseResultDto<StoreDto>> InsertAsyncDto(StoreDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<StoreDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    if ((!NameIsUnique(dto.Name)))
                    {
                        return new BaseResultDto<StoreDto>(isSuccess: false, val1: Resource.Notification.TheNameIsDuplicate, val2: nameof(dto.Name), dto);
                    }
                    var item = mapper.Map<Store>(dto);
                    item.CreateDate = DateTime.Now;
                    item.ReferralCode = await ReferralCodeGenerator.CreateStoreCodeAsync(_context);
                    await _context.Stores.AddAsync(item);
                    _context.SaveChanges();
                    return new BaseResultDto<StoreDto>(true, mapper.Map<StoreDto>(item));
                }

            }
            catch (Exception ex)
            {
                return new BaseResultDto<StoreDto>(isSuccess: false, val: ex.Message, data: dto);
            }


        }

        public async Task<BaseResultDto<StoreVDto>> FindRequestAsync(long id, long userId)
        {
            var item = await _context.Stores
                .AsNoTracking()
                .Include(s => s.City).ThenInclude(s => s.State)
                .Include(s => s.Picture)
                .Include(s => s.Icon)
                .Include(s => s.Type)
                .Include(s => s.Users)
                .FirstOrDefaultAsync(s =>
                    s.Id == id &&
                    !s.Deleted &&
                    s.Users.Any(u => u.Id == userId));

            return new BaseResultDto<StoreVDto>(item != null, mapper.Map<StoreVDto>(item));
        }

        public async Task<BaseResultDto<StoreDto>> InsertRequestAsync(StoreDto dto, long userId)
        {
            var checker = await ValidateRequestAsync(dto);
            if (!checker.IsSuccess)
                return new BaseResultDto<StoreDto>(false, checker.Messages, dto);

            if (!NameIsUnique(dto.Name))
                return new BaseResultDto<StoreDto>(false, Resource.Notification.TheNameIsDuplicate, dto);

            var user = await _context.Users
                .AsTracking()
                .FirstOrDefaultAsync(s => s.Id == userId && !s.Deleted);
            if (user == null)
                return new BaseResultDto<StoreDto>(false, Resource.Notification.NothingFound, dto);

            if (await _context.Stores.AnyAsync(s => !s.Deleted && s.Users.Any(u => u.Id == userId)))
                return new BaseResultDto<StoreDto>(false, Resource.Notification.StoreRequestAlreadyExistsForUser, dto);

            var item = mapper.Map<Store>(dto);
            item.Active = false;
            item.Approved = false;
            item.ApprovalValue = null;
            item.ShowToSite = false;
            item.Deleted = false;
            item.CreateDate = DateTime.Now;
            item.MaxDiscountPercent = 0;
            item.RateAvg = 0;
            item.RateCount = 0;
            item.CommentCount = 0;
            item.CommissionPercent = 0;
            item.ReferralCode = await ReferralCodeGenerator.CreateStoreCodeAsync(_context);
            item.Users = new List<Entities.Entities.Security.User> { user };

            await _context.Stores.AddAsync(item);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Creating store request for user {UserId} failed.", userId);
                return new BaseResultDto<StoreDto>(false, Resource.Notification.StoreDataConflictsWithExistingData, dto);
            }
            await TryCreateNoticeAsync(item, NoticeTypeLabels.StoreSubmitted, userId);

            return new BaseResultDto<StoreDto>(true, mapper.Map<StoreDto>(item));
        }

        public async Task<BaseResultDto> ResubmitRequestAsync(StoreDto dto, long userId)
        {
            if (dto == null || dto.Id <= 0)
                return new BaseResultDto(false, Resource.Notification.StoreRequestIdIsInvalid);

            var checker = await ValidateRequestAsync(dto);
            if (!checker.IsSuccess)
                return checker;

            var item = await _context.Stores
                .Include(s => s.Users)
                .AsTracking()
                .FirstOrDefaultAsync(s =>
                    s.Id == dto.Id &&
                    !s.Deleted &&
                    s.Users.Any(u => u.Id == userId));
            if (item == null)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);

            if (item.Approved)
                return new BaseResultDto(false, Resource.Notification.StoreRequestAlreadyApprovedCannotResubmit);

            if (await _context.Stores.AnyAsync(s => s.Id != item.Id && !s.Deleted && s.Name == dto.Name))
                return new BaseResultDto(false, Resource.Notification.TheNameIsDuplicate);

            var referralCode = item.ReferralCode;
            var createDate = item.CreateDate;
            var users = item.Users;
            var maxDiscountPercent = item.MaxDiscountPercent;
            var rateAvg = item.RateAvg;
            var rateCount = item.RateCount;
            var commentCount = item.CommentCount;
            var commissionPercent = item.CommissionPercent;
            mapper.Map(dto, item);
            item.ReferralCode = referralCode;
            item.CreateDate = createDate;
            item.Users = users;
            item.MaxDiscountPercent = maxDiscountPercent;
            item.RateAvg = rateAvg;
            item.RateCount = rateCount;
            item.CommentCount = commentCount;
            item.CommissionPercent = commissionPercent;
            item.Active = false;
            item.Approved = false;
            item.ApprovalValue = null;
            item.ShowToSite = false;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Resubmitting store request {StoreId} failed.", item.Id);
                return new BaseResultDto(false, Resource.Notification.StoreDataConflictsWithExistingData);
            }
            await TryCreateNoticeAsync(item, NoticeTypeLabels.StoreUpdated, userId);

            return new BaseResultDto(true);
        }

        private async Task<BaseResultDto> ValidateRequestAsync(StoreDto dto)
        {
            var errors = new List<Tuple<string, string>>();
            if (dto == null)
                return new BaseResultDto(false, Resource.Notification.InvalidData);

            dto.Name = dto.Name?.Trim();
            dto.Phone = dto.Phone?.Trim();
            dto.Mobile = dto.Mobile?.Trim();
            dto.Address = dto.Address?.Trim();

            if (string.IsNullOrWhiteSpace(dto.Name))
                errors.Add(Tuple.Create(Resource.Notification.PleaseEnterTheName, nameof(dto.Name)));
            if (string.IsNullOrWhiteSpace(dto.Phone) && string.IsNullOrWhiteSpace(dto.Mobile))
                errors.Add(Tuple.Create(Resource.Notification.StorePhoneOrMobileRequired, nameof(dto.Mobile)));
            if (!string.IsNullOrWhiteSpace(dto.Mobile) && !Regex.IsMatch(dto.Mobile, @"^\+?\d{10,13}$"))
                errors.Add(Tuple.Create(Resource.Notification.StoreMobileIsInvalid, nameof(dto.Mobile)));
            if (string.IsNullOrWhiteSpace(dto.Address))
                errors.Add(Tuple.Create(Resource.Notification.StoreAddressRequired, nameof(dto.Address)));
            if (dto.TypeId <= 0 || !await _context.Codes.AnyAsync(s =>
                    s.Id == dto.TypeId &&
                    s.Active &&
                    s.CodeGroup.Label == CodeGroupEnum.Store_Type.ToString()))
                errors.Add(Tuple.Create(Resource.Notification.StoreTypeIsInvalid, nameof(dto.TypeId)));
            if (dto.CityId <= 0 || !await _context.Cities.AnyAsync(s => s.Id == dto.CityId))
                errors.Add(Tuple.Create(Resource.Notification.StoreCityIsInvalid, nameof(dto.CityId)));
            if (dto.PictureId.HasValue && dto.PictureId.Value > 0 &&
                !await _context.Pictures.AnyAsync(s => s.Id == dto.PictureId.Value))
                errors.Add(Tuple.Create(Resource.Notification.StorePictureIsInvalid, nameof(dto.PictureId)));
            if (dto.IconId.HasValue && dto.IconId.Value > 0 &&
                !await _context.Pictures.AnyAsync(s => s.Id == dto.IconId.Value))
                errors.Add(Tuple.Create(Resource.Notification.StoreIconIsInvalid, nameof(dto.IconId)));
            if (dto.Location != null &&
                (dto.Location.x < -180 || dto.Location.x > 180 || dto.Location.y < -90 || dto.Location.y > 90))
                errors.Add(Tuple.Create(Resource.Notification.StoreMapCoordinatesAreInvalid, nameof(dto.Location)));

            return errors.Any()
                ? new BaseResultDto(false, errors)
                : new BaseResultDto(true);
        }

        public async Task<BaseResultDto> UpdateApprovalAsync(StoreApprovalDto dto)
        {
            if (!dto.Approved && string.IsNullOrWhiteSpace(dto.ApprovalValue))
                return new BaseResultDto(false, Resource.Notification.PleaseEnterTheActivationValueReason);

            var item = await _context.Stores
                .Include(s => s.Users)
                .AsTracking()
                .FirstOrDefaultAsync(s => s.Id == dto.Id && !s.Deleted);
            if (item == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);

            var owner = item.Users.OrderBy(s => s.Id).FirstOrDefault();
            if (owner == null)
                return new BaseResultDto(false, Resource.Notification.StoreOwnerNotSpecified);

            item.Approved = dto.Approved;
            item.Active = dto.Approved;
            item.ApprovalValue = dto.Approved ? null : dto.ApprovalValue.Trim();
            item.ShowToSite = dto.Approved && item.ShowToSite;
            await _context.SaveChangesAsync();

            try
            {
                await _pushNotificationService.SendPushAsync(
                    dto.Approved ? PushTypeEnum.PushStoreRequestApproved : PushTypeEnum.PushStoreRequestRejected,
                    owner.Id,
                    token1: item.Name,
                    token2: item.ApprovalValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sending store decision push for {StoreId} failed.", item.Id);
            }

            return new BaseResultDto(true);
        }

        private async Task TryCreateNoticeAsync(Store item, string label, long userId)
        {
            try
            {
                await _noticeService.CreateAsync(new NoticeCreateDto
                {
                    Label = label,
                    ActorUserId = userId,
                    ReferenceType = "Store",
                    ReferenceId = item.Id,
                    DeduplicationKey = $"{label}:{item.Id}:{DateTime.UtcNow.Ticks}",
                    Metadata = new Dictionary<string, string> { { "storeName", item.Name } }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Creating admin notice {NoticeLabel} for store {StoreId} failed.", label, item.Id);
            }
        }

        public override BaseResultDto UpdateDto(StoreDto dto)
        {

            try
            {
                var modelCheker = ModelHelper<StoreDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    var item = _context.Stores.FirstOrDefault(s => s.Id == dto.Id);
                    if (dto.Name != item.Name && (!NameIsUnique(dto.Name)))
                    {
                        return new BaseResultDto<StoreDto>(isSuccess: false, val1: Resource.Notification.TheNameIsDuplicate, val2: nameof(dto.Name), dto);
                    }
                    bool updateProducts = false;
                    if (dto.Active != item.Active)
                    {
                        updateProducts = true;
                    }
                    mapper.Map(dto, item);
                    _context.Stores.Update(item);
                    _context.SaveChanges();
                    if (updateProducts)
                    {
                        _productService.UpdateProductPriceAsync(Common.Enumerable.ProductUpdateTypeEnum.Store, dto.Id.ToString());
                    }
                    return new BaseResultDto<StoreDto>(true, mapper.Map<StoreDto>(item));
                }
            }
            catch (Exception ex)
            {
                return new BaseResultDto(isSuccess: false, val: ex.Message);
            }

        }
        public override BaseResultDto DeleteDto(long id)
        {

            var del = base.DeleteDto(id);
            _productService.UpdateProductPriceAsync(Common.Enumerable.ProductUpdateTypeEnum.Store, id.ToString());
            return del;
        }
        bool NameIsUnique(string name)
        {
            var item = _context.Stores.FirstOrDefault(x => x.Name == name);
            if (item == null)
                return true;
            return false;
        }
        public async Task SetMaxDiscountAsync(long storeId, int maxDiscount)
        {
            var item = await _context.Stores.AsTracking().FirstOrDefaultAsync(s => s.Id == storeId);
            if (item == null) return;
            item.MaxDiscountPercent = maxDiscount;
            _context.Stores.Update(item);
            await _context.SaveChangesAsync();
        }
        public void UpdateStoreCommentCount(long storeId)
        {
            var item = _context.Stores.Include(s => s.StoreComments).ThenInclude(s => s.Status).AsTracking().FirstOrDefault(s => s.Id == storeId);
            item.CommentCount = item.StoreComments.Count(c => c.Status.Label == CommentEnum.Comment_Accept.ToString());
            _context.Stores.Update(item);
            _context.SaveChanges();
        }
        public async Task UpdateStoreCommentRateAsync(long Id)
        {
            var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("UpdateStoreCommentsRate", new { FilterIds = Id }, commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<List<SearchStoreDto>> SearchMinAsync(SearchRequestDto request)
        {
            var predicate = SearchQueryHelper.ContainsAny<Store>(request.SearchTerms, item => item.Name, item => item.Address, item => item.City.Name);
            var query = _context.Stores.Where(s => !s.Deleted && s.Active);
            return await query.Where(predicate).OrderByDescending(s => s.RateAvg)
                .Take(SearchQueryHelper.CandidateCount(request.StoreCount))
                .ProjectTo<SearchStoreDto>(mapper.ConfigurationProvider).ToListAsync();
        }

    }

}
