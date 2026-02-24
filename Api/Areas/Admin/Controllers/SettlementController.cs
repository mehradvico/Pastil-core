using Application.Common.Dto.Result;
using Application.Services.FinanceSrvs.SettlementSrv.Dto;
using Application.Services.FinanceSrvs.SettlementSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// مدیریت تسویه حساب ها
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class SettlementController : ControllerBase
    {
        private ISettlementService SettlementService;
        public SettlementController(ISettlementService SettlementService)
        {
            this.SettlementService = SettlementService;
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه دسته بندی</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<SettlementDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var role = await SettlementService.FindAsyncDto(id);
            return Ok(role);
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<SettlementVDto>), 200)]
        public IActionResult Get([FromQuery] SettlementInputDto dto)
        {
            var searchDto = SettlementService.Search(dto);
            return Ok(searchDto);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(BaseResultDto<SettlementDto>), 200)]
        public async Task<IActionResult> Post(SettlementDto SettlementDto)
        {

            var dto = await SettlementService.InsertAsyncDto(SettlementDto);
            return Ok(dto);
        }
    }
}
