using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrv.CompanionTimeSrv.Dto;
using Application.Services.CompanionSrv.CompanionTimeSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// مدیریت ساعات کاری مرکز توسط خودِ نماینده
    /// </summary>
    ///
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionTimeController : ControllerBase
    {
        private readonly ICompanionTimeService _companionTimeService;
        private readonly ICurrentUserHelper _currentUserHelper;
        public CompanionTimeController(ICompanionTimeService companionTimeService, ICurrentUserHelper currentUserHelper)
        {
            this._companionTimeService = companionTimeService;
            this._currentUserHelper = currentUserHelper;
        }

        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns>
        [HttpGet()]
        [ProducesResponseType(typeof(CompanionTimeSearchDto), 200)]
        public IActionResult Get([FromQuery] CompanionTimeInputDto dto)
        {
            var search = _companionTimeService.Search(dto);
            return Ok(search);
        }

        /// <summary>
        ///  اطلاعات آیتم
        /// </summary>
        /// <param name="id">شناسه نمایندگی</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<CompanionTimeUpdateListDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var agency = await _companionTimeService.GetListAsync(id);
            return Ok(agency);
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Post(CompanionTimeUpdateListDto dto)
        {
            var result = await _companionTimeService.InsertUpdateListAsync(dto, _currentUserHelper.CurrentUser.CompanionId);
            return Ok(result);
        }
    }
}
