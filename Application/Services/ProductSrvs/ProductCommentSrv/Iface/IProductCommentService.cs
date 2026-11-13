using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.ProductSrvs.ProductCommentSrv.Dto;
using Entities.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.ProductSrvs.ProductCommentSrv.Iface
{
    public interface IProductCommentService : ICommonSrv<ProductComment, ProductCommentDto>
    {
        Task<BaseResultDto> UpdateDtoAsync(ProductCommentDto dto);
        ProductCommentSearchDto Search(ProductCommentInputDto baseSearchDto);
        Task UpdateProductCommentRateAsync(long Id);
        /// <summary>وضعیت امکان ثبت دیدگاه یک کاربر برای یک کالا.</summary>
        Task<ProductCommentEligibilityVDto> GetEligibilityAsync(long userId, long productId);
        /// <summary>وضعیت امکان ثبت دیدگاه برای همه‌ی کالاهای یک سفارش (فقط سفارش خودِ کاربر؛ سفارش تحویل‌نگرفته = لیست خالی).</summary>
        Task<List<ProductCommentEligibilityVDto>> GetOrderEligibilityAsync(long userId, string orderId);

    }
}
