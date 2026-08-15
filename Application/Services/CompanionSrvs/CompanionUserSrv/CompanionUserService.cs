using AngleSharp.Dom;
using Application.Common.Dto.Result;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Common.Service;
using Application.Services.Accounting.UserSrv.Iface;
using Application.Services.CompanionSrvs.CompanionUserSrv.Dto;
using Application.Services.CompanionSrvs.CompanionUserSrv.Iface;
using Application.Services.Setting.CodeSrv.Iface;
using Application.Services.Setting.NoticeSrv.Iface;
using Application.Services.Setting.NoticeSrv;
using Application.Services.Setting.NoticeSrv.Dto;
using AutoMapper;
using DocumentFormat.OpenXml.Office.CustomUI;
using Entities.Entities;
using Entities.Entities.CompanionField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Application.Services.CompanionSrvs.CompanionUserSrv
{
    public class CompanionUserService : CommonSrv<CompanionUser, CompanionUserDto>, ICompanionUserService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ICodeService _codeService;
        private readonly INoticeService _notificationService;
        private readonly IUserService _userService;
        public CompanionUserService(IDataBaseContext _context, IMapper mapper, ICodeService codeService, INoticeService notificationService, IUserService userService) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this._codeService = codeService;
            this._notificationService = notificationService;
            this._userService = userService;
        }

        public override async Task<BaseResultDto<CompanionUserDto>> FindAsyncDto(long id)
        {
            var item = await _context.CompanionUsers.Include(p => p.User).ThenInclude(s => s.Picture).Include(s => s.Companion).Include(s => s.Expertise).FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);
            if (item != null)
                return new BaseResultDto<CompanionUserDto>(true, mapper.Map<CompanionUserDto>(item));
            return new BaseResultDto<CompanionUserDto>(false, mapper.Map<CompanionUserDto>(item));
        }

        public CompanionUserSearchDto SearchDto(CompanionUserInputDto dto)
        {
            var model = _context.CompanionUsers.Include(s => s.User).ThenInclude(s => s.Picture).Include(s => s.Companion).Include(s => s.Expertise).AsQueryable().Where(s => !s.Deleted);

            return Search(dto, model);
        }

        public CompanionUserSearchDto SearchPublic(CompanionUserInputDto dto)
        {
            var model = _context.CompanionUsers
                .AsNoTracking()
                .Include(s => s.User)
                    .ThenInclude(s => s.Picture)
                .Include(s => s.Companion)
                .Include(s => s.Expertise)
                .Where(s =>
                    !s.Deleted &&
                    s.Active &&
                    s.UserAccept == true &&
                    !s.User.Deleted &&
                    !s.Companion.Deleted &&
                    s.Companion.Active &&
                    s.Companion.Approved);

            dto.Active = true;
            dto.UserAccept = true;
            dto.AllUserAccept = false;

            return Search(dto, model);
        }

        private CompanionUserSearchDto Search(
            CompanionUserInputDto dto,
            IQueryable<CompanionUser> model)
        {
            if (dto.Active.HasValue)
            {
                model = model.Where(s => s.Active.Equals(dto.Active));
            }
            if (dto.UserId.HasValue)
            {
                model = model.Where(s => s.UserId == dto.UserId);
            }
            if (!dto.AllUserAccept && dto.UserAccept.HasValue)
            {
                model = model.Where(s => s.UserAccept == dto.UserAccept.Value);
            }
            if (dto.CompanionId.HasValue)
            {
                model = model.Where(s => s.CompanionId == dto.CompanionId);
            }
            if (!string.IsNullOrEmpty(dto.Q))
            {
                model = model.Where(s => s.User.LastName.Contains(dto.Q) || s.User.FirstName.Contains(dto.Q) || s.User.Mobile.Contains(dto.Q));
            }

            return new CompanionUserSearchDto(dto, model, mapper);

        }
        public void InsertOrUpdate(CompanionUserDto CompanionUser)
        {
            var item = _context.CompanionUsers.FirstOrDefault(s => s.CompanionId == CompanionUser.CompanionId && s.UserId == CompanionUser.UserId);
            if (item == null)

            {
                item = mapper.Map<CompanionUser>(CompanionUser);
                _context.CompanionUsers.Add(item);
            }
            _context.SaveChanges();
        }

        public void InsertOrUpdate(Companion companion, List<CompanionUserDto> CompanionUsersDto)
        {
            if (companion.CompanionUsers != null)
            {
                _context.CompanionUsers.RemoveRange(companion.CompanionUsers);
                _context.SaveChanges();
            }
            else
            {
                companion.CompanionUsers = new List<CompanionUser>();
            }
            CompanionUsersDto.ForEach(s => s.CompanionId = companion.Id);
            foreach (var item in CompanionUsersDto)
            {
                InsertOrUpdate(item);
            }
        }

        public override async Task<BaseResultDto<CompanionUserDto>> InsertAsyncDto(CompanionUserDto dto)
        {
            try
            {
                if (!await IsValidExpertiseAsync(dto.ExpertiseId))
                {
                    return new BaseResultDto<CompanionUserDto>(
                        false,
                        "لطفاً یک عنوان شغلی فعال و معتبر انتخاب کنید.",
                        nameof(dto.ExpertiseId),
                        dto);
                }

                var modelCheker = ModelHelper<CompanionUserDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }

                var item = mapper.Map<CompanionUser>(dto);

                if (!string.IsNullOrWhiteSpace(dto.Phone))
                {
                    dto.Phone = await dto.Phone.Trim().ToEnglishDigitsAsync();

                    dto.Phone = dto.Phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

                    if (dto.Phone.StartsWith("+98"))
                        dto.Phone = "0" + dto.Phone.Substring(3);

                    if (dto.Phone.StartsWith("98") && dto.Phone.Length == 12)
                        dto.Phone = "0" + dto.Phone.Substring(2);

                    var user = await _userService.GetByMobileDto(dto.Phone);

                    if (user == null)
                    {
                        return new BaseResultDto<CompanionUserDto>(false, Resource.Notification.UserNotFound, dto);
                    }

                    item.UserId = user.Id;
                    dto.UserId = user.Id;
                }
                else
                {
                    if (dto.UserId <= 0)
                    {
                        return new BaseResultDto<CompanionUserDto>(false, Resource.Notification.UserNotFound, dto);
                    }

                    item.UserId = dto.UserId;
                }

                item.CompanionId = dto.CompanionId;
                item.Active = dto.Active;
                item.UserAccept = dto.UserAccept;
                item.ExpertiseId = dto.ExpertiseId;
                item.Expertise = await _context.Expertises
                    .FirstAsync(x =>
                        x.Id == dto.ExpertiseId.Value &&
                        x.Active &&
                        !x.Deleted);

                var isDuplicate = await _context.CompanionUsers.AsNoTracking().AnyAsync(x => x.CompanionId == item.CompanionId && x.UserId == item.UserId && !x.Deleted);

                if (isDuplicate)
                {
                    return new BaseResultDto<CompanionUserDto>(false, Resource.Notification.DuplicateValue,dto);
                }

                await _context.CompanionUsers.AddAsync(item);
                await _context.SaveChangesAsync();

                try
                {
                    await _notificationService.CreateAsync(new NoticeCreateDto
                    {
                        Label = NoticeTypeLabels.CompanionUserSubmitted,
                        ActorUserId = item.UserId,
                        ReferenceType = "CompanionUser",
                        ReferenceId = item.Id,
                        DeduplicationKey = $"{NoticeTypeLabels.CompanionUserSubmitted}:{item.Id}",
                        Metadata = new Dictionary<string, string>
                        {
                            { "companionId", item.CompanionId.ToString() }
                        }
                    });
                }
                catch
                {
                    // ثبت عضویت انجام شده است؛ اختلال اعلان نباید پاسخ ذخیره را ناموفق کند.
                }

                return new BaseResultDto<CompanionUserDto>(true,mapper.Map<CompanionUserDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<CompanionUserDto>(isSuccess: false,val: ex.Message,data: dto);
            }
        }

        public async Task<BaseResultDto<CompanionUserDto>> UpdateAsyncDto(CompanionUserDto dto)
        {
            try
            {
                var modelChecker = ModelHelper<CompanionUserDto>.ModelErrors(dto);
                if (!modelChecker.IsSuccess)
                {
                    return modelChecker;
                }

                if (!await IsValidExpertiseAsync(dto.ExpertiseId))
                {
                    return new BaseResultDto<CompanionUserDto>(
                        false,
                        "لطفاً یک عنوان شغلی فعال و معتبر انتخاب کنید.",
                        nameof(dto.ExpertiseId),
                        dto);
                }

                var item = await _context.CompanionUsers
                    .Include(x => x.User)
                    .Include(x => x.Expertise)
                    .FirstOrDefaultAsync(x => x.Id == dto.Id && !x.Deleted);

                if (item == null)
                {
                    return new BaseResultDto<CompanionUserDto>(
                        false,
                        Resource.Notification.NothingFound,
                        dto);
                }

                var companionId = dto.CompanionId > 0
                    ? dto.CompanionId
                    : item.CompanionId;

                var companionExists = await _context.Companions
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == companionId && !x.Deleted);

                if (!companionExists)
                {
                    return new BaseResultDto<CompanionUserDto>(
                        false,
                        Resource.Notification.NothingFound,
                        dto);
                }

                var userId = dto.UserId;
                if (!string.IsNullOrWhiteSpace(dto.Phone))
                {
                    dto.Phone = await dto.Phone.Trim().ToEnglishDigitsAsync();
                    dto.Phone = dto.Phone
                        .Replace(" ", "")
                        .Replace("-", "")
                        .Replace("(", "")
                        .Replace(")", "");

                    if (dto.Phone.StartsWith("+98"))
                    {
                        dto.Phone = "0" + dto.Phone.Substring(3);
                    }

                    if (dto.Phone.StartsWith("98") && dto.Phone.Length == 12)
                    {
                        dto.Phone = "0" + dto.Phone.Substring(2);
                    }

                    var user = await _userService.GetByMobileDto(dto.Phone);
                    if (user == null)
                    {
                        return new BaseResultDto<CompanionUserDto>(
                            false,
                            Resource.Notification.UserNotFound,
                            dto);
                    }

                    userId = user.Id;
                }
                else
                {
                    var userExists = userId > 0 && await _context.Users
                        .AsNoTracking()
                        .AnyAsync(x => x.Id == userId && !x.Deleted);

                    if (!userExists)
                    {
                        return new BaseResultDto<CompanionUserDto>(
                            false,
                            Resource.Notification.UserNotFound,
                            dto);
                    }
                }

                var isDuplicate = await _context.CompanionUsers
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id != item.Id &&
                        x.CompanionId == companionId &&
                        x.UserId == userId &&
                        !x.Deleted);

                if (isDuplicate)
                {
                    return new BaseResultDto<CompanionUserDto>(
                        false,
                        Resource.Notification.DuplicateValue,
                        dto);
                }

                item.CompanionId = companionId;
                item.UserId = userId;
                item.Active = dto.Active;
                item.ExpertiseId = dto.ExpertiseId;
                item.Expertise = await _context.Expertises
                    .FirstAsync(x => x.Id == dto.ExpertiseId.Value);

                await SynchronizeUserExpertiseAsync(item);

                _context.CompanionUsers.Update(item);
                await _context.SaveChangesAsync();

                return new BaseResultDto<CompanionUserDto>(
                    true,
                    mapper.Map<CompanionUserDto>(item));
            }
            catch (Exception)
            {
                return new BaseResultDto<CompanionUserDto>(
                    false,
                    Resource.Notification.Unsuccess,
                    dto);
            }
        }

        public async Task<BaseResultDto<List<CompanionUserDto>>> GetAvailableCompanionUsersAsync(long companionAssistanceId)
        {
            var companionAssistance = await _context.CompanionAssistances
                .Include(s => s.Companion)
                .FirstOrDefaultAsync(s => s.Id == companionAssistanceId && !s.Deleted);

            if (companionAssistance == null)
            {
                return new BaseResultDto<List<CompanionUserDto>>(false, Resource.Notification.NothingFound, new List<CompanionUserDto>());
            }

            if (companionAssistance.Companion != null && companionAssistance.Companion.IsPersonal)
            {
                return new BaseResultDto<List<CompanionUserDto>>(false, Resource.Notification.AccessDenied, new List<CompanionUserDto>());
            }

            var assignedUserIds = await _context.CompanionAssistanceUsers
                .Where(s => s.CompanionAssistanceId == companionAssistanceId && !s.Deleted)
                .Select(s => s.UserId)
                .ToListAsync();

            var companionUsers = await _context.CompanionUsers
                .Include(s => s.User)
                .ThenInclude(s => s.Picture)
                .Include(s => s.Companion)
                .Include(s => s.Expertise)
                .Where(s =>
                    s.CompanionId == companionAssistance.CompanionId &&
                    !s.Deleted &&
                    s.Active &&
                    s.UserAccept == true &&
                    !assignedUserIds.Contains(s.UserId)
                )
                .ToListAsync();

            return new BaseResultDto<List<CompanionUserDto>>(
                true,
                mapper.Map<List<CompanionUserDto>>(companionUsers)
            );
        }

        public async Task<BaseResultDto> Active(CompanionUserDto user)
        {
            var query = _context.CompanionUsers
                .Include(x => x.User)
                .Include(x => x.Expertise)
                .Where(x => x.Id == user.Id && !x.Deleted);

            if (user.CompanionId > 0)
                query = query.Where(x => x.CompanionId == user.CompanionId);

            var item = await query.FirstOrDefaultAsync();
            if (item == null)
                return new BaseResultDto(false, val: Resource.Notification.AccessDenied);

            if (user.ExpertiseId.HasValue && !await IsValidExpertiseAsync(user.ExpertiseId))
            {
                return new BaseResultDto(
                    false,
                    "لطفاً یک عنوان شغلی فعال و معتبر انتخاب کنید.",
                    nameof(user.ExpertiseId));
            }

            item.Active = user.Active;
            if (user.ExpertiseId.HasValue)
            {
                item.ExpertiseId = user.ExpertiseId;
                item.Expertise = await _context.Expertises
                    .FirstAsync(x => x.Id == user.ExpertiseId.Value);
            }

            await SynchronizeUserExpertiseAsync(item);
            _context.CompanionUsers.Update(item);
            await _context.SaveChangesAsync();

            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto> UserAccept(CompanionUserDto user)
        {
            if (!user.UserAccept.HasValue)
                return new BaseResultDto(false, Resource.Notification.Unsuccess);

            return await UserAccept(user.Id, user.UserId, user.UserAccept.Value);
        }

        public async Task<BaseResultDto> UserAccept(long id, long userId, bool userAccept)
        {
            var item = await _context.CompanionUsers
                .Include(x => x.User)
                .Include(x => x.Expertise)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.UserId == userId &&
                    !x.Deleted);

            if (item == null)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);

            if (item.UserAccept.HasValue)
                return new BaseResultDto(false, Resource.Notification.AlreadyChoose);

            item.UserAccept = userAccept;
            item.Active = userAccept;

            await SynchronizeUserExpertiseAsync(item);
            await _context.SaveChangesAsync();

            return new BaseResultDto(true);
        }

        private async Task SynchronizeUserExpertiseAsync(CompanionUser membership)
        {
            if (membership.User == null)
                return;

            if (membership.Active && membership.UserAccept == true)
            {
                membership.User.Expertise = membership.Expertise?.Name;
                return;
            }

            var fallbackExpertise = await _context.CompanionUsers
                .AsNoTracking()
                .Where(x =>
                    x.Id != membership.Id &&
                    x.UserId == membership.UserId &&
                    !x.Deleted &&
                    x.Active &&
                    x.UserAccept == true &&
                    x.ExpertiseId.HasValue &&
                    x.Expertise.Active &&
                    !x.Expertise.Deleted)
                .OrderByDescending(x => x.Id)
                .Select(x => x.Expertise.Name)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrWhiteSpace(fallbackExpertise))
            {
                membership.User.Expertise = fallbackExpertise;
                return;
            }

            var ownsActiveCompanion = await _context.Companions
                .AsNoTracking()
                .AnyAsync(x =>
                    x.OwnerId == membership.UserId &&
                    !x.Deleted &&
                    x.Active &&
                    x.Approved);

            if (!ownsActiveCompanion)
                membership.User.Expertise = null;
        }

        private async Task<bool> IsValidExpertiseAsync(long? expertiseId)
        {
            if (!expertiseId.HasValue)
                return false;

            return await _context.Expertises
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == expertiseId.Value &&
                    x.Active &&
                    !x.Deleted);
        }
    }
}
