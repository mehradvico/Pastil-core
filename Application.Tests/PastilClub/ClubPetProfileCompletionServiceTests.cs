using Application.Services.PastilClubSrvs.PetProfileSrv;
using Entities.Entities;
using System;
using Xunit;

namespace Application.Tests.PastilClub
{
    public class ClubPetProfileCompletionServiceTests
    {
        private readonly ClubPetProfileCompletionService _service = new();

        [Fact]
        public void IsComplete_WithAllRequiredFields_ReturnsTrue()
        {
            var userPet = CreateCompletePet();

            Assert.True(_service.IsComplete(userPet));
        }

        [Fact]
        public void IsComplete_WithoutPicture_ReturnsFalse()
        {
            var userPet = CreateCompletePet();
            userPet.PictureId = null;

            Assert.False(_service.IsComplete(userPet));
        }

        [Fact]
        public void IsComplete_ForMixedBreedWithoutDistinctSecondBreed_ReturnsFalse()
        {
            var userPet = CreateCompletePet();
            userPet.IsMixBreed = true;
            userPet.PetBreed2Id = userPet.PetBreedId;

            Assert.False(_service.IsComplete(userPet));
        }

        [Fact]
        public void IsComplete_ForMixedBreedWithDistinctSecondBreed_ReturnsTrue()
        {
            var userPet = CreateCompletePet();
            userPet.IsMixBreed = true;
            userPet.PetBreed2Id = 12;

            Assert.True(_service.IsComplete(userPet));
        }

        private static UserPet CreateCompletePet()
        {
            return new UserPet
            {
                Name = "Pastil",
                PetId = 1,
                PictureId = 10,
                PetBreedId = 11,
                Birthday = DateTime.Today.AddYears(-1),
                Active = true,
                Deleted = false
            };
        }
    }
}
