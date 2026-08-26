using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Content.CargoSrv.Dto;
using Application.Services.Content.CargoSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// مدیریت  کارگو ها
    /// </summary>
    /// 
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CargoController : ControllerBase
    {
        private readonly ICargoService _cargoService;
        private readonly ICurrentUserHelper _currentUser;
        public CargoController(ICargoService cargoService, ICurrentUserHelper currentUser)
        {
            _cargoService = cargoService;
            _currentUser = currentUser;
        }

        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(CargoSearchDto), 200)]
        public IActionResult Get([FromQuery] CargoInputDto dto)
        {
            dto.UserId = _currentUser.CurrentUser.UserId;
            var search = _cargoService.Search(dto);
            return Ok(search);
        }


        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه سفر</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<CargoVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var Cargo = await _cargoService.FindAsyncVDto(id);
            return Ok(Cargo);
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<CargoDto>), 200)]
        public async Task<IActionResult> Post(CargoDto dto)
        {
            var result = await _cargoService.InsertAsyncDto(dto);
            return Ok(result);
        }
    }
}
