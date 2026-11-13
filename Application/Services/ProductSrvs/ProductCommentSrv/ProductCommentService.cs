using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Helpers;
using Application.Common.Service;
using Application.Services.ProductSrvs.ProductCommentSrv.Dto;
using Application.Services.ProductSrvs.ProductCommentSrv.Iface;
using Application.Services.Setting.CodeSrv.Iface;
using AutoMapper;
using Dapper;
using Entities.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.ProductSrvs.ProductCommentSrv
{
    public class ProductCommentService : CommonSrv<ProductComment, ProductCommentDto>, IProductCommentService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ICodeService codeService;
        private readonly string connectionString;


        public ProductCommentService(IDataBaseContext _context, IConfiguration config, IMapper mapper, ICodeService codeService) : base(_context, mapper)
        {
            this.codeService = codeService;
            this._context = _context;
            this.mapper = mapper;
            this.connectionString = config.GetValue<string>(
            "connection");
        }

        public override async Task<BaseResultDto<ProductCommentDto>> InsertAsyncDto(ProductCommentDto dto)
        {
            try
            {
                //dto.Text = await SanitizeTextHelper.ToSanitizeAsync(dto.Text);
                var modelCheker = ModelHelper<ProductCommentDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    if (dto.Rate.HasValue && (dto.Rate.Value > 5 || dto.Rate.Value < 1))
                    {
                        return new BaseResultDto<ProductCommentDto>(false, val1: Resource.Notification.TheRangeEnteredIsNotCorrect, val2: nameof(dto.Rate), data: dto);
                    }
                    // فقط خریداری که سفارشِ حاوی این کالا را تحویل گرفته می‌تواند دیدگاه بگذارد (یک دیدگاه برای هر کالا)
                    if (!dto.UserId.HasValue || dto.ProductId <= 0)
                        return new BaseResultDto<ProductCommentDto>(false, val: Resource.Notification.AccessDenied, data: dto);
                    var eligibility = await GetEligibilityAsync(dto.UserId.Value, dto.ProductId);
                    if (!eligibility.Received)
                        return new BaseResultDto<ProductCommentDto>(false, val: Resource.Notification.ProductCommentNotPurchased, data: dto);
                    if (eligibility.HasCommented)
                        return new BaseResultDto<ProductCommentDto>(false, val: Resource.Notification.ProductCommentAlreadySubmitted, data: dto);
                    var item = mapper.Map<ProductComment>(dto);
                    item.Id = 0;
                    item.LikeCount = 0;
                    item.DisLikeCount = 0;
                    var commentStatus = await codeService.GetByLabelAsync(CommentEnum.Comment_NotChecked.ToString());
                    if (commentStatus != null)
                    {
                        item.StatusId = commentStatus.Id;
                        item.CreateDate = DateTime.Now;
                        item.Answer = null;
                        //item.UserId = _currentUserHelper.CurrentUser.UserId;
                        await _context.ProductComments.AddAsync(item);
                        _context.SaveChanges();
                        return new BaseResultDto<ProductCommentDto>(true, mapper.Map<ProductCommentDto>(item));
                    }
                    return new BaseResultDto<ProductCommentDto>(false, val: Resource.Notification.Unsuccess, data: dto);
                }
            }
            catch
            {
                return new BaseResultDto<ProductCommentDto>(false, val: Resource.Notification.Unsuccess, data: dto);
            }
        }
        public async Task<BaseResultDto> UpdateDtoAsync(ProductCommentDto dto)
        {

            try
            {

                var item = _context.ProductComments.Find(dto.Id);
                item.Answer = dto.Answer;
                item.StatusId = dto.StatusId;
                _context.ProductComments.Update(item);
                await _context.SaveChangesAsync();
                await UpdateProductCommentRateAsync(item.ProductId);
                return new BaseResultDto(isSuccess: true);

            }
            catch (Exception ex)
            {
                return new BaseResultDto(isSuccess: false, val: Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex));
            }
        }
        public ProductCommentSearchDto Search(ProductCommentInputDto baseSearchDto)
        {
            var model = BaseSaerch(baseSearchDto);
            return new ProductCommentSearchDto(baseSearchDto, model, mapper);
        }

        private IQueryable<ProductComment> BaseSaerch(ProductCommentInputDto searchDto)
        {
            var query = _context.ProductComments.Include(s => s.Product).Include(s => s.User).AsQueryable();
            if (searchDto.ProductId.HasValue)
                query = query.Where(s => s.ProductId == searchDto.ProductId);
            if (searchDto.UserId.HasValue)
                query = query.Include(s => s.CommentLikes.Where(a => a.UserId == searchDto.UserId)).Where(s => s.UserId == searchDto.UserId);
            if (searchDto.AllStatus == false)
            {
                switch (searchDto.Available)
                {
                    case null:
                        {
                            query = query.Where(s => s.Status.Label == CommentEnum.Comment_NotChecked.ToString());
                            break;
                        }
                    case true:
                        {
                            query = query.Where(s => s.Status.Label == CommentEnum.Comment_Accept.ToString());
                            break;
                        }
                    case false:
                        {
                            query = query.Where(s => s.Status.Label == CommentEnum.Comment_Reject.ToString());
                            break;
                        }
                    default:
                }
            }

            switch (searchDto.SortBy)
            {

                case Common.Enumerable.SortEnum.Default:
                    {
                        query = query.OrderBy(s => s.StatusId).ThenByDescending(s => s.Id);
                        break;
                    }
                case Common.Enumerable.SortEnum.New:
                    {
                        query = query.OrderBy(s => s.StatusId).ThenByDescending(s => s.Id);
                        break;
                    }
                case Common.Enumerable.SortEnum.Old:
                    {
                        query = query.OrderBy(s => s.StatusId).ThenBy(s => s.Id);
                        break;
                    }

                default:
                    query = query.OrderBy(s => s.StatusId).ThenByDescending(s => s.Id);

                    break;
            }

            return query;
        }

        // سفارش «تحویل‌شده»: پرداخت‌شده، لغو‌نشده و کاربر «تحویل گرفتم» زده (یا خودکار ثبت شده)؛ سفارش‌های قدیمی با وضعیت «تحویل داده شد» هم حساب می‌شوند.
        private IQueryable<ProductOrderItem> ReceivedItems(long userId)
        {
            var deliveredLabel = ProductOrderStatusEnum.ProductOrderStatus_Delivered.ToString();
            var canceledLabel = ProductOrderStateEnum.ProductOrderState_Canceled.ToString();
            return _context.ProductOrderItems.Where(i => !i.Deleted
                && !i.ProductOrderStore.Deleted
                && !i.ProductOrderStore.ProductOrder.Deleted
                && i.ProductOrderStore.ProductOrder.UserId == userId
                && i.ProductOrderStore.ProductOrder.IsPaid
                && i.ProductOrderStore.ProductOrder.ProductOrderState.Label != canceledLabel
                && (i.ProductOrderStore.ProductOrder.UserReceived == true
                    || i.ProductOrderStore.ProductOrder.ProductOrderStatus.Label == deliveredLabel));
        }

        public async Task<ProductCommentEligibilityVDto> GetEligibilityAsync(long userId, long productId)
        {
            var received = await ReceivedItems(userId).AnyAsync(i => i.ProductItem.ProductId == productId);
            var rejected = CommentEnum.Comment_Reject.ToString();
            var commented = await _context.ProductComments.AnyAsync(c => c.UserId == userId && c.ProductId == productId && c.Status.Label != rejected);
            return new ProductCommentEligibilityVDto { ProductId = productId, Received = received, HasCommented = commented, CanComment = received && !commented };
        }

        public async Task<List<ProductCommentEligibilityVDto>> GetOrderEligibilityAsync(long userId, string orderId)
        {
            var productIds = await ReceivedItems(userId)
                .Where(i => i.ProductOrderStore.ProductOrderId == orderId)
                .Select(i => i.ProductItem.ProductId).Distinct().ToListAsync();
            if (productIds.Count == 0)
                return new List<ProductCommentEligibilityVDto>();
            var rejected = CommentEnum.Comment_Reject.ToString();
            var commentedIds = await _context.ProductComments
                .Where(c => c.UserId == userId && productIds.Contains(c.ProductId) && c.Status.Label != rejected)
                .Select(c => c.ProductId).Distinct().ToListAsync();
            return productIds.Select(id => new ProductCommentEligibilityVDto
            {
                ProductId = id,
                Received = true,
                HasCommented = commentedIds.Contains(id),
                CanComment = !commentedIds.Contains(id)
            }).ToList();
        }

        public async Task UpdateProductCommentRateAsync(long Id)
        {
            var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("UpdateProductComments", new { FilterIds = Id }, commandType: System.Data.CommandType.StoredProcedure);
        }
    }
}
