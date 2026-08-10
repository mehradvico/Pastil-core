using Application.Common.Dto.Result;
using Application.Services.Content.BannerSrv.Iface;
using AutoMapper;
using Entities.Entities;
using System.Linq;

namespace Application.Services.Content.BannerSrv.Dto
{
    public class BannerSearchDto : BaseSearchDto<Banner, BannerVDto>, IBannerSearchFields
    {
        public BannerSearchDto(BannerInputDto dto, IQueryable<Banner> list, IMapper mapper) : base(dto, list, mapper)
        {
            this.CategoryId = dto.CategoryId;
            this.CategoryLabel = dto.CategoryLabel;
            this.ShowToApp = dto.ShowToApp;
            this.ShowToSite = dto.ShowToSite;
        }

        public long? CategoryId { get; set; }
        public string CategoryLabel { get; set; }
        public bool? ShowToApp { get; set; }
        public bool? ShowToSite { get; set; }

    }
}
