using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Services.Setting.BaseDetailSrv.Dto;
using Application.Services.Setting.BaseDetailSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    ///  مشخصات پایه
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class BaseDetailController : ControllerBase
    {
        private IBaseDetailService BaseDetailService;
        /// <summary>
        ///  مشخصات پایه
        /// </summary>
        ///
        public BaseDetailController(IBaseDetailService BaseDetailService)
        {
            this.BaseDetailService = BaseDetailService;
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<BaseDetailDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var role = await BaseDetailService.FindAsyncDto(id);
            return Ok(role);
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet]
        [ProducesResponseType(typeof(BaseInputDto), 200)]
        public IActionResult Get([FromQuery] BaseInputDto dto)
        {
            var searchDto = BaseDetailService.Search(dto);
            return Ok(searchDto);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<BaseDetailDto>), 200)]
        public async Task<IActionResult> Post(BaseDetailDto BaseDetailDto)
        {

            var dto = await BaseDetailService.InsertAsyncDto(BaseDetailDto);
            return Ok(dto);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(BaseDetailDto BaseDetailDto)
        {
            var dto = BaseDetailService.UpdateDto(BaseDetailDto);
            return Ok(dto);
        }
        /// <summary>
        /// حذف آیتم
        /// </summary>
        ///
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Delete(long id)
        {
            var dto = BaseDetailService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
