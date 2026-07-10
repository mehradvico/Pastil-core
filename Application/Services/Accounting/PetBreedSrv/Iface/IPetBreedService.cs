using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Accounting.PetBreedBreedSrv.Dto;
using Application.Services.Accounting.PetBreedSrv.Dto;
using Application.Services.Accounting.TicketSrv.Dto;
using Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Accounting.PetBreedBreedSrv.Iface
{
    public interface IPetBreedService : ICommonSrv<PetBreed, PetBreedDto>
    {
        PetBreedSearchDto Search(PetBreedInputDto baseSearchDto);
        Task<BaseResultDto<PetBreedVDto>> FindAsyncVDto(long id);
    }
}
