using Application.Services.Accounting.UserPetSrv.Dto;

namespace Application.Services.Accounting.UserPetSrv
{
    /// <summary>
    /// نمای «غیرمالک» از یک پت. صاحب پت (و ادمین) همه‌چیز را می‌بیند؛ هر کاربر لاگین‌کرده‌ی دیگر فقط مشخصات نمایشی را می‌بیند
    /// و اطلاعات پزشکی/شناسایی/تماس (بیماری، دارو، میکروچیپ، آدرس، مشخصات و موبایل/ایمیل صاحب، سوابق) حذف می‌شود.
    /// </summary>
    public static class UserPetPrivacy
    {
        public static UserPetVDto ToPublicView(UserPetVDto pet)
        {
            if (pet == null)
                return null;

            pet.UserId = 0;
            pet.User = null;
            pet.MicroChipCode = null;
            pet.SpecificDisease = null;
            pet.SpecificMedicene = null;
            pet.AddressValue = null;
            pet.UserPetRecords = null;
            return pet;
        }
    }
}
