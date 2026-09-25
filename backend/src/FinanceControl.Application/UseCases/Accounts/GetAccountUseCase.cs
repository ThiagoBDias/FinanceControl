using FinanceControl.Application.DTOs;
using FinanceControl.Application.Interfaces;
using FinanceControl.Application.Validators;

namespace FinanceControl.Application.UseCases.Accounts;

public sealed class GetAccountUseCase : IUseCase<GetAccountRequest, AccountResponse>
{
    private readonly IAccountRepository _accountRepository;

    public GetAccountUseCase(IAccountRepository accountRepository) => _accountRepository = accountRepository;

    public async Task<AccountResponse> ExecuteAsync(GetAccountRequest request, CancellationToken cancellationToken = default)
    {
        var account = await _accountRepository.GetByIdAsync(request.AccountId, request.OwnerScopeId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Domain.Entities.Account), request.AccountId);

        return AccountMapper.ToResponse(account);
    }
}
