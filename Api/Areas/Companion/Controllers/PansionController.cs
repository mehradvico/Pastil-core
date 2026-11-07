using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.PansionSrvs.PansionSrv.Dto;
using Application.Services.PansionSrvs.PansionSrv.Iface;
using Application.Services.PansionSrvs.PansionOwnershipSrv;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// مدیریت پانسیون
    /// </summary>
    /// 
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PansionController : ControllerBase
    {
        private readonly IPansionService _PansionService;
        private readonly ICurrentUserHelper _currentUser;
        private readonly IPansionOwnershipService _ownership;
        public PansionController(IPansionService PansionService, ICurrentUserHelper currentUser, IPansionOwnershipService ownership)
        {
            this._ownership = ownership;
            this._PansionService = PansionService;
            this._currentUser = currentUser;    
        }

        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(PansionSearchDto), 200)]
        public IActionResult Get([FromQuery] PansionInputDto dto)
        {
            dto.OwnerUserId = _currentUser.CurrentUser.UserId;
            var search = _PansionService.Search(dto);
            return Ok(search);
        }


        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه پانسیون</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<PansionDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var Pansion = await _PansionService.FindAsyncVDto(id);
            if (!Pansion.IsSuccess ||
                await _ownership.PansionOfPansionAsync(id, _currentUser.CurrentUser.UserId, approvedOnly: false) == null)
                return NotFound(new BaseResultDto(false, Resource.Notification.NothingFound));
            return Ok(Pansion);
        }


        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<PansionDto>), 200)]
        public async Task<IActionResult> Post(PansionDto dto)
        {
            // مالک: ارائه‌دهنده‌ی کاربر با نقش تأییدشده‌ی «پانسیون»، وگرنه نمایندگی قدیمی‌اش
            var owner = await _ownership.ResolveOwnerForNewPansionAsync(_currentUser.CurrentUser.UserId);
            if (!owner.Ok)
                return Ok(new BaseResultDto<PansionDto>(false, owner.Error, dto));
            dto.ProviderId = owner.ProviderId;
            dto.CompanionId = owner.CompanionId;
            var result = await _PansionService.InsertAsyncDto(dto);
            return Ok(result);
        }

        /// <summary>
        ///  ویرایش آیتم 
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(PansionDto dto)
        {
            var Pansion = await _PansionService.ResubmitAsyncDto(dto, _currentUser.CurrentUser.UserId);
            return Ok(Pansion);
        }

        /// <summary>
        /// حذف نرم پانسیون/مهد پت من (فقط مالک کلینیک). رزروها و گزارش مالی می‌مانند؛ با رزرو باز پرداخت‌شده
        /// حذف نمی‌شود. بازگردانی فقط توسط ادمین.
        /// </summary>
        /// <param name="id">شناسه پانسیون</param>
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _PansionService.SoftDeleteAsync(
                id, _currentUser.CurrentUser.UserId, _currentUser.CurrentUser.UserId);
            return Ok(result);
        }
    }
}
