using Application.Services.CommonSrv.CommentSrv.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PansionSrvs.PansionCommentSrv.Dto
{
    public class PansionCommentVDto : CommentVDto
    {
        public long PansionId { get; set; }
        public long? PansionReserveId { get; set; }
        public bool IsReserved { get; set; }
        public string PansionName { get; set; }
    }
}
