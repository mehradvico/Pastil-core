using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Services.Order.AddressSrv.Dto;
using Application.Services.Order.AddressSrv.iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.Content.AddressSrv
{
    public class AddressService : CommonSrv<Address, AddressDto>, IAddressService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ICurrentUserHelper _currentUserHelper;
        public AddressService(IDataBaseContext _context, IMapper mapper, ICurrentUserHelper currentUserHelper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this._currentUserHelper = currentUserHelper;
        }
        public override async Task<BaseResultDto<AddressDto>> InsertAsyncDto(AddressDto dto)
        {
            dto.UserId = _currentUserHelper.CurrentUser.UserId;

            // اولین آدرسِ هر کاربر خودکار منتخب می‌شه — بقیه باید صریحاً از SelectAsync استفاده کنن.
            // عمداً روی خودِ dto ورودی چیزی ست نمی‌کنیم (UpdateDto این متد رو داخلی و روی یک شیء موقت هم
            // صدا می‌زنه؛ اگه اینجا dto.IsSelected رو تغییر بدیم، همون تغییر روی شیء اصلیِ ویرایش هم اثر
            // می‌ذاره چون هردو یک reference هستن) — به‌جاش بعد از ساخته‌شدن، مستقیم روی ردیف تازه‌ساخته‌شده اعمال می‌کنیم.
            var hasAnyAddress = await _context.Addresses.AnyAsync(s => s.UserId == dto.UserId && !s.Deleted);

            var result = await base.InsertAsyncDto(dto);

            if (!hasAnyAddress && result.IsSuccess && result.Data != null)
            {
                await _context.Addresses
                    .Where(s => s.Id == result.Data.Id)
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsSelected, true));
                result.Data.IsSelected = true;
            }

            return result;
        }

        public async Task<BaseResultDto> SelectAsync(long id, long userId)
        {
            var address = await _context.Addresses.AsTracking()
                .FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);

            if (address == null || address.UserId != userId)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);

            if (!address.IsSelected)
            {
                await _context.Addresses
                    .Where(s => s.UserId == userId && s.Id != id && s.IsSelected && !s.Deleted)
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsSelected, false));

                address.IsSelected = true;
                _context.Addresses.Update(address);
                await _context.SaveChangesAsync();
            }

            return new BaseResultDto(true, Resource.Notification.Success);
        }

        public override BaseResultDto UpdateDto(AddressDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<AddressDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    var existingAddress = _context.Addresses.AsNoTracking()
                        .FirstOrDefault(s => s.Id == dto.Id && !s.Deleted);

                    if (existingAddress == null || existingAddress.UserId != _currentUserHelper.CurrentUser.UserId)
                        return new BaseResultDto(isSuccess: false, val: Resource.Notification.AccessDenied);

                    long addressId = dto.Id;
                    dto.Id = 0;
                    var newAddress = InsertAsyncDto(dto).Result;
                    _context.ProductOrders.Where(s => s.AddressId == addressId).ExecuteUpdate(s => s.SetProperty(x => x.AddressId, newAddress.Data.Id));
                    DeleteDto(newAddress.Data);
                    dto.Id = addressId;


                    var item = mapper.Map<Address>(dto);
                    item.Deleted = false;
                    _context.Addresses.Attach(item);
                    _context.Entry(item).State = EntityState.Modified;

                    _context.SaveChanges();
                    return new BaseResultDto(isSuccess: true);
                }
            }
            catch (Exception)
            {
                return new BaseResultDto(isSuccess: false, val: Resource.Notification.Unsuccess);
            }
        }
        public AddressSearchDto Search(AddressInputDto searchDto)
        {
            var query = _context.Addresses.Where(s => !s.Deleted).AsQueryable();
            query = query.Where(s => s.UserId == searchDto.UserId);
            if (searchDto.SortBy != Common.Enumerable.SortEnum.Default)
            {
                switch (searchDto.SortBy)
                {
                    case Common.Enumerable.SortEnum.Default:
                        {
                            query = query.OrderByDescending(s => s.Id);
                            break;
                        }
                    case Common.Enumerable.SortEnum.New:
                        {
                            query = query.OrderByDescending(s => s.Id);
                            break;
                        }
                    case Common.Enumerable.SortEnum.Old:
                        {
                            query = query.OrderBy(s => s.Id);
                            break;
                        }
                    default:
                        break;
                }
            }

            return new AddressSearchDto(searchDto, query, mapper);
        }

        public AdminAddressSearchDto SearchAdmin(AdminAddressInputDto searchDto)
        {
            var query = _context.Addresses
                .AsNoTracking()
                .Where(address => !address.Deleted)
                .Include(address => address.User)
                .Include(address => address.City)
                    .ThenInclude(city => city.State)
                .AsQueryable();

            if (searchDto.UserId.HasValue)
                query = query.Where(address => address.UserId == searchDto.UserId.Value);

            if (searchDto.CityId.HasValue)
                query = query.Where(address => address.CityId == searchDto.CityId.Value);

            if (searchDto.StateId.HasValue)
                query = query.Where(address => address.City.StateId == searchDto.StateId.Value);

            if (!string.IsNullOrWhiteSpace(searchDto.PostalCode))
            {
                var postalCode = searchDto.PostalCode.Trim();
                query = query.Where(address =>
                    address.PostalCode != null &&
                    address.PostalCode.Contains(postalCode));
            }

            if (!string.IsNullOrWhiteSpace(searchDto.Q))
            {
                var q = searchDto.Q.Trim();
                query = query.Where(address =>
                    (address.Name != null && address.Name.Contains(q)) ||
                    (address.FirstName != null && address.FirstName.Contains(q)) ||
                    (address.LastName != null && address.LastName.Contains(q)) ||
                    (address.Phone != null && address.Phone.Contains(q)) ||
                    (address.Mobile != null && address.Mobile.Contains(q)) ||
                    (address.AddressValue != null && address.AddressValue.Contains(q)) ||
                    (address.PostalCode != null && address.PostalCode.Contains(q)) ||
                    (address.NationalCode != null && address.NationalCode.Contains(q)) ||
                    (address.User.FirstName != null && address.User.FirstName.Contains(q)) ||
                    (address.User.LastName != null && address.User.LastName.Contains(q)) ||
                    (address.User.Mobile != null && address.User.Mobile.Contains(q)) ||
                    (address.User.Email != null && address.User.Email.Contains(q)) ||
                    (address.City.Name != null && address.City.Name.Contains(q)) ||
                    (address.City.State.Name != null && address.City.State.Name.Contains(q)));
            }

            query = searchDto.SortBy switch
            {
                Common.Enumerable.SortEnum.Old => query.OrderBy(address => address.Id),
                _ => query.OrderByDescending(address => address.Id)
            };

            return new AdminAddressSearchDto(searchDto, query, mapper);
        }
    }
}
