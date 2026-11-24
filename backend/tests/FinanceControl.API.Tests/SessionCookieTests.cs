using Microsoft.AspNetCore.Http;

namespace FinanceControl.API.Tests;

public sealed class SessionCookieTests
{
    [Theory]
    [InlineData(false, "__Host-financecontrol_session", true)]
    [InlineData(true, "financecontrol_session", false)]
    public void CookieUsesTheExpectedSecurityOptions(bool development, string name, bool secure)
    {
        var options = SessionCookie.Options(development);
        Assert.Equal(name, SessionCookie.Name(development));
        Assert.Equal(secure, options.Secure);
        Assert.True(options.HttpOnly);
        Assert.Equal(SameSiteMode.Strict, options.SameSite);
        Assert.Equal("/", options.Path);
        Assert.Null(options.Domain);
    }
}
