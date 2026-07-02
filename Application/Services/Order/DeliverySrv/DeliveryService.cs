using AngleSharp.Dom;
using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Helpers;
using Application.Common.Service;
using Application.Services.Order.DeliverySrv.Dto;
using Application.Services.Order.DeliverySrv.iface;
using Application.Services.Setting.BaseDetailSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using Persistence.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.Content.DeliverySrv
{
    public class DeliveryService : CommonSrv<Delivery, DeliveryDto>, IDeliveryService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly IBaseDetailService _baseDetailService;
        public DeliveryService(IDataBaseContext _context, IMapper mapper, IBaseDetailService baseDetailService) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this._baseDetailService = baseDetailService;
        }


        public DeliverySearchDto Search(DeliveryInputDto searchDto)
        {
            var query = _context.Deliveries.Include(s => s.DeliveryType).Include(s => s.City).Include(s => s.State).Include(s => s.Store).Where(s => !s.Deleted).AsQueryable();
            if (searchDto.StoreId.HasValue)
            {
                query = query.Where(s => s.StoreId == searchDto.StoreId.Value);
            }
            if (searchDto.DeliveryTypeEnum.HasValue)
            {
                query = query.Where(s => s.DeliveryType.Label == searchDto.DeliveryTypeEnum.ToString());
            }
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

            return new DeliverySearchDto(searchDto, query, mapper);
        }

        public virtual async Task<BaseResultDto<DeliveryVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.Deliveries
                .Include(s => s.DeliveryType)
                .Include(s => s.City)
                    .ThenInclude(s => s.State)
                .Include(s => s.State)
                .Include(s => s.Store)
                    .ThenInclude(s => s.Picture)
                .FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);
            if (item != null)
                return new BaseResultDto<DeliveryVDto>(true, mapper.Map<DeliveryVDto>(item));
            return new BaseResultDto<DeliveryVDto>(false, mapper.Map<DeliveryVDto>(item));
        }

        public BaseResultDto GetDeliveries(Cart cart, long? storeId)
        {
            var result = new List<DeliveryResultVDto>();
            var deliveries = _context.Deliveries.Include(s => s.DeliveryDistance).Include(s => s.DeliveryType).Where(s => s.StoreId == storeId && s.Deleted == false && s.Active).AsQueryable();
            if (cart.AddressId.HasValue)
            {
                deliveries = deliveries.Where(s => s.CityId == cart.Address.CityId || (s.CityId == null && s.StateId == cart.Address.City.StateId) || (s.CityId == null && s.StateId == null));
            }
            else
            {
                deliveries = deliveries.Where(s => s.DeliveryType.Label == DeliveryTypeEnum.DeliveryType_InStore.ToString());
            }
            foreach (var item in deliveries)
            {
                var newItem = GetDelivery(cart, item, storeId);
                if (newItem != null)
                {
                    result.Add(newItem);
                }
            }
            result = result.OrderBy(s => s.DeliveryPrice).ToList();
            return new BaseResultDto<List<DeliveryResultVDto>>(result.Any(), result);
        }
        public DeliveryResultVDto GetDelivery(Cart cart, long deliveryId, long? storeId)
        {
            var delivery = _context.Deliveries.Include(s => s.DeliveryDistance).Include(s => s.DeliveryType).FirstOrDefault(s => s.Id == deliveryId && s.StoreId == storeId);
            if (delivery != null)
            {
                return GetDelivery(cart, delivery, storeId);
            }
            else
            {
                return null;
            }
        }

        public DeliveryResultVDto GetDelivery(Cart cart, Delivery delivery, long? storeId)
        {
            var cartStore = cart.CartStores.FirstOrDefault(s => s.StoreId == storeId);
            if (cartStore == null)
            {
                return null;
            }
            var newItem = mapper.Map<DeliveryResultVDto>(delivery);
            newItem.DeliveryPrice = delivery.BasePrice;
            bool isFree = false;
            if (delivery.MinPriceForFree > 0 && delivery.MinCountForFree > 0 && delivery.MinCountForFree <= cart.ItemCount && cartStore.Price >= delivery.MinPriceForFree)
            {
                newItem.DeliveryPrice = 0;
                isFree = true;
            }
            if (isFree == false && delivery.DeliveryDistance != null && delivery.DeliveryDistance.Any() && cart.Address.Location != null)
            {
                var storeLocation = mapper.Map<Point>(_baseDetailService.GetBaseDetails().Location);
                var distance = storeLocation.Distance(cart.Address.Location);
                var distanceItem = delivery.DeliveryDistance.FirstOrDefault(s => s.FromD <= distance && s.ToD >= distance);
                if (distanceItem != null)
                {
                    newItem.DeliveryPrice = distanceItem.Price;
                }
                else
                {
                    return null;
                }
            }
            else if (isFree == false)
            {
                newItem.DeliveryPrice = delivery.BasePrice;
            }

            if (newItem.DeliveryPrice > 0)
            {
                newItem.DeliveryPriceString = newItem.DeliveryPrice.ToCurency();
            }
            else if (isFree == false && delivery.AfterRent)
            {
                newItem.DeliveryPriceString = Resource.Lang.AfterRent;
            }
            else
            {
                newItem.DeliveryPriceString = Resource.Lang.Free;
            }
            if (newItem.MaxDays > 0)
            {
                newItem.MaxDaysString = string.Format(Resource.Pattern.DeliveryDays, newItem.MaxDays);
            }
            else
            {
                newItem.MaxDaysString = Resource.Lang.ImmediateDelivery;
            }
            if (delivery.MinCountForFree > 0 && delivery.MinPriceForFree > 0)
            {
                newItem.MinPriceForFreeString = $"رایگان برای خرید بیشتر از {delivery.MinPriceForFree.ToCurency()} و تعداد {delivery.MinCountForFree} قلم جنس و بیشتر";
            }
            else if (delivery.MinPriceForFree > 0)
            {
                newItem.MinPriceForFreeString = $"رایگان برای خرید بیشتر از {delivery.MinPriceForFree.ToCurency()}";
            }
            else if (delivery.MinCountForFree > 0)
            {
                newItem.MinPriceForFreeString = $"رایگان برای خرید بیشتر از {delivery.MinCountForFree} قلم جنس";
            }
            return newItem;
        }


    }
}
