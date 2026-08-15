using Application.Common.Dto.Result;
using Application.Services.ProductSrvs.StoreSrv.Dto;
using Application.Services.StoreSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// تأیید یا رد درخواست فروشگاه
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class StoreApprovalController : ControllerBase
    {
        private readonly IStoreService _storeService;

        public StoreApprovalController(IStoreService storeService)
        {
            _storeService = storeService;
        }

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(StoreApprovalDto dto)
        {
            return Ok(await _storeService.UpdateApprovalAsync(dto));
        }
    }
}
