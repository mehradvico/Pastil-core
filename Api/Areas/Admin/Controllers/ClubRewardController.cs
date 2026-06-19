using Application.Common.Dto.Result;
using Application.Services.Accounting.ClubRewardSrv.Dto;
using Application.Services.Accounting.ClubRewardSrv.Iface;
using Application.Services.CategorySrv.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// مدیریت کلاب
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
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
        ///<returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(ClubRewardSearchDto), 200)]
        public IActionResult Get([FromQuery] ClubRewardInputDto dto)
        {
            var searchDto = ClubRewardService.Search(dto);
            return Ok(searchDto);
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<ClubRewardDto>), 200)]
        public async Task<IActionResult> Post(ClubRewardDto ClubRewardDto)
        {
            var insertDto = await ClubRewardService.InsertAsyncDto(ClubRewardDto);
            return Ok(insertDto);
        }

        /// <summary>
        /// ویرایش آیتم
        /// </summary>
        /// 
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<ClubRewardDto>), 200)]
        public IActionResult Put(ClubRewardDto ClubRewardDto)
        {
            var updateDto = ClubRewardService.UpdateDto(ClubRewardDto);
            return Ok(updateDto);
        }

        /// <summary>
        /// حذف آیتم
        /// </summary>
        ///
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto<ClubRewardDto>), 200)]
        public IActionResult Delete(long id)
        {
            var deleteDto = ClubRewardService.DeleteDto(id);
            return Ok(deleteDto);
        }


    }
}
