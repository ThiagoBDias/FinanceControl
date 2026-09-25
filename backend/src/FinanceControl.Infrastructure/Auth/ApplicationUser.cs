using Microsoft.AspNetCore.Identity;

namespace FinanceControl.Infrastructure.Auth;

/// <summary>
/// Credenciais e dados técnicos de autenticação. O domínio financeiro continua
/// usando seu próprio User; ambos compartilham o mesmo Guid como OwnerScopeId.
/// </summary>
public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string Name { get; set; } = string.Empty;
}
