namespace Application.Services.Accounting.PetTagSrv.Dto
{
    public class PetTagPublicPetVDto
    {
        public string Name { get; set; }
        public string BreedName { get; set; }
        public bool IsMale { get; set; }
        public string PictureUrl { get; set; }
        public string OwnerFirstName { get; set; }
        public string OwnerLastName { get; set; }
        public string OwnerMobile { get; set; }
    }

    public class PetTagPublicStatusDto
    {
        public bool Claimed { get; set; }
        public PetTagPublicPetVDto Pet { get; set; }
    }
}
