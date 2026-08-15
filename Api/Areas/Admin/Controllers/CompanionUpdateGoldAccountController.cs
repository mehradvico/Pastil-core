using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.CompanionSrv.Dto;
using Application.Services.CompanionSrvs.CompanionSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// ارتقای حساب طلایی نماینده
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionUpdateGoldAccountController : ControllerBase
    {
        private readonly ICompanionService _companionService;
        public CompanionUpdateGoldAccountController(ICompanionService companionService)
        {
            this._companionService = companionService;
        }

        /// <summary>
        ///  ارتقا آیتم به طلایی
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(CompanionGoldAccountDto dto)
        {
            var companion = await _companionService.UpdateGoldAccountDto(dto);
            return Ok(companion);
        }
    }
}
