using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Services.PastilMatchSrvs.PastilMatchProfileLikeSrv.Dto;
using Application.Services.PastilMatchSrvs.PastilMatchProfileLikeSrv.Iface;
using AutoMapper;
using Entities.Entities.PastilMatchField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchProfileLikeSrv
{
    public class PastilMatchProfileLikeService : CommonSrv<PastilMatchProfileLike, PastilMatchProfileLikeDto>, IPastilMatchProfileLikeService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ICurrentUserHelper _currentUser;

        public PastilMatchProfileLikeService( IDataBaseContext context, IMapper mapper, ICurrentUserHelper currentUser) : base(context, mapper)
        {
            _context = context;
            this.mapper = mapper;
            _currentUser = currentUser;
        }

        public override async Task<BaseResultDto<PastilMatchProfileLikeDto>>
            InsertAsyncDto(PastilMatchProfileLikeDto dto)
        {
            try
            {
                var modelChecker = ModelHelper<PastilMatchProfileLikeDto>.ModelErrors(dto);

                if (!modelChecker.IsSuccess)
                {
                    return modelChecker;
                }

                var userId = _currentUser.CurrentUser.UserId;

                if (dto.LikerProfileId == dto.LikedProfileId)
                {
                    return new BaseResultDto<PastilMatchProfileLikeDto>(false, Resource.Notification.PastilMatchProfileCannotLikeItself, dto);
                }

                var likerProfile = await _context.PastilMatchProfiles.Include(s => s.UserPet).FirstOrDefaultAsync(s => s.Id == dto.LikerProfileId && !s.Deleted);

                if (likerProfile == null)
                {
                    return new BaseResultDto<PastilMatchProfileLikeDto>(false, Resource.Notification.NothingFound,dto);
                }

                if (likerProfile.UserPet.UserId != userId)
                {
                    return new BaseResultDto<PastilMatchProfileLikeDto>(false, Resource.Notification.AccessDenied,dto);
                }

                var likedProfile = await _context.PastilMatchProfiles.FirstOrDefaultAsync(s => s.Id == dto.LikedProfileId && !s.Deleted && s.IsActive);

                if (likedProfile == null)
                {
                    return new BaseResultDto<PastilMatchProfileLikeDto>(false, Resource.Notification.NothingFound, dto);
                }

                var profileLike = await _context.PastilMatchProfileLikes.FirstOrDefaultAsync(s => s.LikerProfileId == dto.LikerProfileId && s.LikedProfileId == dto.LikedProfileId);

                if (profileLike != null && !profileLike.Deleted)
                {
                    return new BaseResultDto<PastilMatchProfileLikeDto>(true, mapper.Map<PastilMatchProfileLikeDto>(profileLike));
                }

                if (profileLike != null)
                {
                    profileLike.Deleted = false;
                    profileLike.CreateDate = DateTime.Now;

                    _context.PastilMatchProfileLikes.Update(profileLike);
                }
                else
                {
                    profileLike = mapper.Map<PastilMatchProfileLike>(dto);

                    profileLike.Deleted = false;
                    profileLike.CreateDate = DateTime.Now;

                    await _context.PastilMatchProfileLikes.AddAsync(profileLike);
                }

                likedProfile.LikeCount++;

                _context.PastilMatchProfiles.Update(likedProfile);

                await _context.SaveChangesAsync();

                return new BaseResultDto<PastilMatchProfileLikeDto>(true, mapper.Map<PastilMatchProfileLikeDto>(profileLike));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<PastilMatchProfileLikeDto>(false, ex.Message, dto);
            }
        }

        public override BaseResultDto DeleteDto(long id)
        {
            try
            {
                var userId = _currentUser.CurrentUser.UserId;

                var profileLike = _context.PastilMatchProfileLikes.Include(s => s.LikerProfile).ThenInclude(s => s.UserPet).Include(s => s.LikedProfile).FirstOrDefault(s => s.Id == id && !s.Deleted);

                if (profileLike == null)
                {
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                }

                if (profileLike.LikerProfile.UserPet.UserId != userId)
                {
                    return new BaseResultDto(false, Resource.Notification.AccessDenied);
                }

                profileLike.Deleted = true;

                if (profileLike.LikedProfile.LikeCount > 0)
                {
                    profileLike.LikedProfile.LikeCount--;
                }

                _context.PastilMatchProfileLikes.Update(profileLike);
                _context.PastilMatchProfiles.Update(profileLike.LikedProfile);

                _context.SaveChanges();

                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, ex.Message);
            }
        }

        public override BaseResultDto DeleteDto(PastilMatchProfileLikeDto dto)
        {
            return DeleteDto(dto.Id);
        }
    }
}
