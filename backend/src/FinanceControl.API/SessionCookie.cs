namespace FinanceControl.API;

public static class SessionCookie
{
    public const string BrowserHeader = "X-FinanceControl-Client";
    public static string Name(bool development) => development ? "financecontrol_session" : "__Host-financecontrol_session";
    public static CookieOptions Options(bool development, DateTimeOffset? expiration = null) => new()
    {
        Secure = !development,
        HttpOnly = true,
        SameSite = SameSiteMode.Strict,
        Path = "/",
        Expires = expiration
    };
}
