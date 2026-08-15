using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.ReminderSrvs.ReminderCycleSrv.Dto;
using Application.Services.ReminderSrvs.ReminderSrv.Dto;
using Application.Services.ReminderSrvs.ReminderSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// مدیریت یادآوری ها
    /// </summary>
    ///
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ReminderController : ControllerBase
    {
        private IReminderService _reminderService;
        private ICurrentUserHelper _currentUser;
        /// <summary>
        /// مدیریت یادآوری ها
        /// </summary>
        ///
        public ReminderController(IReminderService reminderService, ICurrentUserHelper currentUser)
        {
            _reminderService = reminderService;
            _currentUser = currentUser;
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
            var result = await _reminderService.FindUserAsyncVDto(
                id,
                _currentUser.CurrentUser.UserId);
            return Ok(result);
        }

        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet()]
        [ProducesResponseType(typeof(ReminderSearchDto), 200)]
        public IActionResult Get([FromQuery] ReminderInputDto dto)
        {
            dto.UserId = _currentUser.CurrentUser.UserId;
            var roles = _reminderService.Search(dto);
            return Ok(roles);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<ReminderDto>), 200)]
        public async Task<IActionResult> Post(ReminderDto reminderDto)
        {
            var result = await _reminderService.InsertUserAsyncDto(
                reminderDto,
                _currentUser.CurrentUser.UserId);
            return Ok(result);
        }
        /// <summary>
        /// حذف آیتم
        /// </summary>
        ///
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _reminderService.DeleteUserAsync(
                id,
                _currentUser.CurrentUser.UserId);
            return Ok(result);
        }
    }
}
