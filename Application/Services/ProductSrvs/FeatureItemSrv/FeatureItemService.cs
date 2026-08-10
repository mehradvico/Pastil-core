using Application.Common.Service;
using Application.Services.CommonSrv.SearchSrv.Dto;
using Application.Services.ProductSrvs.FeatureItemSrv.Dto;
using Application.Services.ProductSrvs.FeatureSrv.Dto;
using Application.Services.ProductSrvs.FeatureSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Application.Common.Helpers;

namespace Application.Services.ProductSrvs.FeatureItemSrv
{
    public class FeatureItemService : CommonSrv<FeatureItem, FeatureItemDto>, IFeatureItemService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        public FeatureItemService(IDataBaseContext context, IMapper mapper) : base(context, mapper)
        {
            this._context = context;
            this.mapper = mapper;
        }
        public FeatureItemSearchDto Search(FeatureItemInputDto searchDto)
        {
            var query = _context.FeatureItems.Where(s => s.FeatureId == searchDto.FeatureId).AsQueryable();

            if (!string.IsNullOrEmpty(searchDto.Q))
            {
                query = query.Where(s => s.Name.Contains(searchDto.Q));
            }
            if (searchDto.SortBy != Common.Enumerable.SortEnum.Default)
            {
                switch (searchDto.SortBy)
                {
                    case Common.Enumerable.SortEnum.Default:
                        {
                            query = query.OrderBy(s => s.Priority);

                            break;
                        }
                    case Common.Enumerable.SortEnum.New:
                        {
                            query = query.OrderByDescending(s => s.Id);
                            break;
                        }
                    case Common.Enumerable.SortEnum.Old:
                        {
                            query = query.OrderBy(s => s.Id);
                            break;
                        }
                    case Common.Enumerable.SortEnum.Name:
                        {
                            query = query.OrderByDescending(s => s.Name);
                            break;
                        }

                    case Common.Enumerable.SortEnum.MorePriority:
                        {
                            query = query.OrderBy(s => s.Priority);
                            break;
                        }
                    case Common.Enumerable.SortEnum.LessPriority:
                        {
                            query = query.OrderByDescending(s => s.Priority);
                            break;
                        }
                    default:
                        break;
                }
            }
            return new FeatureItemSearchDto(searchDto, query, mapper);
        }
        public async Task<List<SearchFeatureItemDto>> SearchMinAsync(SearchRequestDto request)
        {
            var predicate = SearchQueryHelper.ContainsAny<FeatureItem>(request.SearchTerms, item => item.Name, item => item.Feature.Name);
            var model = _context.FeatureItems.Include(s => s.Feature)
                .Where(s => s.Feature.IsGroup && s.Deleted == false && s.Feature.Deleted == false && s.Feature.Active)
                .Where(predicate)
                .Take(SearchQueryHelper.CandidateCount(request.FeatureCount))
                .Select(s => new SearchFeatureItemDto { Id = s.Id, Name = s.Name, FeatureId = s.FeatureId, FeatureName = s.Feature.Name });
            return await model.ToListAsync();
        }
    }
}
