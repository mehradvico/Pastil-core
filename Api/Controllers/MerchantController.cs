using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Interface;
using Application.Services.Order.MerchantSrv.Dto;
using Application.Services.Order.MerchantSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// مرتبط با بنر ها
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class MerchantController : ControllerBase
    {
        private IMerchantService MerchantService;
        private readonly ICurrentUserHelper _currentUser;
        /// <summary>
        /// مرتبط با درگاه بانکی
        /// </summary>
        public MerchantController(IMerchantService MerchantService, ICurrentUserHelper currentUser)
        {
            _currentUser = currentUser;
            this.MerchantService = MerchantService;
        }


        /// <summary>
        /// جستجو
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(BaseSearchDto<MerchantVDto>), 200)]
        public IActionResult Get([FromQuery] BaseInputDto dto)
        {
            dto.Available = true;
            // همه زرین‌پال، مدیر اصلی علاوه بر آن درگاه تستی پاستیل؛ سمت شروع پرداخت هم همین اجباری است
            var model = MerchantService.Search(dto, onlyBankIds: Application.Services.Order.MerchantSrv.MerchantGatewayRules.AllowedBankIds(_currentUser.CurrentUser));
            return Ok(model);
        }
    }
}
