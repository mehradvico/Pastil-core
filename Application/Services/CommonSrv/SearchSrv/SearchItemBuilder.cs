using Application.Common.Enumerable;
using Application.Common.Helpers;
using Application.Services.CommonSrv.SearchSrv.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.CommonSrv.SearchSrv
{
    internal static class SearchItemsBuilder
    {
        public static List<SearchItemDto> Build(SearchDto result, SearchRequestDto request)
        {
            var q = request.Q;

            var items = new List<SearchItemDto>(128);

            if (result.Products != null)
            {
                items.AddRange(result.Products.Select(p => new SearchItemDto
                {
                    Type = SearchItemType.Product,
                    Id = p.Id,
                    Title = p.Name,
                    SubTitle = $"{p.StoreName} · {p.Price:N0} تومان",
                    Picture = p.Picture,
                    Score = Score(request, p.Name, p.SecondName, p.BrandName, p.CategoryName, p.StoreName) + 5,
                    Url = $"/product/{p.Id}",
                    MatchedBy = MatchField(q, ("name", p.Name), ("brand", p.BrandName), ("category", p.CategoryName), ("store", p.StoreName))
                }));
            }

            if (result.Brands != null)
            {
                items.AddRange(result.Brands.Select(b => new SearchItemDto
                {
                    Type = SearchItemType.Brand,
                    Id = b.Id,
                    Title = b.Name,
                    SubTitle = "برند",
                    Picture = b.Icon,
                    Score = Score(request, b.Name, b.SecondName),
                    Url = $"/brand/{b.Id}",
                    MatchedBy = MatchField(q, ("name", b.Name), ("secondName", b.SecondName))
                }));
            }

            if (result.Categories != null)
            {
                items.AddRange(result.Categories.Select(c => new SearchItemDto
                {
                    Type = SearchItemType.Category,
                    Id = c.Id,
                    Title = c.Name,
                    SubTitle = "دسته‌بندی",
                    Picture = c.Icon,
                    Score = Score(request, c.Name, c.Label),
                    Url = $"/category/{c.Id}",
                    MatchedBy = MatchField(q, ("name", c.Name), ("label", c.Label))
                }));
            }

            if (result.Feature != null)
            {
                items.AddRange(result.Feature.Select(f => new SearchItemDto
                {
                    Type = SearchItemType.FeatureItem,
                    Id = f.Id,
                    Title = f.Name,
                    SubTitle = "ویژگی",
                    Picture = null,
                    Score = Score(request, f.Name, f.FeatureName),
                    Url = $"/feature/{f.Id}",
                    MatchedBy = MatchField(q, ("name", f.Name), ("feature", f.FeatureName))
                }));
            }

            if (result.Companions != null)
            {
                items.AddRange(result.Companions.Select(c => new SearchItemDto
                {
                    Type = SearchItemType.Companion,
                    Id = c.Id,
                    Title = c.Name,
                    SubTitle = "همیار",
                    Picture = c.Icon,
                    Score = Score(request, c.Name) + Math.Min(10, c.RateAvg),
                    Url = $"/companion/{c.Id}",
                    MatchedBy = MatchField(q, ("name", c.Name))
                }));
            }

            if (result.Assistances != null)
            {
                items.AddRange(result.Assistances.Select(a => new SearchItemDto
                {
                    Type = SearchItemType.Assistance,
                    Id = a.Id,
                    Title = a.Name,
                    SubTitle = "خدمت",
                    Picture = a.Picture,
                    Score = Score(request, a.Name),
                    Url = $"/assistance/{a.Id}",
                    MatchedBy = MatchField(q, ("name", a.Name))
                }));
            }

            if (result.Stores != null)
            {
                items.AddRange(result.Stores.Select(s => new SearchItemDto
                {
                    Type = SearchItemType.Store,
                    Id = s.Id,
                    Title = s.Name,
                    SubTitle = "فروشگاه",
                    Picture = s.Icon ?? s.Picture,
                    Score = Score(request, s.Name) + Math.Min(10, s.RateAvg),
                    Url = $"/store/{s.Id}",
                    MatchedBy = MatchField(q, ("name", s.Name))
                }));
            }

            if (result.Pansions != null)
            {
                items.AddRange(result.Pansions.Select(p => new SearchItemDto
                {
                    Type = SearchItemType.Pansion,
                    Id = p.Id,
                    Title = p.Name,
                    SubTitle = "پانسیون",
                    Picture = p.Picture,
                    Score = Score(request, p.Name) + Math.Min(10, p.RateAvg),
                    Url = $"/pansion/{p.Id}",
                    MatchedBy = MatchField(q, ("name", p.Name))
                }));
            }

            if (result.Packages != null)
            {
                items.AddRange(result.Packages.Select(package => new SearchItemDto
                {
                    Type = SearchItemType.CompanionAssistancePackage,
                    Id = package.Id,
                    Title = package.Name,
                    SubTitle = $"{package.CompanionName} · {package.AssistanceName} · {package.Price:N0} تومان",
                    Picture = package.Picture,
                    Score = Score(request, package.Name, package.CompanionName, package.AssistanceName, package.Description) + 5,
                    Url = $"/companion-assistance-package/{package.Id}",
                    MatchedBy = MatchField(q, ("name", package.Name), ("companion", package.CompanionName), ("assistance", package.AssistanceName), ("description", package.Description))
                }));
            }

            return items
                .Where(item => item.Score >= 20)
                .OrderByDescending(x => x.Score)
                .ThenBy(x => (int)x.Type)
                .ToList();
        }

        private static string MatchField(string query, params (string Name, string Value)[] fields)
        {
            var normalizedQuery = SearchNormalizeHelper.Normalize(query);
            return fields
                .Where(field => !string.IsNullOrWhiteSpace(field.Value) && SearchNormalizeHelper.Normalize(field.Value).Contains(normalizedQuery))
                .Select(field => field.Name)
                .FirstOrDefault() ?? "fuzzy";
        }

        private static double Score(SearchRequestDto request, string title, params string[] secondaryTexts)
        {
            var terms = request.SearchTerms
                .Where(term => term.Length >= 3)
                .DefaultIfEmpty(request.Q);

            return terms.Max(term =>
            {
                var score = SearchScoreHelper.Score(title, term, secondaryTexts);
                return term.Equals(request.Q, StringComparison.OrdinalIgnoreCase) ? score : score * 0.9;
            });
        }
    }
}
