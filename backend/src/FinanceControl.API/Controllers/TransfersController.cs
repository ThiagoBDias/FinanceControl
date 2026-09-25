using FinanceControl.API;
using FinanceControl.Application.DTOs;
using FinanceControl.Application.UseCases.Transfers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceControl.API.Controllers;

[ApiController]
[Authorize]
[Route("api/transfers")]
public sealed class TransfersController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<TransferResponse>> Create(
        CreateTransferRequest request, [FromServices] CreateTransferUseCase useCase, CancellationToken cancellationToken)
        => StatusCode(StatusCodes.Status201Created, await useCase.ExecuteAsync(request with { OwnerScopeId = User.GetCurrentUserId() }, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TransferResponse>> Get(
        Guid id, [FromServices] GetTransferUseCase useCase, CancellationToken cancellationToken)
        => Ok(await useCase.ExecuteAsync(new GetTransferRequest(User.GetCurrentUserId(), id), cancellationToken));
}
