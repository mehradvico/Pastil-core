using Application.Common.Helpers;
using System.Linq;

namespace Application.Services.CommonSrv.SearchSrv
{
    internal static class SearchScoreHelper
    {
        public static double Score(string title, string query)
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

            return score;
        }
    }
}
