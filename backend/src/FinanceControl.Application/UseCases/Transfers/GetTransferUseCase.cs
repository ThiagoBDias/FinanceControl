using FinanceControl.Application.DTOs;
using FinanceControl.Application.Interfaces;
using FinanceControl.Application.Validators;
using FinanceControl.Domain.Entities;

namespace FinanceControl.Application.UseCases.Transfers;

public sealed class GetTransferUseCase : IUseCase<GetTransferRequest, TransferResponse>
{
    private readonly ITransferRepository _transferRepository;

    public GetTransferUseCase(ITransferRepository transferRepository) => _transferRepository = transferRepository;

    public async Task<TransferResponse> ExecuteAsync(GetTransferRequest request, CancellationToken cancellationToken = default)
    {
        var transferGroup = await _transferRepository.GetByIdAsync(request.TransferGroupId, request.OwnerScopeId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(TransferGroup), request.TransferGroupId);

        return TransferMapper.ToResponse(transferGroup);
    }
}
