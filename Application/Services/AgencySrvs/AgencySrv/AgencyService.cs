using Application.Common.Enumerable;
using Application.Services.AgencySrvs.AgencySrv.Dto;
using Application.Services.AgencySrvs.AgencySrv.Iface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.AgencySrvs.AgencySrv
{
    public class AgencyService : IAgencyService
    {
        private readonly IDataBaseContext _context;
        private readonly ILogger<AgencyService> _logger;

        public AgencyService(IDataBaseContext context, ILogger<AgencyService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<AgencySearchVDto> SearchAsync(AgencyInputDto input)
        {
            var pageIndex = Math.Max(1, input.PageIndex);
            var pageSize = Math.Clamp(input.PageSize, 1, 200);
            var take = (int)Math.Min(int.MaxValue, (long)pageIndex * pageSize);
            var now = DateTime.Now;
            var total = 0;
            var candidates = new List<AgencyRow>();

            // هر نوع مرکز جداگانه خوانده می‌شود تا UNION بزرگ بین جدول‌های مستقل
            // نتواند کل فهرست ادمین را با خطای SQL از کار بیندازد.
            foreach (var source in AgencyQueries.Sources(_context, input))
            {
                // خطای یک نوع (مثلاً ستونی که migrationاش روی دیتابیس اعمال نشده) فقط همان نوع را از لیست حذف می‌کند، نه کل لیست ادمین را.
                try
                {
                    var count = await source.CountAsync();
                    var rows = await AgencyQueries.Order(source, input.SortBy, now)
                        .Take(take).AsNoTracking().ToListAsync();
                    total += count;
                    candidates.AddRange(rows);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Agency list: one center kind failed to load and was skipped");
                }
            }

            var ordered = input.SortBy switch
            {
                SortEnum.Old => candidates.OrderBy(r => r.Id).ThenBy(r => r.Kind),
                SortEnum.MorePriority => candidates
                    .OrderBy(r => r.GoldAccountDate.HasValue && r.GoldAccountDate.Value > now ? 0 :
                        r.SilverAccountDate.HasValue && r.SilverAccountDate.Value > now ? 1 : 2)
                    .ThenByDescending(r => r.Id).ThenBy(r => r.Kind),
                _ => candidates.OrderByDescending(r => r.Id).ThenBy(r => r.Kind),
            };
            var skip = (int)Math.Min(int.MaxValue, (long)(pageIndex - 1) * pageSize);
            var list = ordered.Skip(skip).Take(pageSize)
                .Select(r => r.ToView()).ToList();

            return new AgencySearchVDto { PageIndex = pageIndex, PageSize = pageSize, TotalCount = total, List = list };
        }
    }
}
