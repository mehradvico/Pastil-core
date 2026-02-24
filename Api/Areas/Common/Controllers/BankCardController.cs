using Application.Common.Dto.Result;
using Application.Services.FinanceSrvs.BankCardSrv.Dto;
using Application.Services.FinanceSrvs.BankCardSrv.Iface;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Common.Controllers
{
    /// <summary>
    /// مدیریت اطلاعات بانک ها
    /// </summary>
    ///
    [Area("Common")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class BankCardController : ControllerBase
    {
        private IBankCardService BankCardService;
        public BankCardController(IBankCardService BankCardService)
        {
            this.BankCardService = BankCardService;
        }
        
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<BankCardVDto>), 200)]
        public IActionResult Get([FromQuery] BankCardInputDto dto)
        {
            var searchDto = BankCardService.Search(dto);
            return Ok(searchDto);
        }

    }
}
