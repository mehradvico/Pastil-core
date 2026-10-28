using Entities.Entities.CompanionField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.ExpertiseSrv
{
    /// <summary>
    /// قواعد مشترک نقش/تخصص اعضای تیم (کلینیک، مربی، آرایشگاه؛ ۲۰۲۶-۱۰-۲۸) — همان قاعده‌ی تیم نمایندگی (CompanionUserService):
    /// حداکثر ۱۰ تخصص، فقط تخصص فعال و حذف‌نشده؛ اولین مورد «تخصص اصلی» (ExpertiseId) است.
    /// </summary>
    public static class StaffExpertiseRules
    {
        public const int MaxPerUser = 10;

        /// <summary>تخصص‌های معتبر به همان ترتیب ورودی؛ null = ورودی نامعتبر (بیش از ۱۰، تکراری‌ها حذف، شناسه‌ی ناموجود/غیرفعال).</summary>
        public static async Task<List<Expertise>> ResolveAsync(IDataBaseContext context, IEnumerable<long> expertiseIds)
        {
            var ids = (expertiseIds ?? Enumerable.Empty<long>()).Where(x => x > 0).Distinct().ToList();
            if (ids.Count > MaxPerUser)
                return null;
            if (ids.Count == 0)
                return new List<Expertise>();
            var rows = await context.Expertises.AsNoTracking()
                .Where(x => ids.Contains(x.Id) && x.Active && !x.Deleted)
                .ToListAsync();
            if (rows.Count != ids.Count)
                return null;
            return ids.Select(id => rows.First(r => r.Id == id)).ToList();
        }

        // متن نمایشی نقش‌ها: «دامپزشک، جراح»
        public static string JoinNames(IEnumerable<string> names)
        {
            var list = (names ?? Enumerable.Empty<string>()).Where(n => !string.IsNullOrWhiteSpace(n)).Select(n => n.Trim()).Distinct().ToList();
            return list.Count == 0 ? null : string.Join("، ", list);
        }
    }
}
