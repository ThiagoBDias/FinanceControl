using System.Security.Cryptography;
using System.Text;
using FinanceControl.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace FinanceControl.API.Middlewares;

public sealed class FinancialMutationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, FinanceControlDbContext database)
    {
        var request = context.Request;
        var financialWrite = request.Path.StartsWithSegments("/api") &&
            !request.Path.StartsWithSegments("/api/auth") &&
            context.GetEndpoint()?.Metadata.GetMetadata<AuthorizeAttribute>() is not null &&
            (HttpMethods.IsPost(request.Method) || HttpMethods.IsPut(request.Method) || HttpMethods.IsDelete(request.Method));
        if (!financialWrite || context.User.Identity?.IsAuthenticated != true)
        {
            await next(context);
            return;
        }

        var owner = context.User.GetCurrentUserId();
        var cancellationToken = context.RequestAborted;
        var hasKey = request.Headers.TryGetValue("Idempotency-Key", out var suppliedKey);
        var mandatory = HttpMethods.IsPost(request.Method) &&
            (request.Path.StartsWithSegments("/api/transactions") || request.Path.StartsWithSegments("/api/transfers") ||
             request.Path.StartsWithSegments("/api/invoices") || request.Path.StartsWithSegments("/api/debts"));
        var requestId = Guid.Empty;
        if ((mandatory || hasKey) && (!Guid.TryParse(suppliedKey, out requestId) || requestId == Guid.Empty))
        {
            await Results.Problem(statusCode: 400, detail: "Envie um Idempotency-Key com um UUID válido.").ExecuteAsync(context);
            return;
        }

        string? fingerprint = null;
        if (hasKey)
        {
            request.EnableBuffering();
            var bodyHash = await SHA256.HashDataAsync(request.Body, cancellationToken);
            request.Body.Position = 0;
            fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
                $"{request.Method}:{request.Path}:{request.QueryString}:{Convert.ToHexString(bodyHash)}")));
        }

        await using var transaction = await database.Database.BeginTransactionAsync(cancellationToken);
        // All writers for one user serialize across app instances before reading their ledger.
        await database.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock(hashtextextended({owner.ToString()}, 5419))", cancellationToken);
        var requests = database.Set<FinancialRequest>();
        if (hasKey)
        {
            var existing = await requests.FindAsync([owner, requestId], cancellationToken);
            if (existing is not null)
            {
                if (existing.Fingerprint != fingerprint)
                {
                    await Results.Problem(statusCode: 409, detail: "Esta chave já foi usada para outra operação.").ExecuteAsync(context);
                    return;
                }

                context.Response.StatusCode = existing.StatusCode;
                context.Response.ContentType = existing.ContentType;
                if (existing.Location is not null)
                    context.Response.Headers.Location = existing.Location;
                await context.Response.Body.WriteAsync(existing.Response, cancellationToken);
                return;
            }
        }

        var original = context.Response.Body;
        using var buffer = new MemoryStream();
        context.Response.Body = buffer;
        try
        {
            await next(context);
            if (context.Response.StatusCode is >= 200 and < 300)
            {
                if (hasKey)
                {
                    requests.Add(new FinancialRequest(owner, requestId, fingerprint!, context.Response.StatusCode,
                        context.Response.ContentType, context.Response.Headers.Location.FirstOrDefault(), buffer.ToArray()));
                    await database.SaveChangesAsync(cancellationToken);
                }
                await transaction.CommitAsync(cancellationToken);
            }
        }
        finally
        {
            context.Response.Body = original;
        }

        buffer.Position = 0;
        await buffer.CopyToAsync(original, cancellationToken);
    }
}
