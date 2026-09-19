using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Common.Service;
using Application.Services.ProductSrvs.VarietyItemSrv.Dto;
using Application.Services.ProductSrvs.VarietyItemSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.ProductSrvs.VarietyItemSrv
{
    public class VarietyItemService : CommonSrv<VarietyItem, VarietyItemDto>, IVarietyItemService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        public VarietyItemService(IDataBaseContext _context, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }

        public VarietyItemSearchDto SearchDto(VarietyItemInputDto searchDto)
        {
            var model = _context.VarietyItems.Where(s => s.VarietyId == searchDto.VarietyId && s.Deleted == false).AsQueryable();
            if (!string.IsNullOrEmpty(searchDto.Q))
            {
                model = model.Where(s => s.Name.Contains(searchDto.Q)).OrderByDescending(o => o.Id);
            }
            return new VarietyItemSearchDto(searchDto, model, mapper);
        }

        public override async Task<BaseResultDto<VarietyItemDto>> InsertAsyncDto(VarietyItemDto dto)
        {

            try
            {
                var modelCheker = ModelHelper<VarietyItemDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    if (string.IsNullOrEmpty(SearchNormalizeHelper.NormalizeNoSpace(dto.Name)))
                    {
                        return new BaseResultDto<VarietyItemDto>(isSuccess: false, val1: Resource.Notification.InvalidData, val2: nameof(dto.Name), dto);
                    }
                    if (!NameIsUnique(dto.Name, dto.VarietyId))
                    {
                        return new BaseResultDto<VarietyItemDto>(isSuccess: false, val1: Resource.Notification.TheNameIsDuplicate, val2: nameof(dto.Name), dto);
                    }
                    var item = mapper.Map<VarietyItem>(dto);
                    await _context.VarietyItems.AddAsync(item);
                    _context.SaveChanges();
                    return new BaseResultDto<VarietyItemDto>(true, mapper.Map<VarietyItemDto>(item));
                }

            }
            catch (Exception ex)
            {
                return new BaseResultDto<VarietyItemDto>(isSuccess: false, val: Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex), data: dto);
            }


        }

        public override BaseResultDto UpdateDto(VarietyItemDto dto)
        {

            try
            {
                var modelCheker = ModelHelper<VarietyItemDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    var item = _context.VarietyItems.FirstOrDefault(s => s.Id == dto.Id);
                    if (item == null)
                        return new BaseResultDto(isSuccess: false, val: Resource.Notification.NothingFound);
                    if (string.IsNullOrEmpty(SearchNormalizeHelper.NormalizeNoSpace(dto.Name)))
                    {
                        return new BaseResultDto<VarietyItemDto>(isSuccess: false, val1: Resource.Notification.InvalidData, val2: nameof(dto.Name), dto);
                    }
                    // فقط وقتی نام یا تنوع عوض می‌شود بررسی می‌کنیم تا ردیف‌های تکراریِ قدیمی همچنان قابل ویرایش (مثلاً Label) بمانند
                    if ((dto.Name != item.Name || dto.VarietyId != item.VarietyId) && !NameIsUnique(dto.Name, dto.VarietyId, item.Id))
                    {
                        return new BaseResultDto<VarietyItemDto>(isSuccess: false, val1: Resource.Notification.TheNameIsDuplicate, val2: nameof(dto.Name), dto);
                    }
                    mapper.Map(dto, item);
                    _context.VarietyItems.Attach(item);
                    _context.Entry(item).State = EntityState.Modified;
                    _context.SaveChanges();
                    return new BaseResultDto<VarietyItemDto>(true, mapper.Map<VarietyItemDto>(item));
                }
            }
            catch (Exception ex)
            {
                return new BaseResultDto(isSuccess: false, val: Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex));
            }

        }
        // ادمین فقط مقدار «اضافه» می‌کند؛ مقداری که در آیتم فعال یک فروشنده یا در سفارش‌ها استفاده شده حذف نمی‌شود.
        public override BaseResultDto DeleteDto(long id)
        {
            var inUse = _context.ProductItems.IgnoreQueryFilters()
                .Any(pi => (pi.VarietyItemId == id || pi.VarietyItem2Id == id)
                           && (!pi.Deleted || _context.ProductOrderItems.Any(oi => oi.ProductItemId == pi.Id)));
            if (inUse)
                return new BaseResultDto(false, Resource.Notification.VarietyItemInUseCannotBeDeleted);
            return base.DeleteDto(id);
        }

        // یکتایی «داخل همان تنوع» و با نرمال‌سازی املایی (فاصله/نیم‌فاصله، ی/ک عربی، ارقام فارسی/لاتین، حروف بزرگ/کوچک)؛
        // ردیف‌های حذف‌شده مانع نیستند. «قرمز» در دو تنوع مختلف مجاز است. مقدارهای هر تنوع کم‌اند، پس مقایسه در حافظه مشکلی ندارد.
        bool NameIsUnique(string name, long varietyId, long excludeId = 0)
        {
            var normalized = SearchNormalizeHelper.NormalizeNoSpace(name);
            var existingNames = _context.VarietyItems.AsNoTracking()
                .Where(x => x.VarietyId == varietyId && !x.Deleted && x.Id != excludeId)
                .Select(x => x.Name)
                .ToList();
            return !existingNames.Any(existing => SearchNormalizeHelper.NormalizeNoSpace(existing) == normalized);
        }
    }
}
