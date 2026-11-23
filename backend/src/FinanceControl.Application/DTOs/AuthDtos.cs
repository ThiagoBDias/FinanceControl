namespace FinanceControl.Application.DTOs;

public sealed record RegisterRequest(string Name, string Email, string Password, string? InviteCode = null);

public sealed record LoginRequest(string Email, string Password);

public sealed record AuthResponse(
    Guid UserId,
    string Name,
    string Email,
    string AccessToken,
    DateTime ExpiresAtUtc);
