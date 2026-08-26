using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.ProductSrvs.StoreSrv.Dto;
using Application.Services.StoreSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Seller.Controllers
{
    /// <summary>
    /// مدیریت فروشنده ها
    /// </summary>
    [Area("Seller")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class StoreController : ControllerBase
    {
        private readonly IStoreService StoreService;
        private readonly ICurrentUserHelper _currentUser;
        public StoreController(IStoreService StoreService, ICurrentUserHelper currentUser)
        {
            this.StoreService = StoreService;
            this._currentUser = currentUser;
        }

        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        ///

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<StoreDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var Store = await StoreService.FindAsyncDto(id);
            if (Store.IsSuccess && Store.Data?.Id != _currentUser.CurrentUser.StoreId)
                return Ok(new BaseResultDto<StoreDto>(false, Resource.Notification.AccessDenied, default));
            return Ok(Store);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>
        ///
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<StoreDto>), 200)]
        public async Task<IActionResult> Put(StoreDto StoreDto)
        {
            var currentStoreId = _currentUser.CurrentUser.StoreId;
            var existing = await StoreService.FindAsyncDto(currentStoreId);
            if (!existing.IsSuccess || existing.Data == null)
                return Ok(new BaseResultDto<StoreDto>(false, Resource.Notification.AccessDenied, default));

            StoreDto.Id = currentStoreId;
            StoreDto.ReferralCode = existing.Data.ReferralCode;
            StoreDto.MaxDiscountPercent = existing.Data.MaxDiscountPercent;
            StoreDto.RateAvg = existing.Data.RateAvg;
            StoreDto.RateCount = existing.Data.RateCount;
            StoreDto.Active = existing.Data.Active;
            StoreDto.Approved = existing.Data.Approved;
            StoreDto.ApprovalValue = existing.Data.ApprovalValue;
            StoreDto.ShowToSite = existing.Data.ShowToSite;

            var item = StoreService.UpdateDto(StoreDto);
            return Ok(item);
        }
    }
}
