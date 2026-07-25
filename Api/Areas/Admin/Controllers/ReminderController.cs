using Application.Common.Dto.Result;
using Application.Services.ProductSrvs.ProductRelateSrv;
using Application.Services.ProductSrvs.ProductRelateSrv.Dto;
using Application.Services.ReminderSrvs.ReminderCycleSrv.Dto;
using Application.Services.ReminderSrvs.ReminderSrv.Dto;
using Application.Services.ReminderSrvs.ReminderSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// یادآوری ها
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ReminderController : ControllerBase
    {
        private IReminderService _reminderService;
        public ReminderController(IReminderService reminderService)
        {
            _reminderService = reminderService;
        }

        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه یادآور</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<ReminderVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var Driver = await _reminderService.FindAsyncVDto(id);
            return Ok(Driver);
        }

        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet()]
        [ProducesResponseType(typeof(ReminderSearchDto), 200)]
        public IActionResult Get([FromQuery] ReminderInputDto dto)
        {
            var roles = _reminderService.Search(dto);
            return Ok(roles);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<ReminderDto>), 200)]
        public async Task<IActionResult> Post(ReminderDto roleDto)
        {
            var dto = await _reminderService.InsertAsyncDto(roleDto);
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
            var dto = _reminderService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
