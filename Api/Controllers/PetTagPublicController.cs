using Application.Services.Accounting.PetTagSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Api.Controllers
{
    /// <summary>
    /// استعلام عمومی کد قلاده (بدون نیاز به ورود)
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class PetTagPublicController : ControllerBase
    {
        private readonly IPetTagService _service;
        public PetTagPublicController(IPetTagService service)
        {
            _service = service;
        }

        /// <summary>
        /// وضعیت یک کد قلاده: اگر متصل به پتی نباشد claimed=false برمی‌گردد،
        /// اگر متصل باشد پروفایل عمومی پت و مالکش برمی‌گردد.
        /// </summary>
        [HttpGet("{code}")]
        public async Task<IActionResult> Get(string code) => Ok(await _service.GetPublicStatusAsync(code));
    }
}
