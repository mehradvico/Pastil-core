using Application.Services.Accounting.PetTagSrv.Dto;
using Application.Services.Accounting.PetTagSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// کیوآر کد
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PetTagController : ControllerBase
    {
        private readonly IPetTagService _service;
        public PetTagController(IPetTagService service)
        {
            _service = service;
        }

        /// <summary>
        /// جزئیات یک کد
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(long id) => Ok(await _service.FindAsyncDto(id));

        /// <summary>
        /// جستجو/لیست کدها
        /// </summary>
        [HttpGet]
        public IActionResult Get([FromQuery] PetTagInputDto dto) => Ok(_service.Search(dto));

        /// <summary>
        /// تولید دسته‌ای کد یکتا (۱ تا ۲۰۰۰ عدد)
        /// </summary>
        [HttpPost("generate")]
        public async Task<IActionResult> Generate(PetTagGenerateInputDto dto) => Ok(await _service.GenerateAsync(dto.Count));

        /// <summary>
        /// غیرفعال‌کردن یک کد
        /// </summary>
        [HttpDelete]
        public IActionResult Delete(long id) => Ok(_service.DeleteDto(id));
    }
}
