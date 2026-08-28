using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Accounting.UserPerRecordSrv.Dto;
using Application.Services.Accounting.UserPerRecordSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Operator.Controllers
{
    /// <summary>
    /// مدیریت تاریخچه پت ها
    /// </summary>
    ///
    [Area("Operator")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class UserPetRecordController : ControllerBase
    {
        private IUserPetRecordService _petService;
        private readonly ICurrentUserHelper _currentUserHelper;

        /// <summary>
        /// مدیریت تاریخچه پت ها
        /// </summary>
        ///
        public UserPetRecordController(IUserPetRecordService petService, ICurrentUserHelper currentUserHelper)
        {
            _petService = petService;
            _currentUserHelper = currentUserHelper;
        }
        /// <summary>
        ///  اطلاعات تاریخجه پت 
        /// </summary>
        /// <param name="id">شناسه</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<UserPetRecordDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var role = await _petService.FindAsyncDto(id);
            if (role.IsSuccess && role.Data?.OperatorId != _currentUserHelper.CurrentUser.UserId)
                return Ok(new BaseResultDto<UserPetRecordDto>(false, Resource.Notification.AccessDenied, default!));
            return Ok(role);
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet]
        [ProducesResponseType(typeof(UserPetRecordInputDto), 200)]
        public IActionResult Get([FromQuery] UserPetRecordInputDto dto)
        {
            dto.OperatorId = _currentUserHelper.CurrentUser.UserId;
            var searchDto = _petService.Search(dto);
            return Ok(searchDto);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<UserPetRecordDto>), 200)]
        public async Task<IActionResult> Post(UserPetRecordDto UserPetRecordDto)
        {
            UserPetRecordDto.OperatorId = _currentUserHelper.CurrentUser.UserId;
            var dto = await _petService.InsertAsyncDto(UserPetRecordDto);
            return Ok(dto);
        }
    }
}
