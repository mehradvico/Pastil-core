using Application.Common.Dto.Result;
using Application.Services.FinanceSrvs.BankCardSrv.Dto;
using Application.Services.FinanceSrvs.BankCardSrv.Iface;
using Application.Services.FinanceSrvs.UserBankCardSrv.Dto;
using Application.Services.FinanceSrvs.UserBankCardSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// مدیریت کارت های بانکی
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class BankCardController : ControllerBase
    {
        private readonly IBankCardService BankCardService;

        public BankCardController(IBankCardService BankCardService)
        {
            this.BankCardService = BankCardService;
        }
        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(BankCardSearchDto), 200)]
        public IActionResult Get([FromQuery] BankCardInputDto dto)
        {
            var UserBankCard = BankCardService.Search(dto);
            return Ok(UserBankCard);
        }
    }
}
