using FinanceControl.Application.DTOs;
using FinanceControl.Application.Interfaces;
using FinanceControl.Domain.Enums;

namespace FinanceControl.Application.UseCases.CreditCards;

public sealed class ListCreditCardsUseCase : IUseCase<ListCreditCardsRequest, IReadOnlyList<AccountResponse>>
{
    private readonly IAccountRepository _accountRepository;

    public ListCreditCardsUseCase(IAccountRepository accountRepository) => _accountRepository = accountRepository;

    public async Task<IReadOnlyList<AccountResponse>> ExecuteAsync(ListCreditCardsRequest request, CancellationToken cancellationToken = default)
    {
        var accounts = await _accountRepository.ListByOwnerAsync(request.OwnerScopeId, cancellationToken);

        return accounts
            .Where(a => a.Type == AccountType.CreditCard)
            .Select(AccountMapper.ToResponse)
            .ToList();
    }
}
