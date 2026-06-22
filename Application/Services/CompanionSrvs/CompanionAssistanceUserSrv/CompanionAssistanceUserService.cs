using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Common.Service;
using Application.Services.CompanionSrv.CompanionAssistanceUserSrv.Dto;
using Application.Services.CompanionSrv.CompanionAssistanceUserSrv.Iface;
using Application.Services.CompanionSrvs.CompanionAssistanceSrv.Dto;
using Application.Services.CompanionSrvs.CompanionAssistanceUserSrv.Dto;
using Application.Services.CompanionSrvs.CompanionUserSrv.Dto;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrv.CompanionAssistanceUserSrv
{
    public class CompanionAssistanceUserService : CommonSrv<CompanionAssistanceUser, CompanionAssistanceUserDto>, ICompanionAssistanceUserService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        public CompanionAssistanceUserService(IDataBaseContext _context, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }

        public override async Task<BaseResultDto<CompanionAssistanceUserDto>> FindAsyncDto(long id)
        {
            var item = await _context.CompanionAssistanceUsers.Include(s => s.CompanionAssistance).ThenInclude(s => s.Companion).Include(s => s.CompanionAssistance).ThenInclude(s => s.Assistance).Include(s => s.User).FirstOrDefaultAsync(s => s.Id == id && !s.Deleted && s.Active);
            if (item != null)
            {
                return new BaseResultDto<CompanionAssistanceUserDto>(true, mapper.Map<CompanionAssistanceUserDto>(item));
            }
            return new BaseResultDto<CompanionAssistanceUserDto>(false, mapper.Map<CompanionAssistanceUserDto>(item));
        }
        public async Task<BaseResultDto<CompanionAssistanceUserVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.CompanionAssistanceUsers.Include(s => s.CompanionAssistance).ThenInclude(s => s.Companion).Include(s => s.CompanionAssistance).ThenInclude(s => s.Assistance).Include(s => s.User).FirstOrDefaultAsync(s => s.Id == id && !s.Deleted && s.Active);
            if (item != null)
            {
                return new BaseResultDto<CompanionAssistanceUserVDto>(true, mapper.Map<CompanionAssistanceUserVDto>(item));
            }
            return new BaseResultDto<CompanionAssistanceUserVDto>(false, mapper.Map<CompanionAssistanceUserVDto>(item));
        }

        public CompanionAssistanceUserSearchDto Search(CompanionAssistanceUserInputDto baseSearchDto)
        {
            var model = _context.CompanionAssistanceUsers.Include(s => s.CompanionAssistance).ThenInclude(s => s.Companion).Include(s => s.CompanionAssistance).ThenInclude(s => s.Assistance).Include(s => s.User).AsQueryable().Where(s => !s.Deleted);

            if (baseSearchDto.Available.HasValue)
            {
                model = model.Where(s => s.Active == baseSearchDto.Available.Value);
            }
            if (baseSearchDto.CompanionId.HasValue)
            {
                model = model.Where(s => s.CompanionAssistance.CompanionId == baseSearchDto.CompanionId.Value);
            }
            if (baseSearchDto.CompanionAssistanceId.HasValue)
            {
                model = model.Where(s => s.CompanionAssistanceId == baseSearchDto.CompanionAssistanceId.Value);
            }
            if (baseSearchDto.UserId.HasValue)
            {
                model = model.Where(s => s.UserId == baseSearchDto.UserId.Value);
            }
            if (!string.IsNullOrEmpty(baseSearchDto.Q))
            {
                model = model.Where(s => s.User.FirstName.Contains(baseSearchDto.Q) || s.User.LastName.Contains(baseSearchDto.Q) || s.User.Mobile.Contains(baseSearchDto.Q));
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
            return new CompanionAssistanceUserSearchDto(baseSearchDto, model, mapper);
        }

        public override async Task<BaseResultDto<CompanionAssistanceUserDto>> InsertAsyncDto(CompanionAssistanceUserDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<CompanionAssistanceUserDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }

                var companionAssistance = await _context.CompanionAssistances
                    .Include(s => s.Companion)
                    .FirstOrDefaultAsync(s => s.Id == dto.CompanionAssistanceId && !s.Deleted);

                if (companionAssistance == null)
                {
                    return new BaseResultDto<CompanionAssistanceUserDto>(false, Resource.Notification.NothingFound, dto);
                }

                if (companionAssistance.Companion != null && companionAssistance.Companion.IsPersonal)
                {
                    return new BaseResultDto<CompanionAssistanceUserDto>(false, Resource.Notification.AccessDenied, dto);
                }

                var hasCompanionUser = await _context.CompanionUsers.AnyAsync(s =>
                    s.CompanionId == companionAssistance.CompanionId &&
                    s.UserId == dto.UserId &&
                    !s.Deleted &&
                    s.Active
                );

                if (!hasCompanionUser)
                {
                    return new BaseResultDto<CompanionAssistanceUserDto>(false, Resource.Notification.AccessDenied, dto);
                }

                bool exists = await _context.CompanionAssistanceUsers.AnyAsync(a =>
                    a.UserId == dto.UserId &&
                    a.CompanionAssistanceId == dto.CompanionAssistanceId &&
                    !a.Deleted
                );

                if (exists)
                {
                    return new BaseResultDto<CompanionAssistanceUserDto>(false, Resource.Notification.DuplicateValue, dto);
                }

                dto.Active = true;

                var item = mapper.Map<CompanionAssistanceUser>(dto);

                await _context.CompanionAssistanceUsers.AddAsync(item);
                await _context.SaveChangesAsync();

                return new BaseResultDto<CompanionAssistanceUserDto>(true, mapper.Map<CompanionAssistanceUserDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<CompanionAssistanceUserDto>(isSuccess: false, val: ex.Message, data: dto);
            }
        }

        public BaseResultDto ActivationDto(CompanionAssistanceUserActivationDto dto)
        {
            var item = _context.CompanionAssistanceUsers.FirstOrDefault(s => s.Id == dto.Id && !s.Deleted);
            if (!dto.Active)
            {
                item.Active = false;
                item.ActivationValue = dto.ActivationValue;

                if (!item.Active && string.IsNullOrEmpty(dto.ActivationValue))
                {
                    return new BaseResultDto(false, Resource.Notification.PleaseEnterTheActivationValueReason);
                }
            }
            else
            {
                item.Active = true;
                item.ActivationValue = dto.ActivationValue;
            }
            _context.CompanionAssistanceUsers.Update(item);
            _context.SaveChanges();
            return new BaseResultDto(isSuccess: true);
        }
    }
}
