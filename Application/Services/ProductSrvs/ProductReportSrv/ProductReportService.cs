using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Common.Service;
using Application.Services.ProductSrvs.ProductReportSrv.Dto;
using Application.Services.ProductSrvs.ProductReportSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.ProductSrvs.ProductReportSrv
{
    public class ProductReportService : CommonSrv<ProductReport, ProductReportDto>, IProductReportService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        public ProductReportService(IDataBaseContext _context, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }
        public async Task<BaseResultDto<ProductReportVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.ProductReports.Include(s => s.Product).Include(s => s.User).FirstOrDefaultAsync(s => s.Id == id);
            if (item != null)
            {
                return new BaseResultDto<ProductReportVDto>(true, mapper.Map<ProductReportVDto>(item));
            }
            return new BaseResultDto<ProductReportVDto>(false, mapper.Map<ProductReportVDto>(item));
        }

        public ProductReportSearchDto Search(ProductReportInputDto baseSearchDto)
        {
            var model = _context.ProductReports.Include(s => s.Product).Include(s => s.User).AsQueryable();

            if (baseSearchDto.ProductId.HasValue)
            {
                model = model.Where(s => s.ProductId == baseSearchDto.ProductId.Value);
            }
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
            return new ProductReportSearchDto(baseSearchDto, model, mapper);
        }

        public override async Task<BaseResultDto<ProductReportDto>> InsertAsyncDto(ProductReportDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<ProductReportDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    var item = mapper.Map<ProductReport>(dto);
                    bool existed = await _context.ProductReports.AnyAsync(s => s.ProductId == item.ProductId && s.UserId == item.UserId);
                    if (existed)
                    {
                        return new BaseResultDto<ProductReportDto>(false, val1: Resource.Notification.YourReportHasBeenSubmitedForThisProductBefore, val2: nameof(dto.ProductId), data: dto);
                    }
                    item.CreateDate = DateTime.Now;
                    await _context.ProductReports.AddAsync(item);
                    await _context.SaveChangesAsync();
                    return new BaseResultDto<ProductReportDto>(true, mapper.Map<ProductReportDto>(item));
                }

            }
            catch (Exception ex)
            {
                return new BaseResultDto<ProductReportDto>(isSuccess: false, val: Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex), data: dto);
            }
        }
    }
}
