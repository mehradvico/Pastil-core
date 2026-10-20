using Application.Services.GroomerSrvs.GroomerSrv.Dto;
using AutoMapper;
using Entities.Entities.GroomerField;

namespace Application.Maping
{
    /// <summary>نگاشت‌های آرایشگاه (Groomer*)؛ جدا از AllMap تا آن فایل بزرگ‌تر نشود. Pointها از نگاشت مشترک AllMap می‌آیند.</summary>
    public class GroomerMap : Profile
    {
        public GroomerMap()
        {
            CreateMap<GroomerDto, Groomer>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.Name, o => o.MapFrom(s => s.Name));

            CreateMap<Groomer, GroomerMinVDto>();
            CreateMap<Groomer, GroomerVDto>()
                .ForMember(d => d.PetIds, o => o.Ignore());
            CreateMap<Groomer, SearchGroomerDto>();
            CreateMap<Entities.Entities.Security.User, GroomerOwnerVDto>();
            CreateMap<Entities.Entities.ProviderField.Provider, GroomerProviderVDto>();
            CreateMap<GroomerServicePackage, Application.Services.GroomerSrvs.GroomerCatalogSrv.Dto.GroomerServicePackageVDto>();
        }
    }
}
