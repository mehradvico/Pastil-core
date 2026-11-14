using Application.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.CommonSrv.SearchSrv
{
    /// <summary>پلِ بین واژگان هوشمند (<see cref="SearchQueryPlan"/>) و جستجوی اصلی که با «فهرست عبارت‌ها» کار می‌کند.</summary>
    public static class SmartSearchTerms
    {
        private const int MaxTerms = 20;

        /// <summary>
        /// عبارت‌های جستجو: خودِ عبارت، عبارت اصلاح‌شده، معادل‌های واقعی هر کلمه و بعد عبارت‌های قبلی (مترادف‌ها و …).
        /// معادل‌ها جلوتر می‌آیند تا سقف ۲۰ عبارت آن‌ها را حذف نکند.
        /// </summary>
        public static string[] Build(string normalizedQuery, SearchQueryPlan plan, bool enableFuzzy)
        {
            var terms = new List<string> { normalizedQuery };
            if (!string.IsNullOrEmpty(plan.CorrectedQuery))
                terms.Add(plan.CorrectedQuery);

            foreach (var word in plan.Words)
            {
                terms.AddRange(word.Tokens);
                // شکل خامِ دیتابیس (مثلاً «آرایش» با مد) تا LIKE به collation وابسته نباشد
                terms.AddRange(word.LikeTerms.Where(term => term.Length == 1).Select(term => term[0]));
            }

            terms.AddRange(SearchNormalizeHelper.BuildTerms(normalizedQuery, enableFuzzy));

            return terms
                .Where(term => !string.IsNullOrWhiteSpace(term) && term.Length >= 2)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(MaxTerms)
                .ToArray();
        }

        /// <summary>
        /// برای هر کلمه‌ی عبارت: خودش، مترادف‌هایش و معادل‌های واقعی‌اش. کلمه‌ی کنارگذاشته (<see cref="SearchQueryPlan.IgnoredWords"/>)
        /// حساب نمی‌شود، وگرنه همه‌ی نتیجه‌ها را به یک اندازه پایین می‌کشید.
        /// </summary>
        public static List<string[]> WordGroups(SearchQueryPlan plan)
        {
            var known = plan.Words.Where(word => !plan.IgnoredWords.Contains(word.Word)).ToList();

            return known
                .Select(word => SearchNormalizeHelper.BuildTerms(word.Word, enableFuzzy: false)
                    .Concat(word.Tokens)
                    .Append(word.Word)
                    .Where(term => term.Length >= 2)
                    .Distinct()
                    .ToArray())
                .Where(group => group.Length > 0)
                .ToList();
        }
    }
}
