using Application.Common.Dto.Field;
using Application.Services.PastilMatchSrvs.PastilMatchProfileSrv.Dto;
using Application.Services.Setting.CodeSrv.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchRequestSrv.Dto
{
    public class PastilMatchRequestVDto : Id_FieldDto
    {
        public long SenderProfileId { get; set; }
        public long ReceiverProfileId { get; set; }
        public long PastilMatchGoalId { get; set; }
        public long StatusId { get; set; }

        public string Description { get; set; }
        public int CompatibilityPercent { get; set; }

        public DateTime? ResponseDate { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? CancelDate { get; set; }

        public PastilMatchProfileVDto SenderProfile { get; set; }
        public PastilMatchProfileVDto ReceiverProfile { get; set; }
        public CodeVDto PastilMatchGoal { get; set; }
        public CodeVDto Status { get; set; }
    }
}
