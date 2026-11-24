using FinanceControl.API;
using FinanceControl.Application.DTOs;
using FinanceControl.Application.UseCases.Invoices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceControl.API.Controllers;

[ApiController]
[Authorize]
[Route("api/invoices")]
public sealed class InvoicesController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InvoiceResponse>>> List(
        [FromQuery] Guid creditCardAccountId, [FromServices] ListInvoicesUseCase useCase, CancellationToken cancellationToken)
    {
        return Ok(await useCase.ExecuteAsync(
            new ListInvoicesRequest(User.GetCurrentUserId(), creditCardAccountId), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InvoiceResponse>> Get(
        Guid id, [FromServices] GetInvoiceUseCase useCase, CancellationToken cancellationToken)
        => Ok(await useCase.ExecuteAsync(new GetInvoiceRequest(User.GetCurrentUserId(), id), cancellationToken));

    [HttpPost("{id:guid}/close")]
    public async Task<ActionResult<InvoiceResponse>> Close(
        Guid id, [FromServices] CloseInvoiceUseCase useCase, CancellationToken cancellationToken)
        => Ok(await useCase.ExecuteAsync(new CloseInvoiceRequest(User.GetCurrentUserId(), id), cancellationToken));

    [HttpPost("{id:guid}/payments")]
    public Task<InvoiceResponse> Pay(Guid id, PayInvoiceRequest request, [FromServices] PayInvoiceUseCase useCase, CancellationToken cancellationToken) =>
        useCase.ExecuteAsync(request with { OwnerScopeId = User.GetCurrentUserId(), InvoiceId = id }, cancellationToken);
}
