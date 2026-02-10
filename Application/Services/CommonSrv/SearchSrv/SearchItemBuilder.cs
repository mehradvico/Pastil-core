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
                    Score = SearchScoreHelper.Score(p.Name, q),
                    Url = ""
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
                    Score = SearchScoreHelper.Score(b.Name, q),
                    Url = ""
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
                    Score = SearchScoreHelper.Score(c.Name, q),
                    Url = ""
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
                    Score = SearchScoreHelper.Score(f.Name, q),
                    Url = ""
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
                    Score = SearchScoreHelper.Score(c.Name, q),
                    Url = ""
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
                    Score = SearchScoreHelper.Score(a.Name, q),
                    Url = ""
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
                    Score = SearchScoreHelper.Score(s.Name, q),
                    Url = ""
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
                    Score = SearchScoreHelper.Score(p.Name, q),
                    Url = ""
                }));
            }

            const int total = 20;

            return items
                .OrderByDescending(x => x.Score)
                .ThenBy(x => (int)x.Type)
                .Take(total)
                .ToList();
        }
    }
}
