using FinanceControl.Application.DTOs;
using FinanceControl.Application.Interfaces;
using FinanceControl.Application.Validators;
using FinanceControl.Domain.Entities;

namespace FinanceControl.Application.UseCases.Accounts;

public sealed class CreateAccountUseCase : IUseCase<CreateAccountRequest, AccountResponse>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateAccountRequest> _validator;

    public CreateAccountUseCase(
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreateAccountRequest> validator)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<AccountResponse> ExecuteAsync(CreateAccountRequest request, CancellationToken cancellationToken = default)
    {
        var validation = _validator.Validate(request);
        if (!validation.IsValid)
            throw new RequestValidationException(validation.Errors);

        var account = Account.CreateAsset(request.OwnerScopeId, request.Name, request.Type, request.InitialBalance);

        await _accountRepository.AddAsync(account, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return AccountMapper.ToResponse(account);
    }
}
