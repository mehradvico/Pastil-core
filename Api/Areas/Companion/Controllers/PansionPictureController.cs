using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.PansionSrvs.PansionPictureSrv.Dto;
using Application.Services.PansionSrvs.PansionPictureSrv.Iface;
using Application.Services.PansionSrvs.PansionSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// مدیریت تصویر پانسیون ها
    /// </summary>
    ///
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PansionPictureController : ControllerBase
    {
        private readonly IPansionPictureService PansionPictureService;
        private readonly IPansionService _PansionService;
        private readonly ICurrentUserHelper _currentUser;

        public PansionPictureController(IPansionPictureService PansionPictureService, IPansionService PansionService, ICurrentUserHelper currentUser)
        {
            this.PansionPictureService = PansionPictureService;
            this._PansionService = PansionService;
            this._currentUser = currentUser;
        }
        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        /// 
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<PansionPictureVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var PansionPicture = await PansionPictureService.FindAsyncVDto(id);
            return Ok(PansionPicture);
        }
        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(PansionPictureSearchDto), 200)]
        public IActionResult Get([FromQuery] PansionPictureInputDto dto)
        {
            dto.CompanionId = _currentUser.CurrentUser.CompanionId;
            var PansionPicture = PansionPictureService.Search(dto);
            return Ok(PansionPicture);
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<PansionPictureDto>), 200)]
        public async Task<IActionResult> Pansion(PansionPictureDto PansionPictureDto)
        {
            var pansion = await _PansionService.FindAsyncVDto(PansionPictureDto.PansionId);
            if (!pansion.IsSuccess || pansion.Data?.CompanionId != _currentUser.CurrentUser.CompanionId)
                return Ok(new BaseResultDto<PansionPictureDto>(false, Resource.Notification.AccessDenied, PansionPictureDto));

            var result = await PansionPictureService.InsertAsyncDto(PansionPictureDto);
            return Ok(result);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>
        /// <returns></returns>
        ///
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<PansionPictureDto>), 200)]
        public async Task<IActionResult> Put(PansionPictureDto PansionPictureDto)
        {
            var existing = await PansionPictureService.FindAsyncVDto(PansionPictureDto.Id);
            if (!existing.IsSuccess)
                return Ok(new BaseResultDto<PansionPictureDto>(false, Resource.Notification.AccessDenied, PansionPictureDto));
            var pansion = await _PansionService.FindAsyncVDto(existing.Data.PansionId);
            if (!pansion.IsSuccess || pansion.Data?.CompanionId != _currentUser.CurrentUser.CompanionId)
                return Ok(new BaseResultDto<PansionPictureDto>(false, Resource.Notification.AccessDenied, PansionPictureDto));

            PansionPictureDto.PansionId = existing.Data.PansionId;
            var result = PansionPictureService.UpdateDto(PansionPictureDto);
            return Ok(result);
        }

        /// <summary>
        /// حذف آیتم
        /// </summary>
        /// <returns></returns>
        ///
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto<PansionPictureDto>), 200)]
        public async Task<IActionResult> Delete(long id)
        {
            var existing = await PansionPictureService.FindAsyncVDto(id);
            if (!existing.IsSuccess)
                return Ok(new BaseResultDto<PansionPictureDto>(false, Resource.Notification.AccessDenied, default!));
            var pansion = await _PansionService.FindAsyncVDto(existing.Data.PansionId);
            if (!pansion.IsSuccess || pansion.Data?.CompanionId != _currentUser.CurrentUser.CompanionId)
                return Ok(new BaseResultDto<PansionPictureDto>(false, Resource.Notification.AccessDenied, default!));

            var result = PansionPictureService.DeleteDto(id);
            return Ok(result);
        }
    }
}
