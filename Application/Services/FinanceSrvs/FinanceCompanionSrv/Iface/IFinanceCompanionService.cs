using Application.Common.Dto.Result;
using Application.Services.FinanceSrvs.FinanceCompanionSrv.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.FinanceCompanionSrv.Iface
{
    public interface IFinanceCompanionService
    {
        Task<BaseResultDto<FinanceCompanionVDto>> SearchAsync(FinanceCompanionInputDto dto);

        /// <summary>گزارش مالی پانسیون بر اساس خود پانسیون، نه نمایندگی. ownerUserId خالی = ادمین (PansionId الزامی)؛ پر = فقط پانسیون‌های همان کاربر.</summary>
        Task<BaseResultDto<FinancePansionVDto>> SearchPansionAsync(FinancePansionInputDto dto, long? ownerUserId);

    }
}
