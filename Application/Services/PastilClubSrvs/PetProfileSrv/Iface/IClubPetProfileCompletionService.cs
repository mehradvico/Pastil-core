using Entities.Entities;
using System.Collections.Generic;

namespace Application.Services.PastilClubSrvs.PetProfileSrv.Iface
{
    public interface IClubPetProfileCompletionService
    {
        bool IsComplete(UserPet userPet);

        /// <summary>کلیدهای PetProfileFields فیلدهای ناقص (لیست خالی یعنی کامل).</summary>
        IReadOnlyList<string> MissingFields(UserPet userPet);
    }
}
