using Application.Common.Dto.Result;
using Application.Common.Enumerable.Code;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.ProductSrvs.ProductStockAlertSrv.Dto;
using Application.Services.ProductSrvs.ProductStockAlertSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.ProductSrvs.ProductStockAlertSrv
{
    public class ProductStockAlertService : IProductStockAlertService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper _mapper;
        private readonly IPushNotificationService _pushNotificationService;

        public ProductStockAlertService(
            IDataBaseContext context,
            IMapper mapper,
            IPushNotificationService pushNotificationService)
        {
            _context = context;
            _mapper = mapper;
            _pushNotificationService = pushNotificationService;
        }

        public ProductStockAlertSearchDto SearchDto(ProductStockAlertInputDto dto)
        {
            var query = _context.ProductStockAlerts
                .AsNoTracking()
                .Include(item => item.Product)
                .Where(item => item.UserId == dto.UserId);

            if (dto.ProductId.HasValue)
                query = query.Where(item => item.ProductId == dto.ProductId.Value);

            return new ProductStockAlertSearchDto(dto, query.OrderByDescending(item => item.Id), _mapper);
        }

        public async Task<BaseResultDto<ProductStockAlertDto>> SubscribeAsync(ProductStockAlertDto dto)
        {
            if (dto.ProductId <= 0 || dto.UserId <= 0)
                return new BaseResultDto<ProductStockAlertDto>(false, "شناسه محصول معتبر نیست.", dto);

            var productExists = await _context.Products.AnyAsync(product =>
                product.Id == dto.ProductId && product.Active && !product.Deleted);
            if (!productExists)
                return new BaseResultDto<ProductStockAlertDto>(false, Resource.Notification.NothingFound, dto);

            var alert = await _context.ProductStockAlerts
                .Where(item => item.UserId == dto.UserId && item.ProductId == dto.ProductId)
                .OrderByDescending(item => item.Id)
                .FirstOrDefaultAsync();

            if (alert == null)
            {
                alert = new ProductStockAlert
                {
                    UserId = dto.UserId,
                    ProductId = dto.ProductId,
                    IsActive = true,
                    CreateDate = DateTime.Now
                };
                await _context.ProductStockAlerts.AddAsync(alert);
            }
            else
            {
                alert.IsActive = true;
                alert.NotifiedDate = null;
                alert.NotifiedStoreId = null;
                alert.CreateDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return new BaseResultDto<ProductStockAlertDto>(true, _mapper.Map<ProductStockAlertDto>(alert));
        }

        public async Task<BaseResultDto> UnsubscribeAsync(ProductStockAlertDto dto)
        {
            var alerts = await _context.ProductStockAlerts
                .Where(item => item.UserId == dto.UserId && item.ProductId == dto.ProductId && item.IsActive)
                .ToListAsync();

            if (alerts.Count == 0)
                return new BaseResultDto(false, Resource.Notification.NothingFound);

            foreach (var alert in alerts)
                alert.IsActive = false;

            await _context.SaveChangesAsync();
            return new BaseResultDto(true, Resource.Notification.Success);
        }

        public async Task NotifyRestockedAsync(long productId, long storeId)
        {
            if (productId <= 0 || storeId <= 0)
                return;

            var product = await _context.Products
                .AsNoTracking()
                .Where(item => item.Id == productId)
                .Select(item => new { item.Id, item.Name })
                .FirstOrDefaultAsync();
            var store = await _context.Stores
                .AsNoTracking()
                .Where(item => item.Id == storeId)
                .Select(item => new { item.Id, item.Name })
                .FirstOrDefaultAsync();

            if (product == null || store == null)
                return;

            var alerts = await _context.ProductStockAlerts
                .Include(item => item.User)
                .Where(item => item.ProductId == productId && item.IsActive)
                .ToListAsync();
            if (alerts.Count == 0)
                return;

            foreach (var alert in alerts)
            {
                alert.IsActive = false;
                alert.NotifiedDate = DateTime.Now;
                alert.NotifiedStoreId = storeId;
            }

            await _context.SaveChangesAsync();

            foreach (var alert in alerts)
            {
                await _pushNotificationService.SendPushAsync(
                    PushTypeEnum.PushProductStockAvailable,
                    alert.UserId,
                    token1: alert.User?.FirstName,
                    token2: product.Name,
                    token3: store.Name,
                    token4: product.Id.ToString());
            }
        }
    }
}
