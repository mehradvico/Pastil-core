namespace Entities.Entities.PetResanServiceField
{
    // پت‌های یک سرویس هفتگی پت‌رسان (چند پت مجاز)؛ دقیقاً هم‌الگوی TripPet برای سفرهای تکی.
    public class PetResanServicePet : CommonField.Id_Field
    {
        public long PetResanServiceId { get; set; }
        public long UserPetId { get; set; }

        public PetResanService PetResanService { get; set; }
        public UserPet UserPet { get; set; }
    }
}
