using Application.Common.Dto.Result;
using Application.Services.Accounting.PetSrv.Dto;
using Application.Services.Accounting.PetSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// پت ها
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PetController : ControllerBase
    {
        private IPetService _petService;
        /// <summary>
        /// پت ها
        /// </summary>
        ///
        public PetController(IPetService petService)
        {
            _petService = petService;
        }
        /// <summary>
        ///  اطلاعات پت 
        /// </summary>
        /// <param name="id">شناسه</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<PetDto>), 200)]
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
        [ProducesResponseType(typeof(PetInputDto), 200)]
        public IActionResult Get([FromQuery] PetInputDto dto)
        {
            var searchDto = _petService.Search(dto);
            return Ok(searchDto);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<PetDto>), 200)]
        public async Task<IActionResult> Post(PetDto PetDto)
        {

            var dto = await _petService.InsertAsyncDto(PetDto);
            return Ok(dto);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>

        [HttpPut]
        [ProducesResponseType(typeof(PetDto), 200)]
        public IActionResult Put(PetDto PetDto)
        {
            var dto = _petService.UpdateDto(PetDto);
            return Ok(dto);
        }
        /// <summary>
        /// حذف آیتم
        /// </summary>
        ///
        [HttpDelete]
        [ProducesResponseType(typeof(PetDto), 200)]
        public IActionResult Delete(long id)
        {
            var dto = _petService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
