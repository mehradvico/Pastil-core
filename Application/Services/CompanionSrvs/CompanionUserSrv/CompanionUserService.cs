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
            var item = await _context.CompanionUsers.Include(p => p.User).ThenInclude(s => s.Picture).Include(s => s.Companion).FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);
            if (item != null)
                return new BaseResultDto<CompanionUserDto>(true, mapper.Map<CompanionUserDto>(item));
            return new BaseResultDto<CompanionUserDto>(false, mapper.Map<CompanionUserDto>(item));
        }

        public CompanionUserSearchDto SearchDto(CompanionUserInputDto dto)
        {

            var model = _context.CompanionUsers.Include(s => s.User).ThenInclude(s => s.Picture).Include(s => s.Companion).AsQueryable().Where(s => !s.Deleted);
            if (dto.Active.HasValue)
            {
                model = model.Where(s => s.Active.Equals(dto.Active));
            }
            if (dto.UserId.HasValue)
            {
                model = model.Where(s => s.UserId == dto.UserId);
            }
            if (dto.UserAccept.HasValue)
            {
                model = model.Where(s => s.UserAccept == dto.UserAccept.Value);
            }
            if (!dto.AllUserAccept)
            {
                model = model.Where(s => s.UserAccept == dto.UserAccept);
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
                var modelCheker = ModelHelper<CompanionUserDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    var item = mapper.Map<CompanionUser>(dto);
                    if (string.IsNullOrEmpty(dto.Phone))
                    {
                        item.UserId = dto.UserId;
                        item.CompanionId = dto.CompanionId;
                    }
                    else
                    {
                        var user = await _userService.GetByMobileDto(dto.Phone);
                        item.UserId = user.Id;
                    }
                    var isDuplicate = _context.CompanionUsers.AsNoTracking().Any(x => x.CompanionId == dto.CompanionId && x.UserId == dto.UserId && !x.Deleted);
                    if (isDuplicate)
                    {
                        return new BaseResultDto<CompanionUserDto>(false, Resource.Notification.DuplicateValue, dto);
                    }
                    await _context.CompanionUsers.AddAsync(item);
                    await _notificationService.InsertNoticeAsync(item.Id, NoticeTypeEnum.NotifType_AddCompanionUser, NoticeUserTypeEnum.NoticeUserType_Admin);
                    await _context.SaveChangesAsync();
                    return new BaseResultDto<CompanionUserDto>(true, mapper.Map<CompanionUserDto>(item));
                }

            }
            catch (Exception ex)
            {
                return new BaseResultDto<CompanionUserDto>(isSuccess: false, val: ex.Message, data: dto);
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
                .Where(s =>
                    s.CompanionId == companionAssistance.CompanionId &&
                    !s.Deleted &&
                    s.Active &&
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
            var item = await _context.CompanionUsers.FirstOrDefaultAsync(x => x.CompanionId == user.CompanionId && x.Id == user.Id);
            if (item != null)
            {
                item.Active = user.Active;
                _context.CompanionUsers.Update(item);
                await _context.SaveChangesAsync();
                return new BaseResultDto(true);

            }
            return new BaseResultDto(false, val: Resource.Notification.AccessDenied);
        }
        public async Task<BaseResultDto> UserAccept(CompanionUserDto user)
        {
            var item = await _context.CompanionUsers.FirstOrDefaultAsync(x => x.Id == user.Id && x.UserId == user.UserId);
            if (item != null)
            {
                if (item.UserAccept.HasValue)
                {
                    return new BaseResultDto<CompanionUserDto>(false, Resource.Notification.AlreadyChoose, user);
                }
                item.UserAccept = user.UserAccept;
                _context.CompanionUsers.Update(item);
                await _context.SaveChangesAsync();
                return new BaseResultDto(true);

            }
            return new BaseResultDto(false, val: Resource.Notification.AccessDenied);
        }
    }
}
