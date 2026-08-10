using Application.Services.PastilClubSrvs.RewardOfferSrv.Iface;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.PastilClubSrvs.RewardOfferSrv
{
    public class ClubRewardEligibilityService : IClubRewardEligibilityService
    {
        private readonly IDataBaseContext _context;

        public ClubRewardEligibilityService(IDataBaseContext context)
        {
            _context = context;
        }

        public async Task<bool> IsPetEligibleAsync(
            long userId,
            long rewardTemplateId,
            CancellationToken cancellationToken = default)
        {
            var petTypeIds = await _context.ClubRewardPetTypes.AsNoTracking()
                .Where(item => item.RewardTemplateId == rewardTemplateId)
                .Select(item => item.PetTypeId)
                .ToListAsync(cancellationToken);

            var userPetTypeIds = await _context.UserPets.AsNoTracking()
                .Where(item => item.UserId == userId && item.Active && !item.Deleted)
                .Select(item => item.PetId)
                .ToListAsync(cancellationToken);
            return ClubRewardPetEligibilityEvaluator.IsEligible(userPetTypeIds, petTypeIds);
        }
    }
}
