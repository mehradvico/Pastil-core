using Application.Common.Dto.Field;
using Application.Services.Filing.PictureSrv.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PansionSrvs.PansionSrv.Dto
{
    public class PansionFinanceVDto : Name_FieldDto
    {
        public bool? IsSchool { get; set; }
        public long CompanionId { get; set; }

        public decimal DailyCommissionPercent { get; set; }
        public decimal HourlyCommissionPercent { get; set; }
        public bool HasCommission { get; set; }

        public PictureVDto Picture { get; set; }

    }
}
