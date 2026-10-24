using Application.Services.CompanionSrvs.AssistanceSrv;
using Entities.Entities;
using Entities.Entities.ProviderField;
using Xunit;

namespace Application.Tests.AssistanceRoles
{
    public class AssistanceRoleRulesTests
    {
        private static Entities.Entities.Assistance Make(string role, bool grooming = false) => new()
        {
            IsGrooming = grooming,
            ProviderRole = role == null ? null : new ProviderRole { Label = role },
        };

        [Theory]
        [InlineData(ProviderRoleLabels.Clinic, ProviderRoleLabels.Clinic, true)]
        [InlineData(ProviderRoleLabels.Coach, ProviderRoleLabels.Clinic, false)]
        [InlineData(ProviderRoleLabels.Clinic, ProviderRoleLabels.Coach, false)]
        [InlineData(ProviderRoleLabels.Coach, ProviderRoleLabels.Coach, true)]
        [InlineData(null, ProviderRoleLabels.Clinic, false)]
        [InlineData(null, ProviderRoleLabels.Coach, false)]
        [InlineData(ProviderRoleLabels.Pansion, ProviderRoleLabels.Clinic, false)]
        public void Venue_sees_only_its_own_role(string serviceRole, string venueRole, bool allowed) =>
            Assert.Equal(allowed, AssistanceRoleRules.IsAllowedForRole(Make(serviceRole), venueRole));

        [Fact]
        public void Groomer_gets_grooming_only() 
        {
            Assert.True(AssistanceRoleRules.IsAllowedForRole(Make(null, grooming: true), ProviderRoleLabels.Groomer));
            Assert.True(AssistanceRoleRules.IsAllowedForRole(Make(ProviderRoleLabels.Groomer), ProviderRoleLabels.Groomer));
            Assert.False(AssistanceRoleRules.IsAllowedForRole(Make(ProviderRoleLabels.Clinic), ProviderRoleLabels.Groomer));
            Assert.False(AssistanceRoleRules.IsAllowedForRole(Make(ProviderRoleLabels.Groomer, grooming: false), ProviderRoleLabels.Clinic));
        }

        [Fact]
        public void Legacy_companion_excludes_groomer_and_coach()
        {
            Assert.True(AssistanceRoleRules.IsAllowedForRole(Make(null), AssistanceRoleRules.LegacyCompanion));
            Assert.True(AssistanceRoleRules.IsAllowedForRole(Make(ProviderRoleLabels.Clinic), AssistanceRoleRules.LegacyCompanion));
            Assert.False(AssistanceRoleRules.IsAllowedForRole(Make(ProviderRoleLabels.Coach), AssistanceRoleRules.LegacyCompanion));
            Assert.False(AssistanceRoleRules.IsAllowedForRole(Make(null, grooming: true), AssistanceRoleRules.LegacyCompanion));
        }
    }
}
