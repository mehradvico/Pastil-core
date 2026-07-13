using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Helpers;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Services.PastilMatchSrvs.PastilMatchReportReasonSrv.Dto;
using Application.Services.PastilMatchSrvs.PastilMatchReportReasonSrv.Iface;
using AutoMapper;
using Entities.Entities.PastilMatchField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchReportReasonSrv
{
    public class PastilMatchReportReasonService : CommonSrv<PastilMatchReportReason, PastilMatchReportReasonDto>, IPastilMatchReportReasonService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ICurrentUserHelper _currentUser;

        public PastilMatchReportReasonService(IDataBaseContext context, IMapper mapper, ICurrentUserHelper currentUser) : base(context, mapper)
        {
            _context = context;
            this.mapper = mapper;
            _currentUser = currentUser;
        }

        public async Task<BaseResultDto<PastilMatchReportReasonVDto>> FindAsyncVDto(long id)
        {
            try
            {
                var isAdmin = _currentUser.CurrentUser.RoleEnum == RoleEnum.Admin.ToString();

                var item = await _context.PastilMatchReportReasons.FirstOrDefaultAsync(s => s.Id == id && !s.Deleted && (isAdmin || s.Active));

                if (item == null)
                {
                    return new BaseResultDto<PastilMatchReportReasonVDto>(false, Resource.Notification.NothingFound, null);
                }

                return new BaseResultDto<PastilMatchReportReasonVDto>(true, mapper.Map<PastilMatchReportReasonVDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<PastilMatchReportReasonVDto>(false, ex.Message, null);
            }
        }

        public PastilMatchReportReasonSearchDto Search(PastilMatchReportReasonInputDto dto)
        {
            var isAdmin = _currentUser.CurrentUser.RoleEnum == RoleEnum.Admin.ToString();

            var model = _context.PastilMatchReportReasons.Where(s => !s.Deleted).AsQueryable();

            if (!isAdmin)
            {
                model = model.Where(s => s.Active);
            }
            else if (dto.Available.HasValue)
            {
                model = model.Where(s => s.Active == dto.Available.Value);
            }

            if (dto.IsDescriptionRequired.HasValue)
            {
                model = model.Where(s => s.IsDescriptionRequired == dto.IsDescriptionRequired.Value);
            }

            if (!string.IsNullOrWhiteSpace(dto.Q))
            {
                model = model.Where(s => s.Title.Contains(dto.Q) || s.Description.Contains(dto.Q));
            }

            switch (dto.SortBy)
            {
                case SortEnum.New:
                    {
                        model = model.OrderByDescending(s => s.Id);
                        break;
                    }
                case SortEnum.Old:
                    {
                        model = model.OrderBy(s => s.Id);
                        break;
                    }
                case SortEnum.Name:
                    {
                        model = model.OrderBy(s => s.Title);
                        break;
                    }
                default:
                    {
                        model = model.OrderBy(s => s.Priority).ThenBy(s => s.Id);
                        break;
                    }
            }

            return new PastilMatchReportReasonSearchDto(dto, model, mapper);
        }

        public override async Task<BaseResultDto<PastilMatchReportReasonDto>> InsertAsyncDto(PastilMatchReportReasonDto dto)
        {
            try
            {
                var isAdmin = _currentUser.CurrentUser.RoleEnum == RoleEnum.Admin.ToString();

                if (!isAdmin)
                {
                    return new BaseResultDto<PastilMatchReportReasonDto>(false, Resource.Notification.AccessDenied, dto);
                }

                var modelChecker = ModelHelper<PastilMatchReportReasonDto>.ModelErrors(dto);

                if (!modelChecker.IsSuccess)
                {
                    return modelChecker;
                }

                if (string.IsNullOrWhiteSpace(dto.Title))
                {
                    return new BaseResultDto<PastilMatchReportReasonDto>(false, Resource.Notification.PleaseEnterTheName, dto);
                }

                var reasonExists = await _context.PastilMatchReportReasons.AnyAsync(s => s.Title == dto.Title.Trim() && !s.Deleted);

                if (reasonExists)
                {
                    return new BaseResultDto<PastilMatchReportReasonDto>(false, Resource.Notification.DuplicateValue, dto);
                }

                var item = mapper.Map<PastilMatchReportReason>(dto);

                item.Title = dto.Title.Trim();
                item.Active = true;
                item.Deleted = false;

                await _context.PastilMatchReportReasons.AddAsync(item);
                await _context.SaveChangesAsync();

                return new BaseResultDto<PastilMatchReportReasonDto>(true, mapper.Map<PastilMatchReportReasonDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<PastilMatchReportReasonDto>(false, ex.Message, dto);
            }
        }

        public override BaseResultDto UpdateDto(PastilMatchReportReasonDto dto)
        {
            try
            {
                var isAdmin = _currentUser.CurrentUser.RoleEnum == RoleEnum.Admin.ToString();

                if (!isAdmin)
                {
                    return new BaseResultDto(false, Resource.Notification.AccessDenied);
                }

                var modelChecker = ModelHelper<PastilMatchReportReasonDto>.ModelErrors(dto);

                if (!modelChecker.IsSuccess)
                {
                    return modelChecker;
                }

                if (string.IsNullOrWhiteSpace(dto.Title))
                {
                    return new BaseResultDto(false, Resource.Notification.PleaseEnterTheName);
                }

                var item = _context.PastilMatchReportReasons.FirstOrDefault(s => s.Id == dto.Id && !s.Deleted);

                if (item == null)
                {
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                }

                var reasonExists = _context.PastilMatchReportReasons.Any(s => s.Id != dto.Id && s.Title == dto.Title.Trim() && !s.Deleted);

                if (reasonExists)
                {
                    return new BaseResultDto(false, Resource.Notification.DuplicateValue);
                }

                var active = item.Active;
                var deleted = item.Deleted;

                mapper.Map(dto, item);

                item.Title = dto.Title.Trim();
                item.Active = active;
                item.Deleted = deleted;

                _context.PastilMatchReportReasons.Update(item);
                _context.SaveChanges();

                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, ex.Message);
            }
        }

        public BaseResultDto UpdateActiveDto(PastilMatchReportReasonActiveDto dto)
        {
            try
            {
                var isAdmin = _currentUser.CurrentUser.RoleEnum == RoleEnum.Admin.ToString();

                if (!isAdmin)
                {
                    return new BaseResultDto(false, Resource.Notification.AccessDenied);
                }

                var item = _context.PastilMatchReportReasons.FirstOrDefault(s => s.Id == dto.Id && !s.Deleted);

                if (item == null)
                {
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                }

                item.Active = dto.Active;

                _context.PastilMatchReportReasons.Update(item);
                _context.SaveChanges();

                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, ex.Message);
            }
        }

        public override BaseResultDto DeleteDto(long id)
        {
            try
            {
                var isAdmin = _currentUser.CurrentUser.RoleEnum == RoleEnum.Admin.ToString();

                if (!isAdmin)
                {
                    return new BaseResultDto(false, Resource.Notification.AccessDenied);
                }

                var item = _context.PastilMatchReportReasons.FirstOrDefault(s => s.Id == id && !s.Deleted);

                if (item == null)
                {
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                }

                item.Active = false;
                item.Deleted = true;

                _context.PastilMatchReportReasons.Update(item);
                _context.SaveChanges();

                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, ex.Message);
            }
        }

        public override BaseResultDto DeleteDto(PastilMatchReportReasonDto dto)
        {
            return DeleteDto(dto.Id);
        }
    }
}
