using Application.Common.Dto.Result;
using Application.Common.Service;
using Application.Services.Accounting.UserPetSrv.Dto;
using Application.Services.Accounting.UserPetSrv.Iface;
using Application.Services.PastilClubSrvs.PetProfileSrv.Iface;
using Application.Services.PastilClubSrvs.PointEventSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.Accounting.UserPetSrv
{
    public class UserPetService : CommonSrv<UserPet, UserPetDto>, IUserPetService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly IClubPetProfileCompletionService _profileCompletionService;
        private readonly IClubPointIntegrationService _clubPointIntegrationService;

        public UserPetService(
            IDataBaseContext _context,
            IMapper mapper,
            IClubPetProfileCompletionService profileCompletionService,
            IClubPointIntegrationService clubPointIntegrationService) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            _profileCompletionService = profileCompletionService;
            _clubPointIntegrationService = clubPointIntegrationService;
        }

        public override async Task<BaseResultDto<UserPetDto>> InsertAsyncDto(UserPetDto dto)
        {
            var result = await base.InsertAsyncDto(dto);
            if (!result.IsSuccess || result.Data == null)
                return result;

            var userPet = await _context.UserPets.AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == result.Data.Id);
            if (_profileCompletionService.IsComplete(userPet))
                await _clubPointIntegrationService.PetProfileCompletedAsync(userPet.UserId, userPet.Id);

            return result;
        }

        public async Task<BaseResultDto<UserPetDto>> UpdateAsyncDto(UserPetDto dto)
        {
            var userPet = await _context.UserPets.AsTracking()
                .FirstOrDefaultAsync(item =>
                    item.Id == dto.Id &&
                    item.UserId == dto.UserId &&
                    !item.Deleted);
            if (userPet == null)
                return new BaseResultDto<UserPetDto>(false, Resource.Notification.NothingFound, dto);

            mapper.Map(dto, userPet);
            await _context.SaveChangesAsync();

            if (_profileCompletionService.IsComplete(userPet))
                await _clubPointIntegrationService.PetProfileCompletedAsync(userPet.UserId, userPet.Id);

            return new BaseResultDto<UserPetDto>(true, mapper.Map<UserPetDto>(userPet));
        }

        public async Task<BaseResultDto<UserPetVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.UserPets.Include(s => s.User).Include(s => s.PetBreed).Include(s => s.PetBreed2).Include(s => s.Pet).Include(s => s.Picture).Include(s => s.UserPetPictures).ThenInclude(s => s.Picture).Where(s => s.Deleted == false).FirstOrDefaultAsync(s => s.Id == id && s.Active && s.Deleted == false);
            if (item != null)
                return new BaseResultDto<UserPetVDto>(true, mapper.Map<UserPetVDto>(item));
            return new BaseResultDto<UserPetVDto>(false, mapper.Map<UserPetVDto>(item));
        }

        public UserPetSearchDto Search(UserPetInputDto baseSearchDto)
        {
            var model = _context.UserPets.Include(s => s.User).Include(s => s.PetBreed).Include(s => s.PetBreed2).Include(s => s.Pet).Include(s => s.Picture).AsQueryable().Where(s => s.Deleted == false);
            if (baseSearchDto.UserId.HasValue)
            {
                model = model.Where(s => s.UserId == baseSearchDto.UserId.Value);
            }
            if (baseSearchDto.IsSterile.HasValue)
            {
                model = model.Where(s => s.IsSterile == baseSearchDto.IsSterile.Value);
            }
            if (baseSearchDto.PetBreedId.HasValue)
            {
                var petBreedId = baseSearchDto.PetBreedId.Value;

                model = model.Where(s => s.PetBreedId == petBreedId || s.PetBreed2Id == petBreedId);
            }
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
                default:
                    break;
            }
            return new UserPetSearchDto(baseSearchDto, model, mapper);
        }
    }
}
