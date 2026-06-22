using Application.Common.Service;
using Application.Services.Accounting.PetSrv.Dto;
using Application.Services.Accounting.PetSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Persistence.Interface;
using System.Linq;

namespace Application.Services.Accounting.PetSrv
{
    public class PetService : CommonSrv<Pet, PetDto>, IPetService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        public PetService(IDataBaseContext _context, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }
        public PetSearchDto Search(PetInputDto baseSearchDto)
        {
            {
                var model = _context.Pets.AsQueryable().Where(s => s.Deleted == false);
                if (baseSearchDto.Available.HasValue)
                {
                    model = model.Where(s => s.Active == baseSearchDto.Available.Value);
                }
                switch (baseSearchDto.SortBy)
                {
                    case Common.Enumerable.SortEnum.New:
                        {
                            model = model.OrderByDescending(s => s.Id);
                            break;
                        }
                    case Common.Enumerable.SortEnum.Old:
                        {
                            model = model.OrderBy(s => s.Id);
                            break;
                        }
                    case Common.Enumerable.SortEnum.Name:
                        {
                            model = model.OrderByDescending(s => s.Name);
                            break;
                        }
                    case Common.Enumerable.SortEnum.MorePriority:
                        {
                            model = model.OrderBy(s => s.Priority);
                            break;
                        }
                    case Common.Enumerable.SortEnum.LessPriority:
                        {
                            model = model.OrderByDescending(s => s.Priority);
                            break;
                        }
                    default:
                        break;
                }
                return new PetSearchDto(baseSearchDto, model, mapper);
            }
        }
    }
}
