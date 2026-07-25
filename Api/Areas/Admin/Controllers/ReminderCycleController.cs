using Application.Common.Dto.Result;
using Application.Services.ReminderSrvs.ReminderCycleSrv.Dto;
using Application.Services.ReminderSrvs.ReminderCycleSrv.Iface;
using Application.Services.ReminderSrvs.ReminderSrv.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// چرخه های یادآوری
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ReminderCycleController : ControllerBase
    {
        private IReminderCycleService _reminderCycleService;
        public ReminderCycleController(IReminderCycleService reminderCycleService)
        {
            _reminderCycleService = reminderCycleService;
        }

        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه چرخه یادآور</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<ReminderCycleDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var Driver = await _reminderCycleService.FindAsyncDto(id);
            return Ok(Driver);
        }

        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet()]
        [ProducesResponseType(typeof(ReminderCycleSearchDto), 200)]
        public IActionResult Get([FromQuery] ReminderCycleInputDto dto)
        {
            var roles = _reminderCycleService.Search(dto);
            return Ok(roles);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<ReminderCycleDto>), 200)]
        public async Task<IActionResult> Post(ReminderCycleDto roleDto)
        {
            var dto = await _reminderCycleService.InsertAsyncDto(roleDto);
            return Ok(dto);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(ReminderCycleDto roleDto)
        {
            var dto = _reminderCycleService.UpdateDto(roleDto);
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
            var dto = _reminderCycleService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
