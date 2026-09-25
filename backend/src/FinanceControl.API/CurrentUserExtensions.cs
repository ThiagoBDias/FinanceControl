using System.Security.Claims;

namespace FinanceControl.API;

public static class CurrentUserExtensions
{
    public static Guid GetCurrentUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) && id != Guid.Empty
            ? id
            : throw new UnauthorizedAccessException("Usuário autenticado não possui um identificador válido.");
    }
}
