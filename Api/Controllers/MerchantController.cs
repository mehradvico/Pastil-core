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
            var model = MerchantService.Search(dto, hideTestGateway: _currentUser.CurrentUser?.RoleEnum != RoleEnum.Admin.ToString());
            return Ok(model);
        }
    }
}
