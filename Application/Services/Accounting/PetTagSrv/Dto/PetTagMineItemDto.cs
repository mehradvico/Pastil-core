namespace Application.Services.Accounting.PetTagSrv.Dto
{
    public class PetTagMineItemDto
    {
        public long UserPetId { get; set; }
        public string Code { get; set; }
        public System.DateTime? ClaimedDate { get; set; }
    }
}
