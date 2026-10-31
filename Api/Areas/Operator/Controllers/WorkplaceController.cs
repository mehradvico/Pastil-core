using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.VenueOperatorSrv;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Api.Areas.Operator.Controllers
{
    /// <summary>
    /// مرکزهایی (کلینیک/آرایشگاه/مربی) که من پرسنل یا مالک آن‌ها هستم؛ اپ با لیست غیرخالی منوی پنل اپراتور را نشان می‌دهد
    /// </summary>
    [Area("Operator")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class WorkplaceController : ControllerBase
    {
        private readonly IVenueOperatorService _service;
        private readonly ICurrentUserHelper _currentUser;

        public WorkplaceController(IVenueOperatorService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<List<VenueWorkplaceDto>>), 200)]
        public async Task<IActionResult> Get() =>
            Ok(new BaseResultDto<List<VenueWorkplaceDto>>(true, await _service.GetWorkplacesAsync(_currentUser.CurrentUser.UserId)));
    }
}
