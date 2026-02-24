using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.FinanceSrvs.SettlementSrv.Dto;
using Application.Services.FinanceSrvs.SettlementSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// مدیریت تسویه حساب ها
    /// </summary>
    ///
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class SettlementController : ControllerBase
    {
        private ISettlementService SettlementService;
        private ICurrentUserHelper _currentuser;
        public SettlementController(ISettlementService SettlementService, ICurrentUserHelper currentuser)
        {
            this.SettlementService = SettlementService;
            this._currentuser = currentuser;
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
            dto.CompanionId = _currentuser.CurrentUser.CompanionId;
            var searchDto = SettlementService.Search(dto);
            return Ok(searchDto);
        }
    }
}

