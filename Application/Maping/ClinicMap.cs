using Application.Services.ClinicSrvs.ClinicSrv.Dto;
using AutoMapper;
using Entities.Entities.ClinicField;

namespace Application.Maping
{
    /// <summary>نگاشت‌های کلینیک (Clinic*)؛ جدا از AllMap تا آن فایل بزرگ‌تر نشود. Pointها از نگاشت مشترک AllMap می‌آیند.</summary>
    public class ClinicMap : Profile
    {
        public ClinicMap()
        {
            CreateMap<ClinicDto, Clinic>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.Name, o => o.MapFrom(s => s.Name));

            CreateMap<Clinic, ClinicMinVDto>();
            CreateMap<Clinic, ClinicVDto>()
                .ForMember(d => d.PetIds, o => o.Ignore())
                .ForMember(d => d.TypeIds, o => o.Ignore());
            CreateMap<Clinic, SearchClinicDto>();
            CreateMap<Entities.Entities.Security.User, ClinicOwnerVDto>();
            CreateMap<Entities.Entities.ProviderField.Provider, ClinicProviderVDto>();
            CreateMap<ClinicServicePackage, Application.Services.ClinicSrvs.ClinicCatalogSrv.Dto.ClinicServicePackageVDto>()
                .ForMember(d => d.Types, o => o.Ignore());
        }
    }
}
