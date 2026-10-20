using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Microsoft.Extensions.Caching.Memory;
using Application.Services.Content.StoryGroupSrv.Dto;
using Application.Services.Content.StoryGroupSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// مرتبط با استوری ها
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class StoryGroupController : ControllerBase
    {
        private static readonly TimeSpan HomeCacheDuration = TimeSpan.FromMinutes(5);
        private const string HomeCacheKey = "story-group-home";
        private IStoryGroupService StoryGroupService;
        private readonly IMemoryCache _cache;
        public StoryGroupController(IStoryGroupService StoryGroupService, IMemoryCache cache)
        {
            this.StoryGroupService = StoryGroupService;
            this._cache = cache;
        }

        /// <summary>
        /// استوری‌های صفحه‌ی اصلی در یک درخواست: گروه‌های دارای استوری فعال + تعداد + اولین استوری (کش ۵ دقیقه)
        /// </summary>
        [HttpGet("Home")]
        [ProducesResponseType(typeof(BaseResultDto<List<StoryGroupHomeVDto>>), 200)]
        public async Task<IActionResult> Home()
        {
            if (!_cache.TryGetValue(HomeCacheKey, out List<StoryGroupHomeVDto> groups) || groups == null)
            {
                groups = await StoryGroupService.HomeAsync();
                _cache.Set(HomeCacheKey, groups, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = HomeCacheDuration,
                    Size = 1
                });
            }
            return Ok(new BaseResultDto<List<StoryGroupHomeVDto>>(true, groups));
        }


        /// <summary>
        /// جستجو
        /// </summary>
        [HttpGet]
        [CustomOutputCache(CacheTypeEnum.StoryGroupSearch)]
        [ProducesResponseType(typeof(StoryGroupSearchDto), 200)]
        public IActionResult Get([FromQuery] StoryGroupInputDto dto)
        {
            dto.Available = true;
            var post = StoryGroupService.Search(dto);
            return Ok(post);
        }
    }
}
