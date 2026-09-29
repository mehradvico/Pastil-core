using Application.Common.Dto.Result;
using Application.Services.SchoolSrvs.SchoolPictureSrv.Dto;
using Application.Services.SchoolSrvs.SchoolPictureSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// تصاویر گالری مدرسه
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class SchoolPictureController : ControllerBase
    {
        private readonly ISchoolPictureService SchoolPictureService;

        public SchoolPictureController(ISchoolPictureService SchoolPictureService)
        {
            this.SchoolPictureService = SchoolPictureService;
        }
        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        ///
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<SchoolPictureVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var SchoolPicture = await SchoolPictureService.FindAsyncVDto(id);
            return Ok(SchoolPicture);
        }
        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(SchoolPictureSearchDto), 200)]
        public IActionResult Get([FromQuery] SchoolPictureInputDto dto)
        {
            var SchoolPicture = SchoolPictureService.Search(dto);
            return Ok(SchoolPicture);
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<SchoolPictureDto>), 200)]
        public async Task<IActionResult> Post(SchoolPictureDto SchoolPictureDto)
        {
            var result = await SchoolPictureService.InsertAsyncDto(SchoolPictureDto);
            return Ok(result);
        }
        /// <summary>
        /// حذف آیتم
        /// </summary>
        /// <returns></returns>
        ///
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto<SchoolPictureDto>), 200)]
        public IActionResult Delete(long id)
        {
            var result = SchoolPictureService.DeleteDto(id);
            return Ok(result);
        }
    }
}
