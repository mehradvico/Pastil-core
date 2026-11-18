using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Services.Order.RebateSrv.Dto;
using Application.Services.Order.RebateSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// کد های تخفیف
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class RebateController : ControllerBase
    {
        private IRebateService RebateService;
        public RebateController(IRebateService RebateService)
        {
            this.RebateService = RebateService;
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<RebateDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var role = await RebateService.FindAsyncDto(id);
            return Ok(role);
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<RebateDto>), 200)]
        public IActionResult Get([FromQuery] BaseInputDto dto)
        {
            var searchDto = RebateService.Search(dto);
            return Ok(searchDto);
        }
        /// <summary>
        ///  کدهای تخفیف یک هدف (مثلاً یک فروشگاه: TargetType=Store&amp;TargetId=...) و/یا یک کاربر
        /// </summary>
        [HttpGet("ByTarget")]
        [ProducesResponseType(typeof(BaseSearchDto<RebateListVDto>), 200)]
        public IActionResult ByTarget([FromQuery] RebateInputDto dto)
        {
            return Ok(RebateService.SearchByTarget(dto));
        }
        /// <summary>
        /// ساخت کد تخفیف مخصوص یک فروشگاه (اختیاری: فقط برای یک کاربر خاص با UserId)
        /// </summary>
        [HttpPost("Store/{storeId}")]
        [ProducesResponseType(typeof(BaseResultDto<RebateDto>), 200)]
        public async Task<IActionResult> PostForStore(long storeId, RebateDto rebateDto)
        {
            return Ok(await RebateService.InsertForStoreAsync(rebateDto, storeId));
        }
        /// <summary>
        /// ساخت کد تخفیف مخصوص فروشگاه یا مرکز: Store, Coach, Clinic, Groomer, Pansion, School
        /// </summary>
        [HttpPost("Target/{targetType}/{targetId}")]
        [ProducesResponseType(typeof(BaseResultDto<RebateDto>), 200)]
        public async Task<IActionResult> PostForTarget(Entities.Entities.PastilClubField.ClubRewardTargetTypeEnum targetType, long targetId, RebateDto rebateDto)
        {
            return Ok(await RebateService.InsertForTargetAsync(rebateDto, targetType, targetId));
        }
        /// <summary>
        /// فعال/غیرفعال کردن کد تخفیف
        /// </summary>
        [HttpPut("Active")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> PutActive(long id, bool active)
        {
            return Ok(await RebateService.SetActiveAsync(id, active));
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<RebateDto>), 200)]
        public async Task<IActionResult> Post(RebateDto RebateDto)
        {
            var dto = await RebateService.InsertAsyncDto(RebateDto);
            return Ok(dto);
        }
        /// <summary>
        /// حذف آیتم
        /// </summary>
        ///
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Delete(long id)
        {
            var dto = RebateService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
