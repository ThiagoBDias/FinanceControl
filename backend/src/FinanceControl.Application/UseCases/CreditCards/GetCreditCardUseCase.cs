using FinanceControl.Application.DTOs;
using FinanceControl.Application.Interfaces;
using FinanceControl.Application.Validators;
using FinanceControl.Domain.Enums;

namespace FinanceControl.Application.UseCases.CreditCards;

public sealed class GetCreditCardUseCase : IUseCase<GetCreditCardRequest, AccountResponse>
{
    private readonly IAccountRepository _accountRepository;

    public GetCreditCardUseCase(IAccountRepository accountRepository) => _accountRepository = accountRepository;

    public async Task<AccountResponse> ExecuteAsync(GetCreditCardRequest request, CancellationToken cancellationToken = default)
    {
        var account = await _accountRepository.GetByIdAsync(request.AccountId, request.OwnerScopeId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Domain.Entities.Account), request.AccountId);

        if (account.Type != AccountType.CreditCard)
            throw new EntityNotFoundException(nameof(Domain.Entities.Account), request.AccountId);

        return AccountMapper.ToResponse(account);
    }
}
