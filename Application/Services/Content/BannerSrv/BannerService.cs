using Application.Common.Dto.Result;
using Application.Common.Service;
using Application.Services.Content.BannerSrv.Dto;
using Application.Services.Content.BannerSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.Content.BannerSrv
{
    public class BannerService : CommonSrv<Banner, BannerDto>, IBannerService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly IQueryable<Banner> _baseQuery;

        public BannerService(IDataBaseContext _context, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this._baseQuery = _context.Banners.Where(s => s.Deleted == false);
        }

        public async Task<BaseResultDto<BannerVDto>> FindAsyncVDto(long id)
        {

            var item = await _baseQuery.Include(s => s.Picture2).Include(s => s.Picture).FirstOrDefaultAsync(s => s.Id == id);
            if (item != null)
            {
                return new BaseResultDto<BannerVDto>(true, mapper.Map<BannerVDto>(item));
            }
            return new BaseResultDto<BannerVDto>(false, mapper.Map<BannerVDto>(item));
        }

        public BaseSearchDto<BannerVDto> Search(BannerInputDto searchDto)
        {
            var query = _context.Banners.Include(s => s.Picture2).Include(s => s.Picture).Include(s => s.Category).AsQueryable();

            if (searchDto.CategoryId.HasValue)
            {
                query = query.Where(s => s.CategoryId == searchDto.CategoryId);
            }
            else if (!string.IsNullOrEmpty(searchDto.CategoryLabel))
            {
                query = query.Where(s => s.Category.Label == searchDto.CategoryLabel);
            }
            if (searchDto.Available.HasValue)
            {
                query = query.Where(s => s.Active == searchDto.Available);
            }
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
                    case Common.Enumerable.SortEnum.MoreVisit:
                        {
                            query = query.OrderByDescending(s => s.ClickCount);
                            break;
                        }
                    case Common.Enumerable.SortEnum.LessVisit:
                        {
                            query = query.OrderBy(s => s.ClickCount);
                            break;
                        }
                    case Common.Enumerable.SortEnum.MorePriority:
                        {
                            query = query.OrderByDescending(s => s.Priority);
                            break;
                        }
                    case Common.Enumerable.SortEnum.LessPriority:
                        {
                            query = query.OrderBy(s => s.Priority);
                            break;
                        }

                    default:
                        break;
                }
            }
            return new BaseSearchDto<Banner, BannerVDto>(searchDto, query, mapper);
        }


    }
}
