using Application.Common.Dto.Result;
using Application.Common.Service;
using Application.Services.TripSrv.PriceCalculationSrv.Dto;
using Application.Services.TripSrv.TripAddressSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.TripSrv.TripAddressSrv
{
    public class TripAddressService : CommonSrv<TripAddress, TripAddressDto>, ITripAddressService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        public TripAddressService(IDataBaseContext _context, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }

        public async Task<BaseResultDto<TripAddressVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.TripAddresses.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);
            if (item != null)
            {
                return new BaseResultDto<TripAddressVDto>(true, mapper.Map<TripAddressVDto>(item));
            }
            return new BaseResultDto<TripAddressVDto>(false, mapper.Map<TripAddressVDto>(item));
        }

        public override async Task<BaseResultDto<TripAddressDto>> InsertAsyncDto(TripAddressDto dto)
        {
            // اولین آدرسِ ذخیره‌شده‌ی هر کاربر خودکار منتخب می‌شه — بقیه باید صریحاً از SelectAsync استفاده کنن.
            var hasAnyAddress = await _context.TripAddresses.AnyAsync(s => s.UserId == dto.UserId && !s.Deleted);

            var result = await base.InsertAsyncDto(dto);

            if (!hasAnyAddress && result.IsSuccess && result.Data != null)
            {
                await _context.TripAddresses
                    .Where(s => s.Id == result.Data.Id)
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsSelected, true));
                result.Data.IsSelected = true;
            }

            return result;
        }

        public async Task<BaseResultDto> SelectAsync(long id, long userId)
        {
            var address = await _context.TripAddresses.AsTracking()
                .FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);

            if (address == null || address.UserId != userId)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);

            if (!address.IsSelected)
            {
                await _context.TripAddresses
                    .Where(s => s.UserId == userId && s.Id != id && s.IsSelected && !s.Deleted)
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsSelected, false));

                address.IsSelected = true;
                _context.TripAddresses.Update(address);
                await _context.SaveChangesAsync();
            }

            return new BaseResultDto(true, Resource.Notification.Success);
        }

        public TripAddressSearchDto Search(TripAddressInputDto baseSearchDto)
        {
            var model = _context.TripAddresses.Include(s => s.User).AsQueryable().Where(s => !s.Deleted);

            if (baseSearchDto.UserId.HasValue)
            {
                model = model.Where(s => s.UserId == baseSearchDto.UserId.Value);
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
            return new TripAddressSearchDto(baseSearchDto, model, mapper);
        }
    }
}
