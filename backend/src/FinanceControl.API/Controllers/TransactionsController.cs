using FinanceControl.Application.DTOs;
using FinanceControl.Application.UseCases.Transactions;
using FinanceControl.Domain.Enums;
using FinanceControl.API;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceControl.API.Controllers;

[ApiController]
[Authorize]
[Route("api/transactions")]
public sealed class TransactionsController : ControllerBase
{
    private readonly RegisterIncomeUseCase _registerIncome;
    private readonly RegisterExpenseUseCase _registerExpense;
    private readonly ListTransactionsUseCase _list;
    private readonly GetTransactionUseCase _get;

    public TransactionsController(
        RegisterIncomeUseCase registerIncome,
        RegisterExpenseUseCase registerExpense,
        ListTransactionsUseCase list,
        GetTransactionUseCase get)
    {
        _registerIncome = registerIncome;
        _registerExpense = registerExpense;
        _list = list;
        _get = get;
    }

    [HttpPost("income")]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionResponse>> RegisterIncome(
        [FromBody] RegisterIncomeRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _registerIncome.ExecuteAsync(request with { OwnerScopeId = User.GetCurrentUserId() }, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id, ownerScopeId = response.OwnerScopeId }, response);
    }

    [HttpPost("expense")]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionResponse>> RegisterExpense(
        [FromBody] RegisterExpenseRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _registerExpense.ExecuteAsync(request with { OwnerScopeId = User.GetCurrentUserId() }, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id, ownerScopeId = response.OwnerScopeId }, response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TransactionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TransactionResponse>>> List(
        [FromQuery] Guid? accountId,
        [FromQuery] Guid? categoryId,
        [FromQuery] Guid? invoiceId,
        [FromQuery] TransactionType? type,
        [FromQuery] DateOnly? competenceDateFrom,
        [FromQuery] DateOnly? competenceDateTo,
        CancellationToken cancellationToken)
    {
        var response = await _list.ExecuteAsync(
            new ListTransactionsRequest(
                User.GetCurrentUserId(),
                accountId,
                categoryId,
                invoiceId,
                type,
                competenceDateFrom,
                competenceDateTo),
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var response = await _get.ExecuteAsync(
            new GetTransactionRequest(User.GetCurrentUserId(), id),
            cancellationToken);

        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id, [FromServices] DeleteTransactionUseCase deleteUseCase, CancellationToken cancellationToken)
    {
        await deleteUseCase.ExecuteAsync(
            new GetTransactionRequest(User.GetCurrentUserId(), id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/settle")]
    public Task<TransactionResponse> Settle(Guid id, SettleExpenseBody request, [FromServices] SettleExpenseUseCase useCase, CancellationToken cancellationToken) =>
        useCase.ExecuteAsync(User.GetCurrentUserId(), id, request.PaymentDate, cancellationToken);
}

public sealed record SettleExpenseBody(DateOnly PaymentDate);
