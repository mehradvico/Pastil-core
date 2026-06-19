using Application.Common.Dto.Result;
using Application.Services.Accounting.ClubRewardSrv.Dto;
using Application.Services.Accounting.ClubRewardSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// مدیریت کلاب
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ClubRewardController : ControllerBase
    {
        private readonly IClubRewardService ClubRewardService;
        /// <summary>
        /// مدیریت کلاب
        /// </summary>
        public ClubRewardController(IClubRewardService ClubRewardService)
        {
            this.ClubRewardService = ClubRewardService;
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
