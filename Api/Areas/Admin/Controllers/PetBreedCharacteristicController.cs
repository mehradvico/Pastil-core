using Application.Common.Dto.Result;
using Application.Services.Accounting.PetBreedCharacteristicSrv.Dto;
using Application.Services.Accounting.PetBreedCharacteristicSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// خصوصیات نژاد پت
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PetBreedCharacteristicController : ControllerBase
    {
        private readonly IPetBreedCharacteristicService _petBreedCharacteristicService;

        public PetBreedCharacteristicController(IPetBreedCharacteristicService petBreedCharacteristicService)
        {
            this._petBreedCharacteristicService = petBreedCharacteristicService;
        }

        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<PetBreedCharacteristicVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var result = await _petBreedCharacteristicService.FindAsyncVDto(id);
            return Ok(result);
        }

        /// <summary>
        /// جستجو
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PetBreedCharacteristicSearchDto), 200)]
        public IActionResult Get([FromQuery] PetBreedCharacteristicInputDto dto)
        {
            var result = _petBreedCharacteristicService.Search(dto);
            return Ok(result);
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<PetBreedCharacteristicDto>), 200)]
        public async Task<IActionResult> Post(PetBreedCharacteristicDto dto)
        {
            var result = await _petBreedCharacteristicService.InsertAsyncDto(dto);
            return Ok(result);
        }

        /// <summary>
        /// ویرایش آیتم
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(PetBreedCharacteristicDto dto)
        {
            var result = _petBreedCharacteristicService.UpdateDto(dto);
            return Ok(result);
        }

        /// <summary>
        /// حذف آیتم
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Delete(long id)
        {
            var result = _petBreedCharacteristicService.DeleteDto(id);
            return Ok(result);
        }
    }
}
