using FinanceControl.Application.DTOs;
using FinanceControl.Application.UseCases.Budgets;
using FinanceControl.API;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceControl.API.Controllers;

[ApiController]
[Authorize]
[Route("api/budgets")]
public sealed class BudgetsController : ControllerBase
{
    [HttpPost]
    public Task<BudgetResponse> Create(CreateBudgetRequest request, [FromServices] CreateBudgetUseCase useCase, CancellationToken cancellationToken) => useCase.ExecuteAsync(request with { OwnerScopeId = User.GetCurrentUserId() }, cancellationToken);

    [HttpGet]
    public Task<IReadOnlyList<BudgetResponse>> List([FromQuery] int year, [FromQuery] int month, [FromServices] ListBudgetsUseCase useCase, CancellationToken cancellationToken) =>
        useCase.ExecuteAsync(new ListBudgetsRequest(User.GetCurrentUserId(), year, month), cancellationToken);

    [HttpPut("{id:guid}")]
    public Task<BudgetResponse> Update(Guid id, UpdateBudgetBody body, [FromServices] UpdateBudgetUseCase useCase, CancellationToken cancellationToken) =>
        useCase.ExecuteAsync(new UpdateBudgetRequest(User.GetCurrentUserId(), id, body.LimitAmount), cancellationToken);
}

public sealed record UpdateBudgetBody(decimal LimitAmount);
