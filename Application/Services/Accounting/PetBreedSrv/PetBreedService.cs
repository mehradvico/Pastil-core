using Application.Common.Dto.Result;
using Application.Common.Service;
using Application.Services.Accounting.PetBreedBreedSrv.Dto;
using Application.Services.Accounting.PetBreedBreedSrv.Iface;
using Application.Services.Accounting.PetBreedSrv.Dto;
using Application.Services.Accounting.PetBreedSrv.Iface;
using Application.Services.Accounting.TicketSrv.Dto;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Accounting.PetBreedBreedSrv
{
    public class PetBreedService : CommonSrv<PetBreed, PetBreedDto>, IPetBreedService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        public PetBreedService(IDataBaseContext _context, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }

        public async Task<BaseResultDto<PetBreedVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.PetBreeds.Include(s => s.Picture).Include(s => s.Pet).FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);

            if (item == null)
                return new BaseResultDto<PetBreedVDto>(false, null);

            var dto = mapper.Map<PetBreedVDto>(item);
            return new BaseResultDto<PetBreedVDto>(true, dto);
        }

        public PetBreedSearchDto Search(PetBreedInputDto baseSearchDto)
        {
            var model = _context.PetBreeds.Include(s => s.Picture).Include(s => s.Pet).AsQueryable().Where(s => !s.Deleted);

            if (baseSearchDto.PetId.HasValue && baseSearchDto.PetId.Value > 0)
                model = model.Where(s => s.PetId == baseSearchDto.PetId.Value);

            switch (baseSearchDto.SortBy)
            {
                case Common.Enumerable.SortEnum.New:
                    model = model.OrderByDescending(s => s.Id);
                    break;

                case Common.Enumerable.SortEnum.Old:
                    model = model.OrderBy(s => s.Id);
                    break;

                case Common.Enumerable.SortEnum.Name:
                    model = model.OrderByDescending(s => s.Name);
                    break;

                case Common.Enumerable.SortEnum.MorePriority:
                    model = model.OrderBy(s => s.Priority);
                    break;

                case Common.Enumerable.SortEnum.LessPriority:
                    model = model.OrderByDescending(s => s.Priority);
                    break;
            }

            return new PetBreedSearchDto(baseSearchDto, model, mapper);
        }
    }
}