using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Accounting.UserPerRecordSrv.Dto;
using Application.Services.Accounting.UserPerRecordSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// سابقه پت کاربر
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class UserPetRecordController : ControllerBase
    {
        private IUserPetRecordService _petService;

        public UserPetRecordController(IUserPetRecordService petService)
        {
            _petService = petService;
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
            var dto = await _petService.InsertAsyncDto(UserPetRecordDto);
            return Ok(dto);
        }
    }
}
