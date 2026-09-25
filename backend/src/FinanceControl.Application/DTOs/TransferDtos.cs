using FinanceControl.Domain.Entities;

namespace FinanceControl.Application.DTOs;

public sealed record CreateTransferRequest(
    Guid OwnerScopeId,
    Guid SourceAccountId,
    Guid DestinationAccountId,
    decimal Amount,
    DateOnly TransferDate,
    string? Description);

public sealed record GetTransferRequest(Guid OwnerScopeId, Guid TransferGroupId);

public sealed record TransferResponse(
    Guid Id,
    Guid OwnerScopeId,
    Guid SourceAccountId,
    Guid DestinationAccountId,
    decimal Amount,
    DateOnly TransferDate,
    string? Description,
    DateTime CreatedAt);

internal static class TransferMapper
{
    public static TransferResponse ToResponse(TransferGroup group) => new(
        group.Id, group.OwnerScopeId, group.SourceAccountId, group.DestinationAccountId,
        group.Amount, group.TransferDate, group.Description, group.CreatedAt);
}
