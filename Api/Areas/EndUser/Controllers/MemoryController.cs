using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.MemorySrvs.MemorySrv.Dto;
using Application.Services.MemorySrvs.MemorySrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// خاطرات روزانه کاربر و پت
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class MemoryController : ControllerBase
    {
        private readonly IMemoryService _memoryService;
        private readonly ICurrentUserHelper _currentUser;

        public MemoryController(
            IMemoryService memoryService,
            ICurrentUserHelper currentUser)
        {
            _memoryService = memoryService;
            _currentUser = currentUser;
        }

        /// <summary>
        /// دریافت جزئیات یک خاطره متعلق به کاربر
        /// </summary>
        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(BaseResultDto<MemoryVDto>), 200)]
        public async Task<IActionResult> Get(long id, CancellationToken cancellationToken)
        {
            var result = await _memoryService.FindAsync(
                id,
                _currentUser.CurrentUser.UserId,
                cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// جستجو و دریافت خاطرات کاربر براساس متن، پت و تاریخ
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MemorySearchDto), 200)]
        public async Task<IActionResult> Get(
            [FromQuery] MemoryInputDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _memoryService.SearchAsync(
                dto,
                _currentUser.CurrentUser.UserId,
                cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// ثبت خاطره جدید
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<MemoryVDto>), 200)]
        public async Task<IActionResult> Post(
            [FromBody] MemoryDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _memoryService.InsertAsync(
                _currentUser.CurrentUser.UserId,
                dto,
                cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// ویرایش خاطره متعلق به کاربر
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<MemoryVDto>), 200)]
        public async Task<IActionResult> Put(
            [FromBody] MemoryDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _memoryService.UpdateAsync(
                _currentUser.CurrentUser.UserId,
                dto,
                cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// حذف خاطره متعلق به کاربر
        /// </summary>
        [HttpDelete("{id:long}")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
        {
            var result = await _memoryService.DeleteAsync(
                _currentUser.CurrentUser.UserId,
                id,
                cancellationToken);
            return Ok(result);
        }
    }
}
