using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrv.CompanionAssistanceTimeSrv.Dto;
using Application.Services.CompanionSrv.CompanionAssistanceTimeSrv.Dto.Dto;
using Application.Services.CompanionSrv.CompanionAssistanceTimeSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// مدیریت زمان های خدمات نمایندگان
    /// </summary>
    /// 
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionAssistanceTimeController : ControllerBase
    {
        private readonly ICompanionAssistanceTimeService _companionAssistanceTimeService;
        private readonly ICurrentUserHelper _currentUserHelper;
        public CompanionAssistanceTimeController(ICompanionAssistanceTimeService companionAssistanceTimeService, ICurrentUserHelper currentUserHelper)
        {
            this._companionAssistanceTimeService = companionAssistanceTimeService;
            this._currentUserHelper = currentUserHelper;
        }

        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(CompanionAssistanceTimeSearchDto), 200)]
        public IActionResult Get([FromQuery] CompanionAssistanceTimeInputDto dto)
        {
            var search = _companionAssistanceTimeService.Search(dto);
            return Ok(search);
        }

        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه زمان خدمات نمایندگان</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<CompanionAssistanceTimeUpdateDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var agency = await _companionAssistanceTimeService.GetListAsync(id);
            return Ok(agency);
        }


        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<CompanionAssistanceTimeUpdateListDto>), 200)]
        public async Task<IActionResult> Post(CompanionAssistanceTimeUpdateListDto dto)
        {
            var result = await _companionAssistanceTimeService.InsertUpdateListAsync(dto);
            return Ok(result);
        }

        /// <summary>
        ///  ویرایش آیتم 
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(CompanionAssistanceTimeDto dto)
        {
            var agency = await _companionAssistanceTimeService.ActiveAsync(dto);
            return Ok(agency);
        }
    }
}
