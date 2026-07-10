using Application.Common.Dto.Result;
using Application.Services.Accounting.UserPetSrv.Iface;
using AutoMapper;
using Entities.Entities;
using System.Linq;

namespace Application.Services.Accounting.UserPetSrv.Dto
{
    public class UserPetSearchDto : BaseSearchDto<UserPet, UserPetVDto>, IUserPetSearchFields
    {
        public UserPetSearchDto(UserPetInputDto dto, IQueryable<UserPet> list, IMapper mapper) : base(dto, list, mapper)
        {
            this.UserId = dto.UserId;
            this.PetBreedId = dto.PetBreedId;
            this.IsSterile = dto.IsSterile;
        }
        public long? UserId { get; set; }
        public long? PetBreedId { get; set; }
        public bool? IsSterile { get; set; }
    }
}
