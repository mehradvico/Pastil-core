using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.PansionSrvs.PansionSrv.Dto;
using Application.Services.PansionSrvs.PansionSrv.Iface;
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
        public PansionController(IPansionService PansionService, ICurrentUserHelper currentUser)
        {
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
            dto.CompanionId = _currentUser.CurrentUser.CompanionId;
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
                !_currentUser.CurrentUser.CompanionId.HasValue ||
                Pansion.Data?.CompanionId != _currentUser.CurrentUser.CompanionId.Value)
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
            if (!_currentUser.CurrentUser.CompanionId.HasValue)
                return Forbid();
            dto.CompanionId = _currentUser.CurrentUser.CompanionId!.Value;
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
            if (!_currentUser.CurrentUser.CompanionId.HasValue)
                return Forbid();
            dto.CompanionId = _currentUser.CurrentUser.CompanionId!.Value;
            var Pansion = await _PansionService.ResubmitAsyncDto(
                dto,
                dto.CompanionId,
                _currentUser.CurrentUser.UserId);
            return Ok(Pansion);
        }
    }
}
