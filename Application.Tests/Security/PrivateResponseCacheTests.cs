using Application.Common.Security;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Application.Tests.Security;

public class PrivateResponseCacheTests
{
    [Fact]
    public void AuthenticatedResponse_IsNeverStoredBySharedCache()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers.Authorization = "Bearer abc";
        SecurityHeadersExtensions.ApplyHeaders(context);
        Assert.Equal("private, no-store", context.Response.Headers.CacheControl.ToString());
        Assert.Contains("Authorization", context.Response.Headers.Vary.ToString());
    }

    [Fact]
    public void AnonymousResponse_KeepsCdnPolicy()
    {
        var context = new DefaultHttpContext();
        SecurityHeadersExtensions.ApplyHeaders(context);
        Assert.True(string.IsNullOrEmpty(context.Response.Headers.CacheControl.ToString()));
    }
}
