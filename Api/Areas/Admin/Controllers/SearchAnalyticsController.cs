using Application.Common.Dto.Result;
using Application.Services.CommonSrv.SearchSrv.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// آمار عبارت‌های جستجو شده و جستجوهای بدون نتیجه
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class SearchAnalyticsController : ControllerBase
    {
        private readonly IDataBaseContext _context;

        public SearchAnalyticsController(IDataBaseContext context)
        {
            _context = context;
        }

        /// <summary>
        /// دریافت پرتکرارترین عبارت‌های جستجو
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] int days = 30,
            [FromQuery] int take = 50,
            [FromQuery] bool zeroResultOnly = false,
            CancellationToken cancellationToken = default)
        {
            days = Math.Clamp(days, 1, 365);
            take = Math.Clamp(take, 1, 200);
            var fromDate = DateTime.UtcNow.AddDays(-days);

            var query = _context.SearchQueryLogs
                .AsNoTracking()
                .Where(item => item.CreateDateUtc >= fromDate);

            if (zeroResultOnly)
                query = query.Where(item => item.ResultCount == 0);

            var result = await query
                .GroupBy(item => new { item.NormalizedQuery, item.Channel })
                .Select(group => new SearchAnalyticsDto
                {
                    Query = group.Key.NormalizedQuery,
                    Channel = group.Key.Channel,
                    SearchCount = group.Count(),
                    ZeroResultCount = group.Count(item => item.ResultCount == 0),
                    AverageResultCount = group.Average(item => item.ResultCount),
                    AverageTookMilliseconds = group.Average(item => item.TookMilliseconds),
                    LastSearchDateUtc = group.Max(item => item.CreateDateUtc)
                })
                .OrderByDescending(item => item.SearchCount)
                .ThenByDescending(item => item.LastSearchDateUtc)
                .Take(take)
                .ToListAsync(cancellationToken);

            return Ok(new BaseResultDto<List<SearchAnalyticsDto>>(true, result));
        }
    }
}
