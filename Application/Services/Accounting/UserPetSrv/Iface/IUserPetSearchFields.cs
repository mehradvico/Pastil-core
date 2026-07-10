namespace Application.Services.Accounting.UserPetSrv.Iface
{
    public interface IUserPetSearchFields
    {
        public long? UserId { get; set; }
        public long? PetBreedId { get; set; }
        public bool? IsSterile { get; set; }
    }
}
