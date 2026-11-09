using Application.Common.Dto.Input;
using Application.Services.PansionSrvs.PansionPetSrv.Iface;
using Application.Services.PansionSrvs.PansionSrv.Iface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PansionSrvs.PansionPetSrv.Dto
{
    public class PansionPetInputDto : BaseInputDto, IPansionPetSearchFields
    {
        public long? CompanionId { get; set; }
        public long? PansionId { get; set; }
        // فقط پنل/اپ ارائه‌دهنده: پانسیون‌های «این کاربر»؛ سمت کنترلر از کاربر پر می‌شود
        public long? OwnerUserId { get; set; }
    }
}
