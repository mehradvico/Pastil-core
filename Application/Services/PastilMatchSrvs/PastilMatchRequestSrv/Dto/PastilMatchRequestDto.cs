using Application.Common.Dto.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchRequestSrv.Dto
{
    public class PastilMatchRequestDto : Id_FieldDto
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
    }
}
