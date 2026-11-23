using FinanceControl.Application.DTOs;
using FinanceControl.Application.UseCases.Debts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceControl.API.Controllers;

[ApiController]
[Authorize]
[Route("api/debts")]
public sealed class DebtsController(DebtUseCases debts) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<DebtResponse>> List(CancellationToken cancellationToken) =>
        debts.ListAsync(User.GetCurrentUserId(), cancellationToken);

    [HttpPost]
    public async Task<ActionResult<DebtResponse>> Create(CreateDebtRequest request, CancellationToken cancellationToken) =>
        StatusCode(StatusCodes.Status201Created, await debts.CreateAsync(User.GetCurrentUserId(), request, cancellationToken));

    [HttpPost("{id:guid}/installments/{number:int}/pay")]
    public Task<DebtResponse> Pay(Guid id, int number, PayDebtInstallmentRequest request, CancellationToken cancellationToken) =>
        debts.PayAsync(User.GetCurrentUserId(), id, number, request, cancellationToken);
}
