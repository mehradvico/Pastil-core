using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;
using System.Collections.Generic;

namespace Entities.Entities.ProviderField
{
    /// <summary>
    /// ارائه‌دهنده = کاربری که صاحب یک یا چند کسب‌وکار (فروشگاه، کلینیک، آرایشگاه، پانسیون و ...) است.
    /// اشتراک (نقره‌ای/طلایی/الماس) روی همین سطح تعریف می‌شود، نه روی هر نمایندگی.
    /// </summary>
    public class Provider : Id_Field
    {
        public long UserId { get; set; }
        public bool Active { get; set; }
        public bool Approved { get; set; }
        public bool Deleted { get; set; }
        public DateTime CreateDate { get; set; }
        public string Note { get; set; }

        public User User { get; set; }
        public ICollection<ProviderRoleAssignment> RoleAssignments { get; set; }
        public ICollection<ProviderSubscription> Subscriptions { get; set; }
        public ICollection<Companion> Companions { get; set; }
        public ICollection<Store> Stores { get; set; }
        public ICollection<GroomerField.Groomer> Groomers { get; set; }
        public ICollection<ClinicField.Clinic> Clinics { get; set; }
        public ICollection<CoachField.Coach> Coaches { get; set; }
    }
}
