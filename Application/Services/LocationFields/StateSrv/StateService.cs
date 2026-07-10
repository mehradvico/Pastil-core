using AngleSharp.Dom;
using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Common.Service;
using Application.Services.LocationFields.StateSrv.Dto;
using Application.Services.LocationFields.StateSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.LocationFields.StateSrv
{
    public class StateService : CommonSrv<State, StateDto>, IStateService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;

        public StateService(IDataBaseContext _context, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }

        public override async Task<BaseResultDto<StateDto>> FindAsyncDto(long id)
        {
            var item = await _context.States.Include(s => s.Country).FirstOrDefaultAsync(s => s.Id == id);
            if (item != null)
                return new BaseResultDto<StateDto>(true, mapper.Map<StateDto>(item));
            return new BaseResultDto<StateDto>(false, mapper.Map<StateDto>(item));
        }

        public BaseSearchDto<StateVDto> Search(StateInputDto baseSearchDto)
        {
            var model = _context.States.Include(s => s.Country).AsQueryable();
            if (!string.IsNullOrEmpty(baseSearchDto.Q))
            {
                model = model.Where(s => s.Name.Contains(baseSearchDto.Q) || s.Country.Name.Contains(baseSearchDto.Q)).OrderBy(o => o.Id);
            }
            if (baseSearchDto.CountryId.HasValue)
            {
                model = model.Where(s => s.CountryId == baseSearchDto.CountryId.Value);
            }
            return new BaseSearchDto<State, StateVDto>(baseSearchDto, model, mapper);
        }
    }
}
