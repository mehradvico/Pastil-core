using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Accounting.ClubRewardSrv.Dto;
using Application.Services.Accounting.ClubRewardSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// مدیریت کلاب
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ClubRewardController : ControllerBase
    {
        private readonly IClubRewardService ClubRewardService;
        private readonly ICurrentUserHelper currentUser;
        /// <summary>
        /// مدیریت کلاب
        /// </summary>
        public ClubRewardController(IClubRewardService ClubRewardService, ICurrentUserHelper currentUser)
        {
            this.ClubRewardService = ClubRewardService;
            this.currentUser = currentUser;
        }

        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        /// <param name="id">شناسه دسته بندی</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<ClubRewardDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var getAll = await ClubRewardService.FindAsyncDto(id);
            return Ok(getAll);
        }

        /// <summary>
        /// جستجو
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ClubRewardSearchDto), 200)]
        public IActionResult Get([FromQuery] ClubRewardInputDto dto)
        {            
            var searchDto = ClubRewardService.Search(dto);
            return Ok(searchDto);
        }
    }
}
