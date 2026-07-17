namespace Application.Services.Setting.NoticeSrv.Dto
{
    public class NoticeBulkReadVDto
    {
        public int RequestedCount { get; set; }
        public int ReadCount { get; set; }
        public int AlreadyReadCount { get; set; }
        public int NotFoundCount { get; set; }
        public string AdminName { get; set; }
    }
}
