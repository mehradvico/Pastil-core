using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrvs.CompanionSrv.Dto;
using Application.Services.CompanionSrvs.CompanionSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// مدیریت نمایندگان
    /// </summary>
    /// 
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionController : ControllerBase
    {
        private readonly ICompanionService _companionService;
        private readonly ICurrentUserHelper _currentUser;
        public CompanionController(ICompanionService companionService, ICurrentUserHelper currentUser)
        {
            this._companionService = companionService;
            this._currentUser = currentUser;    
        }


        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(CompanionSearchDto), 200)]
        public IActionResult Get([FromQuery] CompanionInputDto dto)
        {
            var search = _companionService.Search(dto);
            return Ok(search);
        }

        /// <summary>
        /// دریافت همراهان نزدیک به موقعیت فعلی کاربر
        /// </summary>
        [HttpGet("Nearby")]
        [ProducesResponseType(typeof(BaseResultDto<NearbyCompanionSearchDto>), 200)]
        public async Task<IActionResult> GetNearby([FromQuery] NearbyCompanionInputDto dto)
        {
            var result = await _companionService.GetNearbyAsync(_currentUser.CurrentUser.UserId, dto);
            return Ok(result);
        }


        /// <summary>
        /// اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه نماینده</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<CompanionVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var companion = await _companionService.FindAsyncVDto(id);
            return Ok(companion);
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<CompanionDto>), 200)]
        public async Task<IActionResult> Post(CompanionDto dto)
        {
            dto.OwnerId = _currentUser.CurrentUser.UserId;
            var result = await _companionService.InsertAsyncDto(dto);
            return Ok(result);
        }
    }
}
