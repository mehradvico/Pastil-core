using Application.Common.Dto.Result;
using Application.Services.CompanionSrv.CompanionAssistanceUserSrv.Dto;
using Application.Services.CompanionSrv.CompanionAssistanceUserSrv.Iface;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// مدیریت پکیج های خدمات کاربران
    /// </summary>
    /// 
    [Route("api/[controller]")]
    [ApiController]
    public class CompanionAssistanceUserController : ControllerBase
    {
        private readonly ICompanionAssistanceUserService _companionAssistanceUserService;
        public CompanionAssistanceUserController(ICompanionAssistanceUserService companionAssistanceUserService)
        {
            this._companionAssistanceUserService = companionAssistanceUserService;
        }


        /// <summary>
        /// جستجو کاربران خدمات نمایندگان
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(CompanionAssistanceUserSearchDto), 200)]
        public IActionResult Get([FromQuery] CompanionAssistanceUserInputDto dto)
        {
            var search = _companionAssistanceUserService.Search(dto);
            return Ok(search);
        }


        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه کاربر خدمات نمایندگان</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<CompanionAssistanceUserDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var agency = await _companionAssistanceUserService.FindAsyncVDto(id);
            return Ok(agency);
        }
    }
}
