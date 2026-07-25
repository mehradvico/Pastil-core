using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Services.ProductSrvs.WalletSrv.Dto;
using Application.Services.ProductSrvs.WalletSrv.IFace;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// کیف پول ها
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;

        public WalletController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<WalletDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var role = await _walletService.FindAsyncVDto(id);
            return Ok(role);
        }
        /// <summary>
        /// جستجو
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(BaseInputDto), 200)]
        public IActionResult Get([FromQuery] WalletInputDto dto)
        {
            var searchDto = _walletService.Search(dto);
            return Ok(searchDto);
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<WalletDto>), 200)]
        public async Task<IActionResult> Post(WalletDto WalletDto)
        {
            var dto = await _walletService.InsertAsyncDto(WalletDto);
            return Ok(dto);
        }

        /// <summary>
        /// حذف مورد در انتظار پرداخت
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id)
        {
            var dto = await _walletService.DeleteAsync(id);
            return Ok(dto);
        }

        /// <summary>
        /// دریافت مقدار کیف پول کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>مقدار کیف پول</returns>
        [HttpGet("amount/{userId}")]
        public async Task<IActionResult> GetAmount(long userId)
        {
            var result = await _walletService.GetAmountAsync(userId);
            return Ok(result.Data);
        }
    }
}
