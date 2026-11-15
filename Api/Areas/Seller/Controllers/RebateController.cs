using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Order.RebateSrv.Dto;
using Application.Services.Order.RebateSrv.Iface;
using Entities.Entities.PastilClubField;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Seller.Controllers
{
    /// <summary>
    /// کدهای تخفیفِ مخصوص فروشگاه (نماینده برای مشتریان خودش می‌سازد؛ فقط روی سبد همین فروشگاه معتبر است)
    /// </summary>
    [Area("Seller")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class RebateController : ControllerBase
    {
        private readonly IRebateService _rebateService;
        private readonly ICurrentUserHelper _currentUser;

        public RebateController(IRebateService rebateService, ICurrentUserHelper currentUser)
        {
            _rebateService = rebateService;
            _currentUser = currentUser;
        }

        /// <summary>
        /// لیست کدهای تخفیف فروشگاه من (UserId = فقط کدهای مخصوص یک کاربر)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(BaseSearchDto<RebateListVDto>), 200)]
        public IActionResult Get([FromQuery] RebateInputDto dto)
        {
            dto.TargetType = ClubRewardTargetTypeEnum.Store;
            dto.TargetId = _currentUser.CurrentUser.StoreId;
            return Ok(_rebateService.SearchByTarget(dto));
        }

        /// <summary>
        /// ساخت کد تخفیف برای فروشگاه من؛ UserId اختیاری (کد فقط برای همان کاربر کار می‌کند)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<RebateDto>), 200)]
        public async Task<IActionResult> Post(RebateDto rebateDto)
        {
            return Ok(await _rebateService.InsertForStoreAsync(rebateDto, _currentUser.CurrentUser.StoreId));
        }

        /// <summary>
        /// فعال/غیرفعال کردن کد
        /// </summary>
        [HttpPut("Active")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> PutActive(long id, bool active)
        {
            return Ok(await _rebateService.SetActiveAsync(id, active, _currentUser.CurrentUser.StoreId));
        }

        /// <summary>
        /// حذف کد
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id)
        {
            return Ok(await _rebateService.DeleteForStoreAsync(id, _currentUser.CurrentUser.StoreId));
        }
    }
}
