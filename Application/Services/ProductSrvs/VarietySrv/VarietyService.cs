using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Common.Service;
using Application.Services.ProductSrvs.VarietySrv.Dto;
using Application.Services.ProductSrvs.VarietySrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.ProductSrvs.VarietySrv
{
    public class VarietyService : CommonSrv<Variety, VarietyDto>, IVarietyService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        public VarietyService(IDataBaseContext _context, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }
        // ادمین تنوعی را که روی محصولی تنظیم شده یا مقدارهایش در آیتم‌ها/سفارش‌ها استفاده شده حذف نمی‌کند.
        public override BaseResultDto DeleteDto(long id)
        {
            var usedByProduct = _context.Products.AsNoTracking()
                .Any(p => !p.Deleted && (p.VarietyId == id || p.Variety2Id == id));
            var valuesInUse = _context.ProductItems.IgnoreQueryFilters()
                .Any(pi => ((pi.VarietyItem != null && pi.VarietyItem.VarietyId == id) || (pi.VarietyItem2 != null && pi.VarietyItem2.VarietyId == id))
                           && (!pi.Deleted || _context.ProductOrderItems.Any(oi => oi.ProductItemId == pi.Id)));
            if (usedByProduct || valuesInUse)
                return new BaseResultDto(false, Resource.Notification.VarietyInUseCannotBeDeleted);
            return base.DeleteDto(id);
        }

        public BaseSearchDto<VarietyVDto> SearchDto(BaseInputDto dto)
        {
            var model = _context.Varieties.Include(s => s.VarietyItems.Where(s => s.Deleted == false)).Where(s => s.Deleted == false).AsQueryable();
            if (!string.IsNullOrEmpty(dto.Q))
            {
                model = model.Where(s => s.Name.Contains(dto.Q)).OrderByDescending(o => o.Id);
            }
            return new BaseSearchDto<Variety, VarietyVDto>(dto, model, mapper);
        }

        public override async Task<BaseResultDto<VarietyDto>> InsertAsyncDto(VarietyDto dto)
        {

            try
            {
                var modelCheker = ModelHelper<VarietyDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    if ((!NameIsUnique(dto.Name)))
                    {
                        return new BaseResultDto<VarietyDto>(isSuccess: false, val1: Resource.Notification.TheNameIsDuplicate, val2: nameof(dto.Name), dto);
                    }
                    var item = mapper.Map<Variety>(dto);
                    await _context.Varieties.AddAsync(item);
                    _context.SaveChanges();
                    return new BaseResultDto<VarietyDto>(true, mapper.Map<VarietyDto>(item));
                }

            }
            catch (Exception ex)
            {
                return new BaseResultDto<VarietyDto>(isSuccess: false, val: Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex), data: dto);
            }


        }

        public override BaseResultDto UpdateDto(VarietyDto dto)
        {

            try
            {
                var modelCheker = ModelHelper<VarietyDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    var item = _context.Varieties.FirstOrDefault(s => s.Id == dto.Id);
                    if (dto.Name != item.Name && (!NameIsUnique(dto.Name)))
                    {
                        return new BaseResultDto<VarietyDto>(isSuccess: false, val1: Resource.Notification.TheNameIsDuplicate, val2: nameof(dto.Name), dto);
                    }
                    mapper.Map(dto, item);
                    _context.Varieties.Attach(item);
                    _context.Entry(item).State = EntityState.Modified;
                    _context.SaveChanges();
                    return new BaseResultDto<VarietyDto>(true, mapper.Map<VarietyDto>(item));
                }
            }
            catch (Exception ex)
            {
                return new BaseResultDto(isSuccess: false, val: Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex));
            }

        }
        bool NameIsUnique(string name)
        {
            var item = _context.Varieties.FirstOrDefault(x => x.Name == name);
            if (item == null)
                return true;
            return false;
        }

        public async Task<BaseResultDto<VarietyVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.Varieties.Include(s => s.VarietyItems).FirstOrDefaultAsync(s => s.Id == id);
            if (item != null)
                return new BaseResultDto<VarietyVDto>(true, mapper.Map<VarietyVDto>(item));
            return new BaseResultDto<VarietyVDto>(false, mapper.Map<VarietyVDto>(item));
        }
    }
}
