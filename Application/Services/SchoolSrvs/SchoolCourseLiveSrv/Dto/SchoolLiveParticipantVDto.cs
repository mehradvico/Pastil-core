namespace Application.Services.SchoolSrvs.SchoolCourseLiveSrv.Dto
{
    public class SchoolLiveParticipantVDto
    {
        public long UserPetId { get; set; }
        public string PetName { get; set; }
        public long? PetPictureId { get; set; }
        public string PetPictureUrl { get; set; }
        public string PetTypeName { get; set; }
        public string PetBreedName { get; set; }
        public long OwnerId { get; set; }
        public string OwnerName { get; set; }
        public string OwnerMobile { get; set; }
        public bool Present { get; set; }
    }
}
