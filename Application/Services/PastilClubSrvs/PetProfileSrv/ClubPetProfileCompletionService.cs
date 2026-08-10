using Application.Services.PastilClubSrvs.PetProfileSrv.Iface;
using Entities.Entities;
using System;

namespace Application.Services.PastilClubSrvs.PetProfileSrv
{
    public class ClubPetProfileCompletionService : IClubPetProfileCompletionService
    {
        public bool IsComplete(UserPet userPet)
        {
            if (userPet == null || userPet.Deleted || !userPet.Active)
                return false;

            if (string.IsNullOrWhiteSpace(userPet.Name) ||
                userPet.PetId <= 0 ||
                !userPet.PictureId.HasValue ||
                !userPet.PetBreedId.HasValue ||
                userPet.Birthday == default ||
                userPet.Birthday.Date > DateTime.Today)
            {
                return false;
            }

            return !userPet.IsMixBreed ||
                   userPet.PetBreed2Id.HasValue &&
                   userPet.PetBreed2Id != userPet.PetBreedId;
        }
    }
}
