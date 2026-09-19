using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Helpers;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Services.CompanionSrvs.CompanionInsurancePackageSrv.Dto;
using Application.Services.CompanionSrvs.CompanionInsurancePackageSrv.Iface;
using AutoMapper;
using DocumentFormat.OpenXml.Office.CustomUI;
using Entities.Entities;
using Entities.Entities.CompanionField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.CompanionInsurancePackageSrv
{
    public class CompanionInsurancePackageService : CommonSrv<CompanionInsurancePackage, CompanionInsurancePackageDto>, ICompanionInsurancePackageService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ICurrentUserHelper _currentUser;
        public CompanionInsurancePackageService(IDataBaseContext _context, IMapper mapper, ICurrentUserHelper currentUser) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this._currentUser = currentUser;
        }
        public override async Task<BaseResultDto<CompanionInsurancePackageDto>> FindAsyncDto(long id)
        {
            var item = await _context.CompanionInsurancePackages.Include(s => s.Companion).Include(s => s.Pet).FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);
            if (item != null)
            {
                return new BaseResultDto<CompanionInsurancePackageDto>(true, mapper.Map<CompanionInsurancePackageDto>(item));
            }
            return new BaseResultDto<CompanionInsurancePackageDto>(false, mapper.Map<CompanionInsurancePackageDto>(item));
        }
        public async Task<BaseResultDto<CompanionInsurancePackageVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.CompanionInsurancePackages.Include(s => s.Companion).Include(s => s.Pet).FirstOrDefaultAsync(s => s.Id == id && !s.Deleted && s.Active);
            if (item != null)
            {
                return new BaseResultDto<CompanionInsurancePackageVDto>(true, mapper.Map<CompanionInsurancePackageVDto>(item));
            }
            return new BaseResultDto<CompanionInsurancePackageVDto>(false, mapper.Map<CompanionInsurancePackageVDto>(item));
        }

        public CompanionInsurancePackageSearchDto Search(CompanionInsurancePackageInputDto baseSearchDto)
        {
            var model = _context.CompanionInsurancePackages.Include(s => s.Companion).Include(s => s.Pet).AsQueryable().Where(s => !s.Deleted);

            if (baseSearchDto.Available.HasValue)
            {
                model = model.Where(s => s.Active == baseSearchDto.Available.Value);
            }
            if (baseSearchDto.CompanionId.HasValue)
            {
                model = model.Where(s => s.CompanionId == baseSearchDto.CompanionId.Value);
            }
            if (baseSearchDto.PetId.HasValue)
            {
                model = model.Where(s => s.PetId == baseSearchDto.PetId.Value);
            }
            if (baseSearchDto.MaxDayCount.HasValue)
            {
                model = model.Where(s => s.DayCount <= baseSearchDto.MaxDayCount.Value);
            }
            if (baseSearchDto.MinDayCount.HasValue)
            {
                model = model.Where(s => s.DayCount == baseSearchDto.MinDayCount.Value);
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
                        model = model.OrderByDescending(s => s.Priority);
                        break;
                    }
                case Common.Enumerable.SortEnum.LessPriority:
                    {
                        model = model.OrderBy(s => s.Priority);
                        break;
                    }
                default:
                    break;
            }
            return new CompanionInsurancePackageSearchDto(baseSearchDto, model, mapper);
        }

        public override async Task<BaseResultDto<CompanionInsurancePackageDto>> InsertAsyncDto(CompanionInsurancePackageDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<CompanionInsurancePackageDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                bool exists = await _context.CompanionInsurancePackages.AnyAsync(a => a.PetId == dto.PetId && a.Price == dto.Price && a.DayCount == dto.DayCount && a.CompanionId == dto.CompanionId && !a.Deleted);

                if (exists)
                {
                    return new BaseResultDto<CompanionInsurancePackageDto>(false, Resource.Notification.DuplicateValue, dto);
                }
                var item = mapper.Map<CompanionInsurancePackage>(dto);
                if (_currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin)
                {
                    item.Active = true;
                }
                await _context.CompanionInsurancePackages.AddAsync(item);
                await _context.SaveChangesAsync();

                return new BaseResultDto<CompanionInsurancePackageDto>(true, mapper.Map<CompanionInsurancePackageDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<CompanionInsurancePackageDto>(isSuccess: false, val: Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex), data: dto);
            }
        }

        public BaseResultDto ActivationDto(CompanionInsurancePackageActivationDto dto)
        {
            var item = _context.CompanionInsurancePackages.FirstOrDefault(s => s.Id == dto.Id && !s.Deleted);
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
            _context.CompanionInsurancePackages.Update(item);
            _context.SaveChanges();
            return new BaseResultDto(isSuccess: true);
        }
    }
}
