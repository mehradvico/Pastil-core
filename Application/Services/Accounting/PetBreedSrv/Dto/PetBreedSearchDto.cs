using Application.Common.Dto.Result;
using Application.Services.Accounting.PetBreedSrv.Dto;
using Application.Services.Accounting.PetBreedSrv.Iface;
using AutoMapper;
using Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Accounting.PetBreedBreedSrv.Dto
{
    public class PetBreedSearchDto : BaseSearchDto<PetBreed, PetBreedVDto>, IPetBreedSearchFields
    {
        public PetBreedSearchDto(PetBreedInputDto dto, IQueryable<PetBreed> list, IMapper mapper) : base(dto, list, mapper)
        {
            this.PetId = dto.PetId;
        }

        public long? PetId { get; set; }
    }
}
