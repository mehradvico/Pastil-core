using Application.Common.Dto.Input;
using Application.Services.Accounting.ClubRewardSrv.Iface;
using Application.Services.Accounting.DriverSrv.Iface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Accounting.ClubRewardSrv.Dto
{
    public class ClubRewardInputDto : BaseInputDto, IClubRewardSearchFields
    {
        public long? RebateTypeId { get; set; }
    }
}
