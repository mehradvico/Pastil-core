using Application.Common.Helpers;
using System.Linq;
using System;

namespace Application.Services.CommonSrv.SearchSrv
{
    internal static class SearchScoreHelper
    {
        public static double Score(string title, string query, params string[] secondaryTexts)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(query))
                return 0;

            var t = SearchNormalizeHelper.Normalize(title);
            var q = query;

            var tNoSpace = t.Replace(" ", "");
            var qNoSpace = q.Replace(" ", "");

            var tTokens = t.Split(' ');
            var qTokens = q.Split(' ');

            double score = 0;

            if (t == q) score = 100;
            else if (tNoSpace == qNoSpace) score = 95;
            else if (t.StartsWith(q)) score = 85;
            else if (tNoSpace.StartsWith(qNoSpace)) score = 80;
            else if (t.Contains(q)) score = 65;
            else if (tNoSpace.Contains(qNoSpace)) score = 60;

            if (tTokens.Any(x => x == q))
                score += 10;

            if (qTokens.All(qt => tTokens.Any(tt => tt == qt)))
                score += 5;

            var tokenMatches = qTokens.Count(qt => tTokens.Any(tt => tt.Contains(qt) || qt.Contains(tt)));
            score += qTokens.Length == 0 ? 0 : 15d * tokenMatches / qTokens.Length;

            if (query.Length >= 3)
            {
                var distance = LevenshteinDistance(tNoSpace, qNoSpace);
                var maximumLength = Math.Max(tNoSpace.Length, qNoSpace.Length);
                if (maximumLength > 0)
                {
                    var similarity = 1d - (double)distance / maximumLength;
                    if (similarity >= 0.72)
                        score = Math.Max(score, 45 + similarity * 30);
                }
            }

            foreach (var secondaryText in secondaryTexts ?? [])
                score = Math.Max(score, ScoreSecondary(secondaryText, query));

            return Math.Min(120, score);
        }

        private static double ScoreSecondary(string value, string query)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0;
            var text = SearchNormalizeHelper.Normalize(value);
            if (text == query) return 75;
            if (text.StartsWith(query)) return 62;
            if (text.Contains(query)) return 48;
            return query.Split(' ', StringSplitOptions.RemoveEmptyEntries).Count(text.Contains) * 12;
        }

        private static int LevenshteinDistance(string source, string target)
        {
            if (source.Length == 0) return target.Length;
            if (target.Length == 0) return source.Length;

            var previous = Enumerable.Range(0, target.Length + 1).ToArray();
            var current = new int[target.Length + 1];

            for (var i = 1; i <= source.Length; i++)
            {
                current[0] = i;
                for (var j = 1; j <= target.Length; j++)
                {
                    var cost = source[i - 1] == target[j - 1] ? 0 : 1;
                    current[j] = Math.Min(Math.Min(current[j - 1] + 1, previous[j] + 1), previous[j - 1] + cost);
                }
                (previous, current) = (current, previous);
            }

            return previous[target.Length];
        }
    }
}
