using Application.Common.Dto.Result;
using Application.Services.Accounting.PetBreedBreedSrv.Dto;
using Application.Services.Accounting.PetBreedBreedSrv.Iface;
using Application.Services.Accounting.PetBreedSrv.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// مدیریت نژادهای پت
    /// </summary>
    ///
    [Route("api/[controller]")]
    [ApiController]
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
    }
}
