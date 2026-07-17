using System.Collections.Generic;

namespace Application.Services.Setting.NoticeSrv.Dto
{
    public class NoticeBulkReadDto
    {
        public List<long> NoticeIds { get; set; } = new List<long>();
        public bool All { get; set; }
        public bool Confirmed { get; set; }
    }
}
