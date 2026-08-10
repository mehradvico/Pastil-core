using Entities.Entities;

namespace Application.Services.PastilClubSrvs.PetProfileSrv.Iface
{
    public interface IClubPetProfileCompletionService
    {
        bool IsComplete(UserPet userPet);
    }
}
