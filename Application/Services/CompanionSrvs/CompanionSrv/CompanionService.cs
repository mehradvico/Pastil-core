using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Common.Service;
using Application.Services.Accounting.UserSrv.Iface;
using Application.Services.CommonSrv.SearchSrv.Dto;
using Application.Services.CompanionSrvs.CompanionSrv.Dto;
using Application.Services.CompanionSrvs.CompanionSrv.Iface;
using Application.Services.Filing.PictureSrv.Dto;
using Application.Services.Setting.CodeSrv.Iface;
using Application.Services.Setting.NoticeSrv.Iface;
using AutoMapper;
using Dapper;
using DocumentFormat.OpenXml.Office2010.Excel;
using Entities.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.CompanionSrv
{
    public class CompanionService : CommonSrv<Companion, CompanionDto>, ICompanionService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly IUserService _userService;
        private readonly ICodeService _codeService;
        private readonly INoticeService _notificationService;
        private readonly string connectionString;
        public CompanionService(IDataBaseContext _context, IMapper mapper, IConfiguration config, IUserService userService, INoticeService notificationService, ICodeService codeService) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this._userService = userService;
            this._codeService = codeService;
            this._notificationService = notificationService;
            this.connectionString = config.GetValue<string>("connection");
        }

        public async Task<BaseResultDto<CompanionVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.Companions.Include(s => s.Picture).Include(s => s.CompanionPets).Include(s => s.BackgroundPicture).Include(s => s.City).ThenInclude(s => s.State)
                .Include(s => s.Neighborhood).Include(s => s.Owner).Include(s => s.CompanionTypes).Include(s => s.CompanionZones).ThenInclude(s => s.City).ThenInclude(s => s.State)
                .Include(s => s.CompanionZones).ThenInclude(s => s.Neighborhood).Include(s => s.Pansions).FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);
            if (item != null)
            {
                return new BaseResultDto<CompanionVDto>(true, mapper.Map<CompanionVDto>(item));
            }
            return new BaseResultDto<CompanionVDto>(false, mapper.Map<CompanionVDto>(item));
        }

        public override async Task<BaseResultDto<CompanionDto>> FindAsyncDto(long id)
        {
            var item = await _context.Companions.Include(s => s.Picture).Include(s => s.CompanionPets).Include(s => s.BackgroundPicture).Include(s => s.City).ThenInclude(s => s.State)
                .Include(s => s.Neighborhood).Include(s => s.Owner).Include(s => s.CompanionTypes).Include(s => s.CompanionZones).ThenInclude(s => s.City).ThenInclude(s => s.State)
                .Include(s => s.CompanionZones).ThenInclude(s => s.Neighborhood).Include(s => s.Pansions).FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);
            if (item != null)
            {
                return new BaseResultDto<CompanionDto>(true, mapper.Map<CompanionDto>(item));
            }
            return new BaseResultDto<CompanionDto>(false, mapper.Map<CompanionDto>(item));
        }
        public CompanionSearchDto Search(CompanionInputDto baseSearchDto)
        {
            var model = _context.Companions.Include(s => s.CompanionAssistances).Include(s => s.Owner).Include(s => s.Picture).Include(s => s.City).ThenInclude(s => s.State)
                .Include(s => s.Neighborhood).Include(s => s.CompanionTypes).Include(s => s.CompanionPets).Include(s => s.Neighborhood).Include(s => s.Owner).Include(s => s.CompanionTypes)
                .Include(s => s.CompanionZones).ThenInclude(s => s.City).ThenInclude(s => s.State).AsQueryable().Where(s => !s.Deleted);

            if (baseSearchDto.Available.HasValue)
            {
                model = model.Where(s => s.Active == baseSearchDto.Available.Value);
            }
            if (!string.IsNullOrEmpty(baseSearchDto.Q))
            {
                model = model.Where(s => s.Name.Contains(baseSearchDto.Q) || s.Phone.Equals(baseSearchDto.Q));
            }
            if (!string.IsNullOrEmpty(baseSearchDto.Q))
            {
                model = model.Where(s => s.Name == baseSearchDto.Q || s.Phone == baseSearchDto.Q);
            }
            if (baseSearchDto.AssistanceId.HasValue)
            {
                model = model.Where(s => s.CompanionAssistances.Any(ca => ca.AssistanceId == baseSearchDto.AssistanceId.Value));
            }
            if (baseSearchDto.NeighborhoodIds != null && baseSearchDto.NeighborhoodIds.Any())
            {
                model = model.Where(s => s.Neighborhood != null && baseSearchDto.NeighborhoodIds.Contains(s.Neighborhood.Id));
            }

            if (baseSearchDto.IsPersonal.HasValue)
            {
                model = model.Where(s => s.IsPersonal == baseSearchDto.IsPersonal.Value);
            }
            if (baseSearchDto.CityId.HasValue)
            {
                model = model.Where(s => s.City.Id == baseSearchDto.CityId.Value);
            }
            if (baseSearchDto.StateId.HasValue)
            {
                model = model.Where(s => s.City.State.Id == baseSearchDto.StateId.Value);
            }
            if (baseSearchDto.Approved.HasValue)
            {
                model = model.Where(s => s.Approved == baseSearchDto.Approved.Value);
            }
            if (baseSearchDto.OwnerId.HasValue)
            {
                model = model.Where(s => s.Owner.Id == baseSearchDto.OwnerId.Value);
            }
            if (baseSearchDto.TypeId.HasValue)
            {
                model = model.Where(s => s.CompanionTypes.Any(p => p.TypeId == baseSearchDto.TypeId && !p.Deleted));
            }
            if (baseSearchDto.PetId.HasValue)
            {
                model = model.Where(s => s.CompanionPets.Any(p => p.PetId == baseSearchDto.PetId));
            }
            if (baseSearchDto.GoldAccount.HasValue && baseSearchDto.GoldAccount.Value &&
                baseSearchDto.SilverAccount.HasValue && baseSearchDto.SilverAccount.Value)
            {
                model = model.Where(s =>
                    (s.GoldAccountDate.HasValue && s.GoldAccountDate >= DateTime.Now) ||
                    (s.SilverAccountDate.HasValue && s.SilverAccountDate >= DateTime.Now)
                );
            }
            else if (baseSearchDto.GoldAccount.HasValue && baseSearchDto.GoldAccount.Value)
            {
                model = model.Where(s => s.GoldAccountDate.HasValue && s.GoldAccountDate >= DateTime.Now);
            }
            else if (baseSearchDto.SilverAccount.HasValue && baseSearchDto.SilverAccount.Value)
            {
                model = model.Where(s => s.SilverAccountDate.HasValue && s.SilverAccountDate >= DateTime.Now);
            }
            model = model.OrderByDescending(s => s.GoldAccountDate.HasValue).ThenByDescending(s => s.SilverAccountDate.HasValue);
            if (baseSearchDto.AssistanceType.HasValue)
            {
                var label = baseSearchDto.AssistanceType.ToString(); model = model.Where(s => s.CompanionAssistances.Any(ca => ca.Codes.Any(code => code.Label == label)));
            }
            if (baseSearchDto.HasInsurance.HasValue)
            {
                if (baseSearchDto.HasInsurance.Value)
                {
                    model = model.Where(s => s.CompanionInsurancePackages.Any(p => !p.Deleted && p.Active));
                }
                else
                {
                    model = model.Where(s => !s.CompanionInsurancePackages.Any(p => !p.Deleted && p.Active));
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
                case Common.Enumerable.SortEnum.MorePriority:
                    {
                        var now = DateTime.Now;
                        model = model.OrderBy(ad =>
        ad.GoldAccountDate != null && ad.GoldAccountDate > now ? 0 :
        ad.SilverAccountDate != null && ad.SilverAccountDate > now ? 1 : 2)
    .ThenByDescending(ad => ad.SilverAccountCreateDate).ThenByDescending(ad => ad.Id);
                        break;
                    }
                case Common.Enumerable.SortEnum.MoreSell:
                    {
                        var now = DateTime.Now;
                        model = model.OrderByDescending(s => s.RateAvg).ThenBy(ad =>
        ad.GoldAccountDate != null && ad.GoldAccountDate > now ? 0 :
        ad.SilverAccountDate != null && ad.SilverAccountDate > now ? 1 : 2)
    .ThenByDescending(ad => ad.SilverAccountCreateDate).ThenByDescending(ad => ad.Id);
                        break;
                    }
                case Common.Enumerable.SortEnum.LessSell:
                    {
                        var now = DateTime.Now;
                        model = model.OrderBy(s => s.RateAvg).ThenBy(ad =>
        ad.GoldAccountDate != null && ad.GoldAccountDate > now ? 0 :
        ad.SilverAccountDate != null && ad.SilverAccountDate > now ? 1 : 2)
    .ThenByDescending(ad => ad.SilverAccountCreateDate).ThenByDescending(ad => ad.Id);
                        break;
                    }
                default:
                    break;
            }
            return new CompanionSearchDto(baseSearchDto, model, mapper);
        }
        public override async Task<BaseResultDto<CompanionDto>> InsertAsyncDto(CompanionDto dto)
        {
            try
            {
                var modelCheker = await InsertCheckerAsync(dto);
                if (!modelCheker.IsSuccess)
                {
                    return new BaseResultDto<CompanionDto>(false, dto);
                }
                else
                {
                    var item = mapper.Map<Companion>(dto);
                    var ownerId = dto.OwnerId;
                    var model = await _context.Users.Include(s => s.Companions).FirstOrDefaultAsync(s => s.Id == ownerId && !s.Deleted);
                    if (model == null)
                    {
                        return new BaseResultDto<CompanionDto>(false, Resource.Notification.NothingFound, dto);
                    }
                    bool existed = await _context.Companions.AnyAsync(x => x.OwnerId == dto.OwnerId && !x.Deleted);
                    if (existed)
                    {
                        return new BaseResultDto<CompanionDto>(false, Resource.Notification.AlreadyIsCompanion, dto);
                    }
                    await _context.Companions.AddAsync(item);
                    await _context.SaveChangesAsync();
                    var companionId = item.Id;
                    await _context.SaveChangesAsync();
                    await _notificationService.InsertNoticeAsync(item.Id, NoticeTypeEnum.NotifType_AddCompanion, NoticeUserTypeEnum.NoticeUserType_Admin);
                    return new BaseResultDto<CompanionDto>(true, mapper.Map<CompanionDto>(item));

                }
            }
            catch (Exception ex)
            {
                return new BaseResultDto<CompanionDto>(isSuccess: false, val: ex.Message, data: dto);
            }
        }
        public async Task<BaseResultDto> InsertCheckerAsync(CompanionDto dto)
        {
            dto.Phone = await dto.Phone?.Trim().ToEnglishDigitsAsync();
            var errors = new List<Tuple<string, string>>();

            if (string.IsNullOrEmpty(dto.Name))
            {
                errors.Add(new Tuple<string, string>(Resource.Notification.PleaseEnterTheName, nameof(dto.Name)));
            }
            if (string.IsNullOrEmpty(dto.Phone))
            {
                errors.Add(new Tuple<string, string>(Resource.Notification.PleaseEnterThePhone, nameof(dto.Phone)));

            }
            if (errors.Any())
            {
                return new BaseResultDto(isSuccess: false, messages: errors);
            }
            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto> UpdateGoldAccountDto(CompanionGoldAccountDto dto)
        {
            var model = await _context.Companions
                .FirstOrDefaultAsync(s => s.Id == dto.Id && !s.Deleted);

            if (model == null)
            {
                return new BaseResultDto<CompanionGoldAccountDto>(false, mapper.Map<CompanionGoldAccountDto>(model));
            }

            bool isAlreadyGold = model.GoldAccountDate.HasValue;
            model.GoldAccountDate = dto.GoldAccountDate;

            if (!isAlreadyGold)
            {
                var previousGold = await _context.Companions.Where(s => s.GoldAccountDate.HasValue && s.Id != dto.Id && !s.Deleted).FirstOrDefaultAsync();
                if (previousGold != null)
                {
                    previousGold.GoldAccountDate = null;
                    _context.Companions.Update(previousGold);
                }
            }

            _context.Companions.Update(model);
            await _context.SaveChangesAsync();

            return new BaseResultDto<CompanionGoldAccountDto>(true, mapper.Map<CompanionGoldAccountDto>(model));
        }

        public BaseResultDto UpdateSilverAccountDto(CompanionSilverAccountDto dto)
        {
            var model = _context.Companions.FirstOrDefault(s => s.Id == dto.Id && !s.Deleted);
            if (model == null)
            {
                return new BaseResultDto<CompanionSilverAccountDto>(false, mapper.Map<CompanionSilverAccountDto>(model));
            }
            else
            {
                if (dto.SilverAccountDate != model.SilverAccountDate)
                    model.SilverAccountCreateDate = DateTime.Now;

                model.SilverAccountDate = dto.SilverAccountDate;

                if (model.SilverAccountDate == null)
                    model.SilverAccountCreateDate = null;
            }
            _context.Companions.Update(model);
            _context.SaveChanges();
            return new BaseResultDto<CompanionSilverAccountDto>(true, mapper.Map<CompanionSilverAccountDto>(model));
        }

        public async Task<BaseResultDto> UpdateAsyncDto(CompanionDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<CompanionDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    var res = UpdateDto(dto);
                    if (res.IsSuccess)
                    {
                        await _notificationService.InsertNoticeAsync(dto.Id, NoticeTypeEnum.NotifType_EditCompanion, NoticeUserTypeEnum.NoticeUserType_Admin);

                    }
                    return res;
                }
            }
            catch (Exception ex)
            {
                return new BaseResultDto(isSuccess: false, val: ex.Message);
            }
        }

        public BaseResultDto ActivationDto(CompanionActivationDto dto)
        {
            var item = _context.Companions.FirstOrDefault(s => s.Id == dto.Id && !s.Deleted);
            if (!dto.Active && !dto.Approved)
            {
                item.Active = false;
                item.Approved = false;
                item.ActivationValue = dto.ActivationValue;

                if (!item.Active && string.IsNullOrEmpty(dto.ActivationValue))
                {
                    return new BaseResultDto(false, Resource.Notification.PleaseEnterTheActivationValueReason);
                }
            }
            else if (dto.Active && !dto.Approved)
            {
                item.Active = true;
                item.Approved = false;
                item.ActivationValue = dto.ActivationValue;

                if (!item.Approved && (string.IsNullOrWhiteSpace(dto.ActivationValue)))
                {
                    return new BaseResultDto(false, Resource.Notification.PleaseEnterTheActivationValueReason);
                }
            }
            else
            {
                item.Active = true;
                item.Approved = true;
                item.ActivationValue = dto.ActivationValue;
            }
            _context.Companions.Update(item);
            _context.SaveChanges();
            return new BaseResultDto(isSuccess: true);
        }

        public BaseResultDto CompanionShareDto(CompanionShareDto dto)
        {
            var item = _context.Companions.FirstOrDefault(s => s.Id == dto.Id && !s.Deleted);
            item.SharePercent = dto.SharePercent;
            _context.Companions.Update(item);
            _context.SaveChanges();
            return new BaseResultDto(isSuccess: true);
        }

        public async Task<List<SearchCompanionDto>> SearchMinAsync(SearchRequestDto request)
        {
            var list = await _context.Companions.Where(s => s.Deleted == false && s.Active && s.SearchKey.Contains(request.Q) && (s.Name.Contains(request.Q) || s.CompanionAssistances.Any(a => a.Active && a.Approved && a.Assistance.Deleted == false && a.Assistance.Active && a.Assistance.Name.Contains(request.Q)))).Take(request.CompanionCount).Select(s => new SearchCompanionDto { Id = s.Id, Name = s.Name, RateAvg = s.RateAvg, RateCount = s.RateCount, IconId = s.IconId, Icon = mapper.Map<PictureVDto>(s.Icon) }).ToListAsync();
            return list;
        }

        public void UpdateCompanionCommentCount(long CompanionId)
        {
            var item = _context.Companions.Include(s => s.CompanionComments).ThenInclude(s => s.Status).AsTracking().FirstOrDefault(s => s.Id == CompanionId);
            item.CommentCount = item.CompanionComments.Count(c => c.Status.Label == CommentEnum.Comment_Accept.ToString());
            _context.Companions.Update(item);
            _context.SaveChanges();
        }
        public async Task UpdateCompanionCommentRateAsync(long Id)
        {
            var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("UpdateCompanionCommentsRate", new { FilterIds = Id }, commandType: System.Data.CommandType.StoredProcedure);
        }
    }
}
