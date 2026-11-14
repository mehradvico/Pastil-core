using Application.Services.PastilClubSrvs.PetProfileSrv.Iface;
using Entities.Entities;
using System;
using System.Collections.Generic;

namespace Application.Services.PastilClubSrvs.PetProfileSrv
{
    // «پروفایل کامل پت» برای امتیاز کلاب (PetProfileCompleted) و هدیه‌ی ۲۰۰ هزار تومانی اولین پت:
    // نام، نوع حیوان، عکس، نژاد اصلی و تاریخ تولدی که در آینده نیست.
    // نژاد دوم (دورگه) لازم نیست: فرم وب‌اپ فیلدی برای آن ندارد و کاربر نمی‌توانست کاملش کند (۱۴۰۵/۰۸/۲۳).
    public class ClubPetProfileCompletionService : IClubPetProfileCompletionService
    {
        public bool IsComplete(UserPet userPet) =>
            userPet != null && !userPet.Deleted && userPet.Active && MissingFields(userPet).Count == 0;

        public IReadOnlyList<string> MissingFields(UserPet userPet)
        {
            var missing = new List<string>();
            if (userPet == null)
                return missing;
            if (string.IsNullOrWhiteSpace(userPet.Name))
                missing.Add(PetProfileFields.Name);
            if (userPet.PetId <= 0)
                missing.Add(PetProfileFields.PetType);
            if (!userPet.PictureId.HasValue)
                missing.Add(PetProfileFields.Picture);
            if (!userPet.PetBreedId.HasValue)
                missing.Add(PetProfileFields.Breed);
            if (userPet.Birthday == default || userPet.Birthday.Date > DateTime.Today)
                missing.Add(PetProfileFields.Birthday);
            return missing;
        }
    }

    // کلیدهای ثابت فیلدهای ناقص که به وب‌اپ/اپ برگردانده می‌شوند (متن فارسی سمت کلاینت)
    public static class PetProfileFields
    {
        public const string Name = "name";
        public const string PetType = "petType";
        public const string Picture = "picture";
        public const string Breed = "breed";
        public const string Birthday = "birthday";
    }
}
