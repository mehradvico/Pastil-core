using Application.Common.Dto.Result;
using Application.Common.Service;
using Application.Services.Accounting.PetBreedCharacteristicSrv.Dto;
using Application.Services.Accounting.PetBreedCharacteristicSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.Accounting.PetBreedCharacteristicSrv
{
    public class PetBreedCharacteristicService : CommonSrv<PetBreedCharacteristic, PetBreedCharacteristicDto>, IPetBreedCharacteristicService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        public PetBreedCharacteristicService(IDataBaseContext _context, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }

        public async Task<BaseResultDto<PetBreedCharacteristicVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.PetBreedCharacteristics.FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);
            if (item != null)
                return new BaseResultDto<PetBreedCharacteristicVDto>(true, mapper.Map<PetBreedCharacteristicVDto>(item));
            return new BaseResultDto<PetBreedCharacteristicVDto>(false, mapper.Map<PetBreedCharacteristicVDto>(item));
        }

        public PetBreedCharacteristicSearchDto Search(PetBreedCharacteristicInputDto searchDto)
        {
            var model = _context.PetBreedCharacteristics.AsQueryable().Where(s => !s.Deleted);
            if (searchDto.PetBreedId.HasValue)
            {
                model = model.Where(s => s.PetBreedId == searchDto.PetBreedId.Value);
            }
            model = model.OrderBy(s => s.Priority);
            return new PetBreedCharacteristicSearchDto(searchDto, model, mapper);
        }
    }
}
