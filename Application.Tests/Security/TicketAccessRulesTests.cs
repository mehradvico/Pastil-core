using Application.Services.Accounting.TicketSrv;
using Xunit;

namespace Application.Tests.Security;

public class TicketAccessRulesTests
{
    [Theory]
    [InlineData(null)]
    [InlineData(7L)]
    [InlineData(99L)]
    public void Super_admin_sees_every_ticket_assigned_or_not(long? ticketAdminId)
    {
        Assert.True(TicketAccessRules.CanAdminAccess(true, ticketAdminId, 7));
    }

    [Fact]
    public void Other_admin_sees_only_the_ticket_assigned_to_them()
    {
        Assert.True(TicketAccessRules.CanAdminAccess(false, 7, 7));
        Assert.False(TicketAccessRules.CanAdminAccess(false, 99, 7));
    }

    [Fact]
    public void Other_admin_never_sees_an_unassigned_ticket()
    {
        Assert.False(TicketAccessRules.CanAdminAccess(false, null, 7));
    }

    [Fact]
    public void Unknown_current_user_sees_nothing_unless_super_admin()
    {
        Assert.False(TicketAccessRules.CanAdminAccess(false, 7, null));
        Assert.False(TicketAccessRules.CanAdminAccess(false, null, null));
    }
}
