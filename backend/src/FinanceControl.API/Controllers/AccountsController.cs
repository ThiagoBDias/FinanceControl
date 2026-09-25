using FinanceControl.Application.DTOs;
using FinanceControl.Application.UseCases.Accounts;
using FinanceControl.API;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceControl.API.Controllers;

[ApiController]
[Authorize]
[Route("api/accounts")]
public sealed class AccountsController : ControllerBase
{
    private readonly CreateAccountUseCase _create;
    private readonly ListAccountsUseCase _list;
    private readonly GetAccountUseCase _get;
    private readonly UpdateAccountUseCase _update;
    private readonly DeactivateAccountUseCase _deactivate;

    public AccountsController(
        CreateAccountUseCase create,
        ListAccountsUseCase list,
        GetAccountUseCase get,
        UpdateAccountUseCase update,
        DeactivateAccountUseCase deactivate)
    {
        _create = create;
        _list = list;
        _get = get;
        _update = update;
        _deactivate = deactivate;
    }

    [HttpPost]
    [ProducesResponseType(typeof(AccountResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AccountResponse>> Create(
        [FromBody] CreateAccountRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _create.ExecuteAsync(request with { OwnerScopeId = User.GetCurrentUserId() }, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id, ownerScopeId = response.OwnerScopeId }, response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AccountResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AccountResponse>>> List(CancellationToken cancellationToken)
    {
        var ownerScopeId = User.GetCurrentUserId();
        var response = await _list.ExecuteAsync(new ListAccountsRequest(ownerScopeId), cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var ownerScopeId = User.GetCurrentUserId();
        var response = await _get.ExecuteAsync(new GetAccountRequest(ownerScopeId, id), cancellationToken);
        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountResponse>> Update(
        Guid id,
        [FromBody] UpdateAccountBody body,
        CancellationToken cancellationToken)
    {
        var response = await _update.ExecuteAsync(
            new UpdateAccountRequest(User.GetCurrentUserId(), id, body.Name),
            cancellationToken);

        return Ok(response);
    }

    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(typeof(AccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountResponse>> Deactivate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var response = await _deactivate.ExecuteAsync(
            new DeactivateAccountRequest(User.GetCurrentUserId(), id),
            cancellationToken);

        return Ok(response);
    }
}

public sealed record UpdateAccountBody(string Name);
