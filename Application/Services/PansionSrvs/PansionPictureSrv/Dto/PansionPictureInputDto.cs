using Application.Common.Dto.Input;
using Application.Services.PansionSrvs.PansionPictureSrv.Iface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PansionSrvs.PansionPictureSrv.Dto
{
    public class PansionPictureInputDto : BaseInputDto, IPansionPictureSearchFields
    {
        public long? PansionId { get; set; }
        public long? CompanionId { get; set; }
        // فقط پنل/اپ ارائه‌دهنده: پانسیون‌های «این کاربر»؛ سمت کنترلر از کاربر پر می‌شود
        public long? OwnerUserId { get; set; }
    }
}
