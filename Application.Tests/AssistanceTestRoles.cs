using Entities.Entities.ProviderField;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Tests
{
    /// <summary>خدمت‌های تستی باید مالکِ نقش باشند (Assistance.ProviderRoleId)؛ نقش را در صورت نبودن می‌سازد.</summary>
    public static class AssistanceTestRoles
    {
        public static async Task<long> IdAsync(DataBaseContext context, string label)
        {
            var existing = await context.ProviderRoles.Where(r => r.Label == label && !r.Deleted).Select(r => (long?)r.Id).FirstOrDefaultAsync();
            if (existing.HasValue) return existing.Value;
            var role = new ProviderRole { Name = label, Label = label, Active = true };
            context.ProviderRoles.Add(role);
            await context.SaveChangesAsync();
            return role.Id;
        }
    }
}
