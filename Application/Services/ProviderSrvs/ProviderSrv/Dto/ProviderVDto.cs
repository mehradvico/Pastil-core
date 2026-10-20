using Application.Common.Dto.Field;
using Application.Services.Dto;
using Application.Services.ProviderSrvs.ProviderRoleAssignmentSrv.Dto;
using Application.Services.ProviderSrvs.ProviderSubscriptionSrv.Dto;
using System;
using System.Collections.Generic;

namespace Application.Services.ProviderSrvs.ProviderSrv.Dto
{
    public class ProviderVDto : Id_FieldDto
    {
        public long UserId { get; set; }
        public bool Active { get; set; }
        public bool Approved { get; set; }
        public DateTime CreateDate { get; set; }
        public string Note { get; set; }
        public int CompanionCount { get; set; }
        public int StoreCount { get; set; }

        public UserMinVDto User { get; set; }
        public List<ProviderRoleAssignmentVDto> RoleAssignments { get; set; }
        /// <summary>اشتراک جاری (بالاترین سطح در بین دوره‌های فعال)؛ null = بدون اشتراک.</summary>
        public ProviderSubscriptionVDto CurrentSubscription { get; set; }
    }
}
