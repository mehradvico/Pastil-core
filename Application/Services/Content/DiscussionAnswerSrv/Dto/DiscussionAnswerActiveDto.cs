using Application.Common.Dto.Field;
using Entities.Entities.CommonField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Content.DiscussionAnswerSrv.Dto
{
    public class DiscussionAnswerActiveDto : Id_FieldDto
    {
        public bool Active { get; set; }
    }
}
