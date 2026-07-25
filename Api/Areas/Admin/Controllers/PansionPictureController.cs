using Application.Common.Dto.Result;
using Application.Services.PansionSrvs.PansionPictureSrv.Dto;
using Application.Services.PansionSrvs.PansionPictureSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// تصاویر پانسیون
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PansionPictureController : ControllerBase
    {
        private readonly IPansionPictureService PansionPictureService;

        public PansionPictureController(IPansionPictureService PansionPictureService)
        {
            this.PansionPictureService = PansionPictureService;
        }
        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        /// 
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<PansionPictureVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var PansionPicture = await PansionPictureService.FindAsyncVDto(id);
            return Ok(PansionPicture);
        }
        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(PansionPictureSearchDto), 200)]
        public IActionResult Get([FromQuery] PansionPictureInputDto dto)
        {
            var PansionPicture = PansionPictureService.Search(dto);
            return Ok(PansionPicture);
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<PansionPictureDto>), 200)]
        public async Task<IActionResult> Pansion(PansionPictureDto PansionPictureDto)
        {
            var result = await PansionPictureService.InsertAsyncDto(PansionPictureDto);
            return Ok(result);
        }
        /// <summary>
        /// حذف آیتم
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto<PansionPictureDto>), 200)]
        public IActionResult Delete(long id)
        {
            var result = PansionPictureService.DeleteDto(id);
            return Ok(result);
        }
    }
}
