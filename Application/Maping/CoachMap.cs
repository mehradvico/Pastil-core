using Application.Services.CoachSrvs.CoachSrv.Dto;
using AutoMapper;
using System.Linq;
using Entities.Entities.CoachField;

namespace Application.Maping
{
    /// <summary>نگاشت‌های مربی (Coach*)؛ جدا از AllMap تا آن فایل بزرگ‌تر نشود. Pointها از نگاشت مشترک AllMap می‌آیند.</summary>
    public class CoachMap : Profile
    {
        public CoachMap()
        {
            CreateMap<CoachDto, Coach>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.Name, o => o.MapFrom(s => s.Name));

            CreateMap<Coach, CoachMinVDto>();
            CreateMap<Coach, CoachVDto>()
                .ForMember(d => d.PetIds, o => o.Ignore());
            CreateMap<Coach, SearchCoachDto>();
            CreateMap<Entities.Entities.Security.User, CoachOwnerVDto>();
            CreateMap<Entities.Entities.ProviderField.Provider, CoachProviderVDto>();
            CreateMap<CoachServicePackage, Application.Services.CoachSrvs.CoachCatalogSrv.Dto.CoachServicePackageVDto>()
                .ForMember(d => d.Types, o => o.Ignore());
        }
    }
}
