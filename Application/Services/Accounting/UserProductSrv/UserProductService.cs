using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Services.Accounting.UserProductSrv.Dto;
using Application.Services.Accounting.UserProductSrv.Iface;
using AutoMapper;
using Dapper;
using Entities.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.Accounting.UserProductSrv
{
    public class UserProductService : CommonSrv<UserProduct, UserProductDto>, IUserProductService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;

        private readonly string connectionString;

        public UserProductService(IDataBaseContext _context, IMapper mapper, IConfiguration config, ICurrentUserHelper currentUserHelper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;

            connectionString = config.GetValue<string>(
                        "connection");
        }
        public UserProductSearchDto SearchDto(UserProductInputDto dto)
        {
            var model = _context.UserProducts.Include(s => s.Product).ThenInclude(s => s.Picture).Include(s => s.User).AsQueryable();
            if (dto.UserId.HasValue)
            {
                model = model.Where(s => s.UserId.Equals(dto.UserId));
            }
            if (dto.ProductId.HasValue)
            {
                model = model.Where(s => s.ProductId.Equals(dto.ProductId));
            }

            if (!string.IsNullOrEmpty(dto.Q))
            {
                model = model.Where(s => s.Product.Name.Contains(dto.Q));
            }
            return new UserProductSearchDto(dto, model, mapper);
        }
        public async Task<bool> UserHasProductAsync(long productId, long userId)
        {
            return await _context.UserProducts.AnyAsync(s => s.ProductId == productId && s.UserId == userId);
        }
        public override Task<BaseResultDto<UserProductDto>> InsertAsyncDto(UserProductDto dto)
        {
            dto.Id = 0;
            dto.CreateDate = DateTime.Now;
            return base.InsertAsyncDto(dto);
        }
        public async Task<BaseResultDto> InsertOrderItemAsyncDto(ProductOrder productOrder)
        {
            foreach (var item in productOrder.ProductOrderStores.SelectMany(s => s.ProductOrderItems.Where(w => w.ProductItem.Product.TypeId == (long)ProductTypeEnum.ProductType_Media)))
            {
                var dto = new UserProductDto();
                dto.UserId = productOrder.UserId;
                dto.ProductId = item.ProductItem.ProductId;
                dto.Price = item.Price;
                dto.CreateDate = DateTime.Now;
                var userProduct = await InsertAsyncDto(dto);

            }
            return new BaseResultDto(true);
        }
        public async Task<BaseResultDto<long>> UserProductCountAsync()
        {
            string sqlQuery = $"SELECT Count(0) From UserProducts";
            var connection = new SqlConnection(connectionString);
            var count = await connection.QueryAsync<long>(sqlQuery);
            return new BaseResultDto<long>(true, count.Single());
        }

    }
}
