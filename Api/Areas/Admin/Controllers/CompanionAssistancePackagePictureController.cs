using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.CompanionAssistancePackagePictureSrv.Dto;
using Application.Services.CompanionSrvs.CompanionAssistancePackagePictureSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// تصویر پکیج های خدمات نمایندگان
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionAssistancePackagePictureController : ControllerBase
    {
        private readonly ICompanionAssistancePackagePictureService companionassistancepackagePictureService;
        public CompanionAssistancePackagePictureController(ICompanionAssistancePackagePictureService companionassistancepackagePictureService)
        {
            this.companionassistancepackagePictureService = companionassistancepackagePictureService;
        }
        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        /// 
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<CompanionAssistancePackagePictureDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var companionassistancepackagePicture = await companionassistancepackagePictureService.FindAsyncDto(id);
            return Ok(companionassistancepackagePicture);
        }
        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(CompanionAssistancePackagePictureSearchDto), 200)]
        public IActionResult Get([FromQuery] CompanionAssistancePackagePictureInputDto dto)
        {
            var companionassistancepackagePicture = companionassistancepackagePictureService.SearchDto(dto);
            return Ok(companionassistancepackagePicture);
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<CompanionAssistancePackagePictureDto>), 200)]
        public async Task<IActionResult> Post(CompanionAssistancePackagePictureDto companionassistancepackagePictureDto)
        {
            var result = await companionassistancepackagePictureService.InsertAsyncDto(companionassistancepackagePictureDto);
            return Ok(result);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<CompanionAssistancePackagePictureDto>), 200)]
        public IActionResult Put(CompanionAssistancePackagePictureDto companionassistancepackagePictureDto)
        {
            var result = companionassistancepackagePictureService.UpdateDto(companionassistancepackagePictureDto);
            return Ok(result);
        }

        /// <summary>
        /// حذف آیتم
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto<CompanionAssistancePackagePictureDto>), 200)]
        public IActionResult Delete(long id)
        {
            var result = companionassistancepackagePictureService.DeleteDto(id);
            return Ok(result);
        }
    }
}
