using FinanceControl.Application.DTOs;
using FinanceControl.Application.Interfaces;

namespace FinanceControl.Application.UseCases.Accounts;

public sealed class ListAccountsUseCase : IUseCase<ListAccountsRequest, IReadOnlyList<AccountResponse>>
{
    private readonly IAccountRepository _accountRepository;

    public ListAccountsUseCase(IAccountRepository accountRepository) => _accountRepository = accountRepository;

    public async Task<IReadOnlyList<AccountResponse>> ExecuteAsync(ListAccountsRequest request, CancellationToken cancellationToken = default)
    {
        var accounts = await _accountRepository.ListByOwnerAsync(request.OwnerScopeId, cancellationToken);
        return accounts.Select(AccountMapper.ToResponse).ToList();
    }
}
