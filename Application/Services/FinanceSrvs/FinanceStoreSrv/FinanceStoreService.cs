using Application.Common.Dto.Result;
using Application.Services.FinanceSrvs.FinanceStoreSrv.Dto;
using Application.Services.FinanceSrvs.FinanceStoreSrv.Iface;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.FinanceSrvs.FinanceStoreSrv
{
    public class FinanceStoreService : Iface.IFinanceStoreService
    {
        private readonly IDataBaseContext _context;

        public FinanceStoreService(IDataBaseContext context)
        {
            _context = context;
        }

        public BaseResultDto<FinanceStoreVDto> Search(FinanceStoreInputDto dto)
        {
            try
            {
                if (dto == null)
                    return new BaseResultDto<FinanceStoreVDto>(false, null);

                if (dto.StoreId <= 0)
                    return new BaseResultDto<FinanceStoreVDto>(false, null);

                var baseQ = _context.ProductOrders
                    .AsNoTracking()
                    .Where(o => !o.Deleted)
                    .Where(o => o.ProductOrderStores.Any(s => s.StoreId == dto.StoreId))
                    .Include(o => o.User)
                    .Include(o => o.ProductOrderStatus)
                    .AsQueryable();

                if (dto.Permitted.HasValue)
                    baseQ = baseQ.Where(o => o.Permitted == dto.Permitted.Value);

                var list = baseQ
                    .OrderByDescending(o => o.CreateDate)
                    .Select(o => new FinanceProductOrderVDto
                    {
                        ProductOrderId = o.Id,
                        OrderCode = o.OrderCode,
                        BuyerFullName = o.User != null ? ((o.User.FirstName ?? "") + " " + (o.User.LastName ?? "")).Trim() : "",
                        PaymentPrice = o.PaymentPrice,

                        CommissionPercent = o.ProductOrderStores
                            .Where(s => s.StoreId == dto.StoreId)
                            .Select(s => (decimal?)s.Store.CommissionPercent)
                            .FirstOrDefault() ?? 0,

                        StoreShare = o.StoreShare,
                        SiteShare = o.SiteShare,
                        StatusLabel = o.ProductOrderStatus != null ? o.ProductOrderStatus.Name : null,
                        Permitted = o.Permitted
                    })
                    .ToList();

                var res = new FinanceStoreVDto
                {
                    StoreId = dto.StoreId,
                    ProductOrderCount = list.Count,
                    TotalStoreShare = list.Sum(x => x.StoreShare),
                    TotalSiteShare = list.Sum(x => x.SiteShare),
                    FinanceProductOrders = list ?? new List<FinanceProductOrderVDto>()
                };

                return new BaseResultDto<FinanceStoreVDto>(true, res);
            }
            catch
            {
                return new BaseResultDto<FinanceStoreVDto>(false, null);
            }
        }
    }
}
