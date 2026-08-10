using Application.Common.Interface;
using Application.Services.PastilClubSrvs.PointSrv.Dto;
using Application.Services.PastilClubSrvs.PointSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// گردش امتیاز پاستیل کلاب کاربر
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PastilClubPointTransactionController : ControllerBase
    {
        private readonly IClubPointService _clubPointService;
        private readonly ICurrentUserHelper _currentUser;

        public PastilClubPointTransactionController(IClubPointService clubPointService, ICurrentUserHelper currentUser)
        {
            _clubPointService = clubPointService;
            _currentUser = currentUser;
        }

        /// <summary>
        /// جستجوی گردش امتیاز کاربر
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ClubPointTransactionSearchDto), 200)]
        public async Task<IActionResult> Get([FromQuery] ClubPointTransactionInputDto dto, CancellationToken cancellationToken)
        {
            return Ok(await _clubPointService.SearchTransactionsAsync(
                dto,
                _currentUser.CurrentUser.UserId,
                cancellationToken));
        }
    }
}
