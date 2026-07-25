using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Services.Dto;
using Application.Services.ReminderSrvs.ReminderCycleSrv.Dto;
using Application.Services.ReminderSrvs.ReminderTypeSrv;
using Application.Services.ReminderSrvs.ReminderTypeSrv.Dto;
using Application.Services.ReminderSrvs.ReminderTypeSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// انواع یادآوری
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ReminderTypeController : ControllerBase
    {
        private IReminderTypeService _reminderTypeService;
        public ReminderTypeController(IReminderTypeService reminderTypeService)
        {
            _reminderTypeService = reminderTypeService;
        }

        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه نوع یادآور</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<ReminderTypeDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var Driver = await _reminderTypeService.FindAsyncDto(id);
            return Ok(Driver);
        }

        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet()]
        [ProducesResponseType(typeof(ReminderTypeSearchDto), 200)]
        public IActionResult Get([FromQuery] ReminderTypeInputDto dto)
        {
            var roles = _reminderTypeService.Search(dto);
            return Ok(roles);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<ReminderTypeDto>), 200)]
        public async Task<IActionResult> Post(ReminderTypeDto roleDto)
        {
            var dto = await _reminderTypeService.InsertAsyncDto(roleDto);
            return Ok(dto);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(ReminderTypeDto roleDto)
        {
            var dto = _reminderTypeService.UpdateDto(roleDto);
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
            var dto = _reminderTypeService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
