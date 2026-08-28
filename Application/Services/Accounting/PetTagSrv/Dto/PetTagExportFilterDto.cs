namespace Application.Services.Accounting.PetTagSrv.Dto
{
    public class PetTagExportFilterDto
    {
        public string Q { get; set; }
        public bool? Claimed { get; set; }
        public bool? Active { get; set; }
    }
}
