using Application.Common.Dto.Result;
using Application.Common.Enumerable.Code;
using Application.Common.Interface;
using Application.Services.Accounting.DriverSrv.Dto;
using Application.Services.Accounting.DriverSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// مدیریت رانندگان
    /// </summary>
    /// 
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class DriverController : ControllerBase
    {
        private readonly IDriverService _DriverService;
        private readonly ICurrentUserHelper _currentUser;
        public DriverController(IDriverService DriverService, ICurrentUserHelper currentUser)
        {
            this._DriverService = DriverService;
            this._currentUser = currentUser;
        }

        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(DriverSearchDto), 200)]
        public IActionResult Get([FromQuery] DriverInputDto dto)
        {
            dto.OwnerId = _currentUser.CurrentUser.UserId;
            var search = _DriverService.Search(dto);
            return Ok(search);
        }

        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه نماینده</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<DriverDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var Driver = await _DriverService.FindAsyncVDto(id);
            if (!Driver.IsSuccess || Driver.Data?.OwnerId != _currentUser.CurrentUser.UserId)
                return NotFound(new BaseResultDto(false, Resource.Notification.NothingFound));
            return Ok(Driver);
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<DriverDto>), 200)]
        public async Task<IActionResult> Post(DriverDto dto)
        {
            dto.StatusId = (long)DriverRequestStatusEnum.DriverRequestStatus_Requested;
            dto.OwnerId = _currentUser.CurrentUser.UserId;
            var result = await _DriverService.InsertAsyncDto(dto);
            return Ok(result);
        }

        /// <summary>
        /// ویرایش و ارسال مجدد درخواست رانندگی
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(DriverDto dto)
        {
            var result = await _DriverService.ResubmitAsyncDto(
                dto,
                _currentUser.CurrentUser.UserId);
            return Ok(result);
        }
    }
}
