using Application.Common.Dto.Result;
using Application.Common.Service;
using Application.Services.Accounting.UserPetPictureSrv.Dto;
using Application.Services.Accounting.UserPetPictureSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Accounting.UserPetPictureSrv
{
    public class UserPetPictureService : CommonSrv<UserPetPicture, UserPetPictureDto>, IUserPetPictureService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        public UserPetPictureService(IDataBaseContext _context, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }

        public async Task<BaseResultDto<UserPetPictureVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.UserPetPictures.Include(s => s.Picture).FirstOrDefaultAsync(s => s.Id == id);
            if (item != null)
                return new BaseResultDto<UserPetPictureVDto>(true, mapper.Map<UserPetPictureVDto>(item));
            return new BaseResultDto<UserPetPictureVDto>(false, mapper.Map<UserPetPictureVDto>(item));
        }

        public async Task<BaseResultDto<UserPetPictureVDto>> FindAsyncVDto(long id, long userId)
        {
            var item = await _context.UserPetPictures.Include(s => s.Picture).FirstOrDefaultAsync(s => s.Id == id && s.UserPet.UserId == userId);
            if (item != null)
                return new BaseResultDto<UserPetPictureVDto>(true, mapper.Map<UserPetPictureVDto>(item));
            return new BaseResultDto<UserPetPictureVDto>(false, mapper.Map<UserPetPictureVDto>(item));
        }

        public Task<bool> IsUserPetOwnedByAsync(long userPetId, long userId)
        {
            return _context.UserPets.AnyAsync(s => s.Id == userPetId && s.UserId == userId);
        }

        public UserPetPictureSearchDto Search(UserPetPictureInputDto searchDto)
        {
            var model = _context.UserPetPictures.Include(s => s.Picture).AsQueryable().Where(s => !s.Deleted);
            if (searchDto.UserPetId.HasValue)
            {
                model = model.Where(s => s.UserPetId.Equals(searchDto.UserPetId));
            }
            if (searchDto.UserId.HasValue)
            {
                model = model.Where(s => s.UserPet.UserId == searchDto.UserId.Value);
            }
            return new UserPetPictureSearchDto(searchDto, model, mapper);
        }
        public void InsertOrUpdate(UserPetPictureDto UserPetPicture)
        {
            var item = _context.UserPetPictures.FirstOrDefault(s => s.UserPetId == UserPetPicture.UserPetId && s.PictureId == UserPetPicture.PictureId);

            item = mapper.Map<UserPetPicture>(UserPetPicture);
            _context.UserPetPictures.Add(item);
            _context.SaveChanges();
        }

        public void InsertOrUpdate(UserPet UserPet, List<UserPetPictureDto> UserPetPicturesDto)
        {
            if (UserPet.UserPetPictures != null)
            {
                _context.UserPetPictures.RemoveRange(UserPet.UserPetPictures);
                _context.SaveChanges();
            }
            else
            {
                UserPet.UserPetPictures = new List<UserPetPicture>();
            }
            UserPetPicturesDto.ForEach(s => s.UserPetId = UserPet.Id);
            foreach (var item in UserPetPicturesDto)
            {
                InsertOrUpdate(item);
            }
        }
    }
}
