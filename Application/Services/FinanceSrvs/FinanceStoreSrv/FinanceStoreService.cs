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
                    .Where(o => !o.Deleted && o.IsPaid)
                    .Where(o => o.ProductOrderStores.Any(s => s.StoreId == dto.StoreId && !s.Deleted))
                    .Include(o => o.User)
                    .Include(o => o.ProductOrderStatus)
                    .AsQueryable();

                if (dto.Permitted.HasValue)
                    baseQ = baseQ.Where(o => o.Permitted == dto.Permitted.Value);

                if (dto.UserDelivery.HasValue)
                {
                    var sendLabel = Application.Common.Enumerable.ProductOrderStatusEnum.ProductOrderStatus_Send.ToString();
                    baseQ = dto.UserDelivery.Value switch
                    {
                        0 => baseQ.Where(o => o.IsPaid && o.UserReceived == null && o.ProductOrderStatus.Label == sendLabel),
                        1 => baseQ.Where(o => o.UserReceived == true),
                        2 => baseQ.Where(o => o.UserReceived == false),
                        _ => baseQ
                    };
                }

                var list = baseQ
                    .OrderByDescending(o => o.CreateDate)
                    .Select(o => new FinanceProductOrderVDto
                    {
                        ProductOrderId = o.Id,
                        OrderCode = o.OrderCode,
                        BuyerFullName = o.User != null ? ((o.User.FirstName ?? "") + " " + (o.User.LastName ?? "")).Trim() : "",
                        PaymentPrice = o.PaymentPrice,
                        GoodsPrice = o.ProductOrderStores.Where(s => !s.Deleted).Sum(s => s.PaymentPrice - s.DeliveryPrice),
                        DeliveryPrice = o.ProductOrderStores.Where(s => !s.Deleted).Sum(s => s.DeliveryPrice),
                        CommissionBase = o.ProductOrderStores.Where(s => !s.Deleted).Sum(s => s.PaymentPrice - s.DeliveryPrice) + o.PastilFundedAmount,
                        CommissionAmount = o.StoreShare != 0 || o.SiteShare != 0
                            ? o.ProductOrderStores.Where(s => !s.Deleted).Sum(s => s.PaymentPrice - s.DeliveryPrice) + o.PastilFundedAmount - o.StoreShare
                            : 0,
                        NetCommissionAmount = o.StoreShare != 0 || o.SiteShare != 0
                            ? o.SiteShare - o.ProductOrderStores.Where(s => !s.Deleted).Sum(s => s.DeliveryPrice)
                            : 0,
                        AccountingReviewRequired = o.IsPaid && (o.StoreShare != 0 || o.SiteShare != 0) && (
                            o.ProductOrderStores.Count(s => !s.Deleted) != 1 ||
                            Math.Abs(o.StoreShare - o.ProductOrderStores.Where(s => !s.Deleted).Sum(s =>
                                (s.PaymentPrice - s.DeliveryPrice + s.ProductOrderItems.Where(i => !i.Deleted && i.Count > 0 && i.PastilDiscountUnit > 0).Sum(i => i.PastilDiscountUnit * i.Count))
                                * (1 - (double)s.Store.CommissionPercent / 100))) > 0.01),

                        CommissionPercent = o.ProductOrderStores
                            .Where(s => s.StoreId == dto.StoreId && !s.Deleted)
                            .Select(s => (decimal?)s.Store.CommissionPercent)
                            .FirstOrDefault() ?? 0,

                        StoreShare = o.StoreShare,
                        SiteShare = o.SiteShare,
                        PastilFundedAmount = o.PastilFundedAmount,
                        StatusLabel = o.ProductOrderStatus != null ? o.ProductOrderStatus.Name : null,
                        Permitted = o.Permitted,
                        UserReceived = o.UserReceived,
                        UserReceivedDate = o.UserReceivedDate,
                        UserReceivedAuto = o.UserReceivedAuto,
                        UserReceiveNote = o.UserReceiveNote
                    })
                    .ToList();

                var res = new FinanceStoreVDto
                {
                    StoreId = dto.StoreId,
                    ProductOrderCount = list.Count,
                    TotalStoreShare = list.Sum(x => x.StoreShare),
                    TotalSiteShare = list.Sum(x => x.SiteShare),
                    TotalGoodsPrice = list.Sum(x => x.GoodsPrice),
                    TotalDeliveryPrice = list.Sum(x => x.DeliveryPrice),
                    TotalCommissionAmount = list.Sum(x => x.CommissionAmount),
                    TotalNetCommissionAmount = list.Sum(x => x.NetCommissionAmount),
                    AccountingReviewRequiredCount = list.Count(x => x.AccountingReviewRequired),
                    TotalPastilFundedAmount = list.Sum(x => x.PastilFundedAmount),
                    NotReceivedCount = list.Count(x => x.UserReceived == false),
                    ReceivedCount = list.Count(x => x.UserReceived == true),
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
