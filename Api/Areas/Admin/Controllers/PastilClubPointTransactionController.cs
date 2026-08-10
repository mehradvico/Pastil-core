using Application.Services.PastilClubSrvs.PointSrv.Dto;
using Application.Services.PastilClubSrvs.PointSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// گردش امتیازها
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PastilClubPointTransactionController : ControllerBase
    {
        private readonly IClubPointService _clubPointService;

        public PastilClubPointTransactionController(IClubPointService clubPointService)
        {
            _clubPointService = clubPointService;
        }

        /// <summary>
        /// جستجوی گردش امتیاز کاربران
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ClubPointTransactionSearchDto), 200)]
        public async Task<IActionResult> Get([FromQuery] ClubPointTransactionInputDto dto, CancellationToken cancellationToken)
        {
            return Ok(await _clubPointService.SearchTransactionsAsync(dto, null, cancellationToken));
        }
    }
}
