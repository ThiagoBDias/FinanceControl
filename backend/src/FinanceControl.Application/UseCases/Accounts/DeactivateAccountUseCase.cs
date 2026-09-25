using FinanceControl.Application.DTOs;
using FinanceControl.Application.Interfaces;
using FinanceControl.Application.Validators;

namespace FinanceControl.Application.UseCases.Accounts;

public sealed class DeactivateAccountUseCase : IUseCase<DeactivateAccountRequest, AccountResponse>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateAccountUseCase(IAccountRepository accountRepository, IUnitOfWork unitOfWork)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AccountResponse> ExecuteAsync(DeactivateAccountRequest request, CancellationToken cancellationToken = default)
    {
        if (request.OwnerScopeId == Guid.Empty || request.AccountId == Guid.Empty)
            throw new RequestValidationException(new[]
            {
                new ValidationError(nameof(request.AccountId), "OwnerScopeId e AccountId sao obrigatorios.")
            });

        var account = await _accountRepository.GetByIdAsync(request.AccountId, request.OwnerScopeId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Domain.Entities.Account), request.AccountId);

        account.Deactivate();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return AccountMapper.ToResponse(account);
    }
}
