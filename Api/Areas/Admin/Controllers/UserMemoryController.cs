using Application.Common.Dto.Result;
using Application.Services.MemorySrvs.MemorySrv.Dto;
using Application.Services.MemorySrvs.MemorySrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// مشاهده خاطرات کاربران توسط مدیر دارای دسترسی
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class UserMemoryController : ControllerBase
    {
        private readonly IMemoryService _memoryService;

        public UserMemoryController(IMemoryService memoryService)
        {
            _memoryService = memoryService;
        }

        /// <summary>
        /// مشاهده جزئیات خاطره
        /// </summary>
        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(BaseResultDto<MemoryVDto>), 200)]
        public async Task<IActionResult> Get(long id, CancellationToken cancellationToken)
        {
            var result = await _memoryService.FindAsync(id, null, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// جستجوی خاطرات کاربران
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MemorySearchDto), 200)]
        public async Task<IActionResult> Get(
            [FromQuery] MemoryInputDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _memoryService.SearchAsync(dto, null, cancellationToken);
            return Ok(result);
        }
    }
}
