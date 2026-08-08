using Application.Common.Dto.Result;
using Application.Common.Dto.Field;
using Application.Common.Service;
using Application.Services.Content.BannerSrv.Dto;
using Application.Services.Content.BannerSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System.Net;
using System.Linq;
using System.Text.RegularExpressions;
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
                var result = mapper.Map<BannerVDto>(item);
                SanitizeTextFields(result);
                return new BaseResultDto<BannerVDto>(true, result);
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
            var result = new BaseSearchDto<Banner, BannerVDto>(searchDto, query, mapper);
            result.List.ForEach(SanitizeTextFields);
            return result;
        }

        public override async Task<BaseResultDto<BannerDto>> InsertAsyncDto(BannerDto dto)
        {
            SanitizeTextFields(dto);
            return await base.InsertAsyncDto(dto);
        }

        public override BaseResultDto UpdateDto(BannerDto dto)
        {
            SanitizeTextFields(dto);
            return base.UpdateDto(dto);
        }

        private static void SanitizeTextFields(FullName_FieldDto dto)
        {
            if (dto == null)
            {
                return;
            }

            dto.Summary = ToPlainText(dto.Summary);
            dto.Description = ToPlainText(dto.Description);
        }

        private static string ToPlainText(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var decoded = WebUtility.HtmlDecode(value);
            decoded = Regex.Replace(
                decoded,
                @"<(script|style)\b[^>]*>[\s\S]*?</\1>",
                " ",
                RegexOptions.IgnoreCase);
            decoded = Regex.Replace(decoded, @"<br\s*/?>", "\n", RegexOptions.IgnoreCase);
            decoded = Regex.Replace(
                decoded,
                @"</(p|div|li|h[1-6])>",
                "\n",
                RegexOptions.IgnoreCase);
            decoded = Regex.Replace(decoded, @"<[^>]+>", " ");
            decoded = Regex.Replace(decoded, @"[^\S\r\n]+", " ");
            decoded = Regex.Replace(decoded, @" *(\r?\n) *", "$1");
            decoded = Regex.Replace(decoded, @"(\r?\n){3,}", "\n\n");

            return decoded.Trim();
        }

    }
}
