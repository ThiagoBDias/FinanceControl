using System.Security.Cryptography;
using System.Text;
using FinanceControl.Application.DTOs;
using FinanceControl.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FinanceControl.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService auth, IConfiguration configuration, IWebHostEnvironment environment) : ControllerBase
{
    [AllowAnonymous]
    [EnableRateLimiting("Auth")]
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        if (!CanRegister(request))
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails { Detail = "Cadastro restrito por convite." });

        var response = await auth.RegisterAsync(request, cancellationToken);
        if (IsBrowser)
            return StatusCode(StatusCodes.Status201Created);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [AllowAnonymous]
    [EnableRateLimiting("Auth")]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await auth.LoginAsync(request, cancellationToken);
        if (!IsBrowser)
            return Ok(response);

        var development = environment.IsDevelopment();
        Response.Cookies.Append(SessionCookie.Name(development), response.AccessToken,
            SessionCookie.Options(development, response.ExpiresAtUtc));
        return NoContent();
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        var development = environment.IsDevelopment();
        Response.Cookies.Delete(SessionCookie.Name(development), SessionCookie.Options(development));
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public ActionResult<object> Me() => Ok(new { UserId = User.GetCurrentUserId(), Name = User.Identity?.Name });

    private bool IsBrowser => Request.Headers[SessionCookie.BrowserHeader] == "browser";

    private bool CanRegister(RegisterRequest request)
    {
        if (environment.IsDevelopment() && configuration.GetValue<bool>("Auth:AllowLocalRegistration"))
            return true;

        var email = configuration["Auth:InviteEmail"];
        var code = configuration["Auth:InviteCode"];
        return !string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(code) && code.Length >= 32 &&
            string.Equals(email.Trim(), request.Email.Trim(), StringComparison.OrdinalIgnoreCase) &&
            request.InviteCode is not null && CryptographicOperations.FixedTimeEquals(
                SHA256.HashData(Encoding.UTF8.GetBytes(code)), SHA256.HashData(Encoding.UTF8.GetBytes(request.InviteCode)));
    }
}
