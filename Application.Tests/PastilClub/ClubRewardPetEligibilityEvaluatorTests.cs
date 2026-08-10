using Application.Services.PastilClubSrvs.RewardOfferSrv;
using Xunit;

namespace Application.Tests.PastilClub
{
    public class ClubRewardPetEligibilityEvaluatorTests
    {
        [Fact]
        public void GeneralReward_IsVisibleToUserWithoutPet()
        {
            Assert.True(ClubRewardPetEligibilityEvaluator.IsEligible([], []));
        }

        [Fact]
        public void DogReward_IsHiddenFromCatOnlyUser()
        {
            Assert.False(ClubRewardPetEligibilityEvaluator.IsEligible([2], [1]));
        }

        [Fact]
        public void DogReward_IsVisibleToDogUser()
        {
            Assert.True(ClubRewardPetEligibilityEvaluator.IsEligible([1], [1]));
        }

        [Fact]
        public void DogReward_IsVisibleToDogAndCatUser()
        {
            Assert.True(ClubRewardPetEligibilityEvaluator.IsEligible([1, 2], [1]));
        }
    }
}
