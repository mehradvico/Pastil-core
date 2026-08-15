using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.PastilMatchSrvs.PastilMatchSrv.Dto;
using Application.Services.PastilMatchSrvs.PastilMatchSrv.Iface;
using AutoMapper;
using Entities.Entities.PastilMatchField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchSrv
{
    public class PastilMatchService : CommonSrv<PastilMatch, PastilMatchDto>, IPastilMatchService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ICurrentUserHelper _currentUser;
        private readonly IPushNotificationService _pushNotificationService;

        public PastilMatchService(
            IDataBaseContext context,
            IMapper mapper,
            ICurrentUserHelper currentUser,
            IPushNotificationService pushNotificationService) : base(context, mapper)
        {
            _context = context;
            this.mapper = mapper;
            _currentUser = currentUser;
            _pushNotificationService = pushNotificationService;
        }

        public async Task<BaseResultDto<PastilMatchVDto>> FindAsyncVDto(long id)
        {
            try
            {
                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin = _currentUser.CurrentUser.RoleEnum == RoleEnum.Admin.ToString();

                var item = await GetPastilMatchQuery().FirstOrDefaultAsync(s => s.Id == id);

                if (item == null)
                {
                    return new BaseResultDto<PastilMatchVDto>(false, Resource.Notification.NothingFound, null);
                }

                if (!isAdmin && item.FirstProfile.UserPet.UserId != userId && item.SecondProfile.UserPet.UserId != userId)
                {
                    return new BaseResultDto<PastilMatchVDto>(false, Resource.Notification.AccessDenied, null);
                }

                return new BaseResultDto<PastilMatchVDto>(true, mapper.Map<PastilMatchVDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<PastilMatchVDto>(false, ex.Message, null);
            }
        }

        public PastilMatchSearchDto Search(PastilMatchInputDto dto)
        {
            var userId = _currentUser.CurrentUser.UserId;
            var isAdmin = _currentUser.CurrentUser.RoleEnum == RoleEnum.Admin.ToString();

            var model = GetPastilMatchQuery();

            if (!isAdmin)
            {
                model = model.Where(s => s.FirstProfile.UserPet.UserId == userId || s.SecondProfile.UserPet.UserId == userId);
            }

            if (dto.PastilMatchRequestId.HasValue)
            {
                model = model.Where(s => s.PastilMatchRequestId == dto.PastilMatchRequestId.Value);
            }

            if (dto.PastilMatchProfileId.HasValue)
            {
                model = model.Where(s => s.FirstProfileId == dto.PastilMatchProfileId.Value || s.SecondProfileId == dto.PastilMatchProfileId.Value);
            }

            if (dto.PastilMatchGoalId.HasValue)
            {
                model = model.Where(s => s.PastilMatchGoalId == dto.PastilMatchGoalId.Value);
            }

            if (dto.StatusId.HasValue)
            {
                model = model.Where(s => s.StatusId == dto.StatusId.Value);
            }

            switch (dto.SortBy)
            {
                case SortEnum.Old:
                    {
                        model = model.OrderBy(s => s.Id);
                        break;
                    }
                default:
                    {
                        model = model.OrderByDescending(s => s.Id);
                        break;
                    }
            }

            return new PastilMatchSearchDto(dto, model, mapper);
        }

        public override BaseResultDto DeleteDto(long id)
        {
            try
            {
                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin = _currentUser.CurrentUser.RoleEnum == RoleEnum.Admin.ToString();

                var item = _context.PastilMatches.Include(s => s.FirstProfile).ThenInclude(s => s.UserPet).Include(s => s.SecondProfile).ThenInclude(s => s.UserPet).FirstOrDefault(s => s.Id == id);

                if (item == null)
                {
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                }

                if (!isAdmin && item.FirstProfile.UserPet.UserId != userId && item.SecondProfile.UserPet.UserId != userId)
                {
                    return new BaseResultDto(false, Resource.Notification.AccessDenied);
                }

                if (item.StatusId != (long)PastilMatchStatusEnum.PastilMatchStatus_Active)
                {
                    return new BaseResultDto(false, Resource.Notification.PastilMatchNotActive);
                }

                item.StatusId = (long)PastilMatchStatusEnum.PastilMatchStatus_Closed;
                item.CloseDate = DateTime.Now;

                _context.PastilMatches.Update(item);
                _context.SaveChanges();

                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, ex.Message);
            }
        }

        public override BaseResultDto DeleteDto(PastilMatchDto dto)
        {
            return DeleteDto(dto.Id);
        }

        public async Task<BaseResultDto> DeleteAsyncDto(long id)
        {
            var userId = _currentUser.CurrentUser.UserId;
            var item = await _context.PastilMatches
                .Include(s => s.FirstProfile).ThenInclude(s => s.UserPet).ThenInclude(s => s.User)
                .Include(s => s.SecondProfile).ThenInclude(s => s.UserPet).ThenInclude(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (item == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);

            if (item.FirstProfile.UserPet.UserId != userId && item.SecondProfile.UserPet.UserId != userId)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);

            if (item.StatusId != (long)PastilMatchStatusEnum.PastilMatchStatus_Active)
                return new BaseResultDto(false, Resource.Notification.PastilMatchNotActive);

            var actorProfile = item.FirstProfile.UserPet.UserId == userId
                ? item.FirstProfile
                : item.SecondProfile;
            var receiverProfile = actorProfile.Id == item.FirstProfileId
                ? item.SecondProfile
                : item.FirstProfile;

            item.StatusId = (long)PastilMatchStatusEnum.PastilMatchStatus_Closed;
            item.CloseDate = DateTime.Now;
            await _context.SaveChangesAsync();

            await _pushNotificationService.SendPushAsync(
                PushTypeEnum.PushPastilMatchClosed,
                receiverProfile.UserPet.UserId,
                actorProfile.UserPet.User.FirstName,
                actorProfile.UserPet.Name);

            return new BaseResultDto(true);
        }

        private IQueryable<PastilMatch> GetPastilMatchQuery()
        {
            return _context.PastilMatches
                .Include(s => s.PastilMatchRequest)
                .Include(s => s.FirstProfile).ThenInclude(s => s.UserPet).ThenInclude(s => s.User)
                .Include(s => s.FirstProfile).ThenInclude(s => s.UserPet).ThenInclude(s => s.Picture)
                .Include(s => s.FirstProfile).ThenInclude(s => s.UserPet).ThenInclude(s => s.Pet)
                .Include(s => s.FirstProfile).ThenInclude(s => s.UserPet).ThenInclude(s => s.PetBreed)
                .Include(s => s.FirstProfile).ThenInclude(s => s.UserPet).ThenInclude(s => s.PetBreed2)
                .Include(s => s.FirstProfile).ThenInclude(s => s.EnergyLevel)
                .Include(s => s.FirstProfile).ThenInclude(s => s.SocialLevel)
                .Include(s => s.FirstProfile).ThenInclude(s => s.City)
                .Include(s => s.FirstProfile).ThenInclude(s => s.Neighborhood)
                .Include(s => s.FirstProfile).ThenInclude(s => s.PastilMatchProfileGoals).ThenInclude(s => s.PastilMatchGoal)
                .Include(s => s.SecondProfile).ThenInclude(s => s.UserPet).ThenInclude(s => s.User)
                .Include(s => s.SecondProfile).ThenInclude(s => s.UserPet).ThenInclude(s => s.Picture)
                .Include(s => s.SecondProfile).ThenInclude(s => s.UserPet).ThenInclude(s => s.Pet)
                .Include(s => s.SecondProfile).ThenInclude(s => s.UserPet).ThenInclude(s => s.PetBreed)
                .Include(s => s.SecondProfile).ThenInclude(s => s.UserPet).ThenInclude(s => s.PetBreed2)
                .Include(s => s.SecondProfile).ThenInclude(s => s.EnergyLevel)
                .Include(s => s.SecondProfile).ThenInclude(s => s.SocialLevel)
                .Include(s => s.SecondProfile).ThenInclude(s => s.City)
                .Include(s => s.SecondProfile).ThenInclude(s => s.Neighborhood)
                .Include(s => s.SecondProfile).ThenInclude(s => s.PastilMatchProfileGoals).ThenInclude(s => s.PastilMatchGoal)
                .Include(s => s.PastilMatchGoal)
                .Include(s => s.Status)
                .AsQueryable();
        }
    }
}
