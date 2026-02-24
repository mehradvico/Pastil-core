using AngleSharp.Dom;
using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Common.Service;
using Application.Services.CompanionSrv.CompanionReserveSrv.Iface;
using Application.Services.FinanceSrvs.SettlementCompanionSrv.Dto;
using Application.Services.FinanceSrvs.SettlementCompanionSrv.Iface;
using Application.Services.FinanceSrvs.SettlementSrv.Dto;
using Application.Services.FinanceSrvs.SettlementSrv.Iface;
using Application.Services.FinanceSrvs.SettlementStoreSrv.Dto;
using Application.Services.FinanceSrvs.SettlementStoreSrv.Iface;
using Application.Services.Order.ProductOrderSrv.Iface;
using Application.Services.PansionSrvs.PansionReserveSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.SettlementSrv
{
    public class SettlementService : CommonSrv<Settlement, SettlementDto>, ISettlementService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ISettlementCompanionService _settlementCompanionService; 
        private readonly ISettlementStoreService _settlementStoreService;
        private readonly IProductOrderService _productOrderService;
        private readonly ICompanionReserveService _companionReserveService;
        private readonly IPansionReserveService _pansionReserveService;
        public SettlementService(IDataBaseContext _context, IProductOrderService productOrderService, ICompanionReserveService companionReserveService, IPansionReserveService pansionReserveService,ISettlementStoreService settlementStoreService, ISettlementCompanionService settlementCompanionService, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this._settlementStoreService = settlementStoreService;
            this._settlementCompanionService = settlementCompanionService;
            this._productOrderService = productOrderService;
            this._pansionReserveService = pansionReserveService;
            this._companionReserveService = companionReserveService;
        }

        public async Task<BaseResultDto<SettlementVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.Settlements.Include(s => s.UserBankCard).Include(s => s.Companion).Include(s => s.Store).FirstOrDefaultAsync(s => s.Id == id);
            if (item != null)
            {
                return new BaseResultDto<SettlementVDto>(true, mapper.Map<SettlementVDto>(item));
            }
            return new BaseResultDto<SettlementVDto>(false, mapper.Map<SettlementVDto>(item));
        }

        public SettlementSearchDto Search(SettlementInputDto baseSearchDto)
        {
            var model = _context.Settlements.Include(s => s.UserBankCard).Include(s => s.Companion).Include(s => s.Store).AsQueryable();

            if (baseSearchDto.StoreId.HasValue)
            {
                model = model.Where(s => s.StoreId == baseSearchDto.StoreId.Value);
            }
            if (baseSearchDto.CompanionId.HasValue)
            {
                model = model.Where(s => s.CompanionId == baseSearchDto.CompanionId.Value);
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
            return new SettlementSearchDto(baseSearchDto, model, mapper);
        }

        public override async Task<BaseResultDto<SettlementDto>> InsertAsyncDto(SettlementDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<SettlementDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                    return modelCheker;

                var hasStore = dto.StoreId.HasValue && dto.StoreId.Value != 0;
                var hasCompanion = dto.CompanionId.HasValue && dto.CompanionId.Value != 0;

                if (hasStore == hasCompanion)
                    return new BaseResultDto<SettlementDto>(false, Resource.Notification.PleaseSpecifyFirstVariationValue, dto);

                var item = mapper.Map<Settlement>(dto);
                item.CreateDate = DateTime.Now;
                item.PaidPrice = 0;
                item.ItemCount = 0;

                await _context.Settlements.AddAsync(item);
                await _context.SaveChangesAsync();

                var createdStoreLinks = new List<long>();
                var createdCompanionLinks = new List<long>();

                double paidPrice = 0;
                long itemCount = 0;

                try
                {
                    if (hasStore)
                    {
                        var orders = _context.ProductOrders.Where(o => !o.Deleted).Where(o => o.ProductOrderStores.Any(s => s.StoreId == dto.StoreId.Value)).Where(o => !o.Permitted).ToList();
                        foreach (var o in orders)
                        {
                            var linkRes = await _settlementStoreService.InsertAsyncDto(new SettlementStoreDto
                            {
                                SettlementId = item.Id,
                                ProductOrderId = o.Id
                            });

                            if (!linkRes.IsSuccess)
                                throw new Exception("SettlementStore insert failed");

                            createdStoreLinks.Add(linkRes.Data.Id);

                            var permittedRes = await _productOrderService.UpdatePermittedAsyncDto(o.Id);
                            if (!permittedRes.IsSuccess)
                                throw new Exception("ProductOrder permitted update failed");

                            paidPrice += o.StoreShare;
                            itemCount++;
                        }
                    }

                    if (hasCompanion)
                    {
                        var companionReserves = _context.CompanionReserves.Include(r => r.CompanionAssistance).Where(r => r.IsReserved && !r.IsCancel).Where(r => r.CompanionAssistance.CompanionId == dto.CompanionId.Value).Where(r => !r.Permitted).ToList();

                        foreach (var r in companionReserves)
                        {
                            var linkRes = await _settlementCompanionService.InsertAsyncDto(new SettlementCompanionDto
                            {
                                SettlementId = item.Id,
                                CompanionReserveId = r.Id
                            });

                            if (!linkRes.IsSuccess)
                                throw new Exception("CompanionReserve link failed");

                            createdCompanionLinks.Add(linkRes.Data.Id);

                            var permittedRes = await _companionReserveService.UpdatePermittedAsyncDto(r.Id);
                            if (!permittedRes.IsSuccess)
                                throw new Exception("CompanionReserve permitted update failed");

                            paidPrice += r.CompanionShare;
                            itemCount++;
                        }

                        var pansionReserves = _context.PansionReserves.Include(r => r.Pansion).Where(r => r.IsReserved && !r.IsCancel).Where(r => r.Pansion.CompanionId == dto.CompanionId.Value).Where(r => !r.Permitted).ToList();

                        foreach (var r in pansionReserves)
                        {
                            var linkRes = await _settlementCompanionService.InsertAsyncDto(new SettlementCompanionDto
                            {
                                SettlementId = item.Id,
                                PansionReserveId = r.Id
                            });

                            if (!linkRes.IsSuccess)
                                throw new Exception("PansionReserve link failed");

                            createdCompanionLinks.Add(linkRes.Data.Id);

                            var permittedRes = await _pansionReserveService.UpdatePermittedAsyncDto(r.Id);
                            if (!permittedRes.IsSuccess)
                                throw new Exception("PansionReserve permitted update failed");

                            paidPrice += r.CompanionShare;
                            itemCount++;
                        }
                    }

                    item.PaidPrice = paidPrice;
                    item.ItemCount = itemCount;

                    _context.Settlements.Update(item);
                    await _context.SaveChangesAsync();

                    return new BaseResultDto<SettlementDto>(true, mapper.Map<SettlementDto>(item));
                }
                catch
                {
                    if (createdStoreLinks.Any())
                    {
                        var links = _context.SettlementStores.Where(x => createdStoreLinks.Contains(x.Id)).ToList();
                        _context.SettlementStores.RemoveRange(links);
                        await _context.SaveChangesAsync();
                    }

                    if (createdCompanionLinks.Any())
                    {
                        var links = _context.SettlementCompanions.Where(x => createdCompanionLinks.Contains(x.Id)).ToList();
                        _context.SettlementCompanions.RemoveRange(links);
                        await _context.SaveChangesAsync();
                    }

                    var createdSettlement = await _context.Settlements.FindAsync(item.Id);
                    if (createdSettlement != null)
                    {
                        _context.Settlements.Remove(createdSettlement);
                        await _context.SaveChangesAsync();
                    }

                    return new BaseResultDto<SettlementDto>(false, Resource.Notification.OperationFailed, dto);
                }
            }
            catch (Exception ex)
            {
                return new BaseResultDto<SettlementDto>(false, ex.Message, dto);
            }
        }
    }
}
