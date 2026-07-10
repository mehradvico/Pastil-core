using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Common.Service;
using Application.Services.LocationFields.CountrySrv.Dto;
using Application.Services.LocationFields.CountrySrv.Iface;
using AutoMapper;
using Entities.Entities;
using Persistence.Interface;
using System.Linq;

namespace Application.Services.LocationFields.CountrySrv
{
    public class CountryService : CommonSrv<Country, CountryDto>, ICountryService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;

        public CountryService(IDataBaseContext _context, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }
        public BaseSearchDto<CountryVDto> Search(BaseInputDto baseSearchDto)
        {
            var model = _context.Countries.AsQueryable();
            if (!string.IsNullOrEmpty(baseSearchDto.Q))
            {
                model = model.Where(s => s.Name.Contains(baseSearchDto.Q)).OrderBy(o => o.Id);
            }
            return new BaseSearchDto<Country, CountryVDto>(baseSearchDto, model, mapper);
        }
    }
}
