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
        public void IsComplete_ForMixedBreedWithoutSecondBreed_ReturnsTrue()
        {
            // فرم وب‌اپ نژاد دوم ندارد؛ نژاد دوم دیگر شرط کامل بودن نیست
            var userPet = CreateCompletePet();
            userPet.IsMixBreed = true;
            userPet.PetBreed2Id = null;

            Assert.True(_service.IsComplete(userPet));
        }

        [Fact]
        public void MissingFields_ListsEveryMissingField()
        {
            var userPet = new UserPet { Active = true, Birthday = DateTime.Today.AddDays(1) };

            Assert.Equal(
                new[] { PetProfileFields.Name, PetProfileFields.PetType, PetProfileFields.Picture, PetProfileFields.Breed, PetProfileFields.Birthday },
                _service.MissingFields(userPet));
            Assert.Empty(_service.MissingFields(CreateCompletePet()));
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
