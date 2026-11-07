using Application.Common.Dto.Input;
using Application.Services.Content.PostCommentSrv.Iface;
using Application.Services.PansionSrvs.PansionCommentSrv.Iface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PansionSrvs.PansionCommentSrv.Dto
{
    public class PansionCommentInputDto : BaseInputDto, IPansionCommentSearchFields
    {
        public long? PansionId { get; set; }
        public bool? IsReserved { get; set; }
        public long? UserId { get; set; }
        /// <summary>نظرهای پانسیون‌های یک نمایندگی (برای مالک)</summary>
        public long? CompanionId { get; set; }
        // فقط پنل/اپ ارائه‌دهنده: پانسیون‌های «این کاربر»؛ سمت کنترلر از کاربر پر می‌شود
        public long? OwnerUserId { get; set; }
        public bool? AllStatus { get; set; }

    }
}
