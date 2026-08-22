using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.ProductSrvs.ProductStockAlertSrv.Dto;
using Application.Services.ProductSrvs.ProductStockAlertSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// اعلان موجودشدن محصولات مورد انتظار کاربر
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductStockAlertController : ControllerBase
    {
        private readonly IProductStockAlertService _service;
        private readonly ICurrentUserHelper _currentUserHelper;

        public ProductStockAlertController(IProductStockAlertService service, ICurrentUserHelper currentUserHelper)
        {
            _service = service;
            _currentUserHelper = currentUserHelper;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ProductStockAlertSearchDto), 200)]
        public IActionResult Get([FromQuery] ProductStockAlertInputDto dto)
        {
            dto.Available = true;
            dto.UserId = _currentUserHelper.CurrentUser.UserId;
            return Ok(_service.SearchDto(dto));
        }

        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<ProductStockAlertDto>), 200)]
        public async Task<IActionResult> Post(ProductStockAlertDto dto)
        {
            dto.UserId = _currentUserHelper.CurrentUser.UserId;
            return Ok(await _service.SubscribeAsync(dto));
        }

        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(ProductStockAlertDto dto)
        {
            dto.UserId = _currentUserHelper.CurrentUser.UserId;
            return Ok(await _service.UnsubscribeAsync(dto));
        }
    }
}
