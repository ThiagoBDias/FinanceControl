using FinanceControl.Domain.Exceptions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FinanceControl.Application.DTOs;
using FinanceControl.Application.Interfaces;
using FinanceControl.Application.Validators;
using FinanceControl.Domain.Entities;
using FinanceControl.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FinanceControl.Infrastructure.Auth;

public sealed class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly FinanceControlDbContext _db;
    private readonly JwtOptions _options;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        FinanceControlDbContext db,
        IOptions<JwtOptions> options)
    {
        _userManager = userManager;
        _db = db;
        _options = options.Value;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        ValidateRequest(request.Name, request.Email, request.Password);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        if (await _userManager.FindByEmailAsync(normalizedEmail) is not null)
            throw new OperationRejectedException("Já existe um usuário com este e-mail.");

        var id = Guid.NewGuid();
        var identityUser = new ApplicationUser
        {
            Id = id,
            UserName = normalizedEmail,
            Email = normalizedEmail,
            Name = request.Name.Trim(),
            EmailConfirmed = true
        };

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        var result = await _userManager.CreateAsync(identityUser, request.Password);
        if (!result.Succeeded)
            throw new OperationRejectedException(string.Join(" ", result.Errors.Select(e => e.Description)));

        var domainUser = User.Create(id, request.Name, normalizedEmail);
        _db.DomainUsers.Add(domainUser);
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return CreateResponse(identityUser);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _userManager.FindByEmailAsync(normalizedEmail);
        if (user is null || await _userManager.IsLockedOutAsync(user))
            throw new UnauthorizedAccessException("E-mail ou senha inválidos.");

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
        {
            await _userManager.AccessFailedAsync(user);
            throw new UnauthorizedAccessException("E-mail ou senha inválidos.");
        }

        if (await _userManager.GetAccessFailedCountAsync(user) > 0)
            await _userManager.ResetAccessFailedCountAsync(user);

        return CreateResponse(user);
    }

    private AuthResponse CreateResponse(ApplicationUser user)
    {
        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(_options.ExpirationMinutes);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: credentials);

        return new AuthResponse(
            user.Id,
            user.Name,
            user.Email ?? string.Empty,
            new JwtSecurityTokenHandler().WriteToken(token),
            expires);
    }

    private static void ValidateRequest(string name, string email, string password)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 100)
            throw new RequestValidationException(new[]
            {
                new ValidationError(nameof(name), "Nome é obrigatório.")
            });
        if (string.IsNullOrWhiteSpace(email) || !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email.Trim()) || email.Trim().Length > 254)
            throw new RequestValidationException(new[]
            {
                new ValidationError(nameof(email), "E-mail inválido.")
            });
        if (string.IsNullOrWhiteSpace(password) || password.Length is < 12 or > 128)
            throw new RequestValidationException(new[]
            {
                new ValidationError(nameof(password), "A senha deve possuir entre 12 e 128 caracteres.")
            });
    }
}
