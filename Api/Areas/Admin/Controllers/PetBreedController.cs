using Application.Common.Dto.Result;
using Application.Services.Accounting.PetBreedBreedSrv.Dto;
using Application.Services.Accounting.PetBreedBreedSrv.Iface;
using Application.Services.Accounting.PetBreedSrv.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// مدیریت نژادهای پت
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PetBreedController : ControllerBase
    {
        private IPetBreedService _petBreedService;
        /// <summary>
        /// مدیریت نژادهای پت
        /// </summary>
        ///
        public PetBreedController(IPetBreedService petBreedService)
        {
            this._petBreedService = petBreedService;
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<PetBreedVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var role = await _petBreedService.FindAsyncVDto(id);
            return Ok(role);
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<PetBreedDto>), 200)]
        public IActionResult Get([FromQuery] PetBreedInputDto dto)
        {
            var searchDto = _petBreedService.Search(dto);
            return Ok(searchDto);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<PetBreedDto>), 200)]
        public async Task<IActionResult> Post(PetBreedDto dto)
        {

            var model = await _petBreedService.InsertAsyncDto(dto);
            return Ok(model);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(PetBreedDto PetBreedDto)
        {
            var dto = _petBreedService.UpdateDto(PetBreedDto);
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
            var dto = _petBreedService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
