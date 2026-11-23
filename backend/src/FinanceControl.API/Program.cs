using System.Threading.RateLimiting;
using FinanceControl.API;
using FinanceControl.Application.UseCases.Debts;
using FinanceControl.API.Middlewares;
using FinanceControl.Application.DTOs;
using FinanceControl.Application.Interfaces;
using FinanceControl.Application.UseCases.Accounts;
using FinanceControl.Application.UseCases.Categories;
using FinanceControl.Application.UseCases.CreditCards;
using FinanceControl.Application.UseCases.Budgets;
using FinanceControl.Application.UseCases.Invoices;
using FinanceControl.Application.UseCases.Transfers;
using FinanceControl.Application.UseCases.Dashboard;
using FinanceControl.Application.UseCases.Transactions;
using FinanceControl.Application.Validators;
using FinanceControl.Infrastructure;
using FinanceControl.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = 128 * 1024);
builder.Services.AddControllers();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("Auth", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "local",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 30, Window = TimeSpan.FromMinutes(1) }));
});
builder.Services.AddEndpointsApiExplorer();
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy =>
    policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe o JWT como: Bearer {token}"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddProblemDetails();

var connectionString = Environment.GetEnvironmentVariable("FINANCECONTROL_CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("Configure FINANCECONTROL_CONNECTION_STRING.");

builder.Services.AddFinanceControlInfrastructure(connectionString, builder.Configuration, SessionCookie.Name(builder.Environment.IsDevelopment()));

// Contas.
builder.Services.AddScoped<CreateAccountUseCase>();
builder.Services.AddScoped<ListAccountsUseCase>();
builder.Services.AddScoped<GetAccountUseCase>();
builder.Services.AddScoped<UpdateAccountUseCase>();
builder.Services.AddScoped<DeactivateAccountUseCase>();

// Categorias.
builder.Services.AddScoped<CreateCategoryUseCase>();
builder.Services.AddScoped<ListCategoriesUseCase>();
builder.Services.AddScoped<UpdateCategoryUseCase>();
builder.Services.AddScoped<DeactivateCategoryUseCase>();

// Lancamentos.
builder.Services.AddScoped<RegisterIncomeUseCase>();
builder.Services.AddScoped<RegisterExpenseUseCase>();
builder.Services.AddScoped<ListTransactionsUseCase>();
builder.Services.AddScoped<GetTransactionUseCase>();
builder.Services.AddScoped<DeleteTransactionUseCase>();

// Cartoes.
builder.Services.AddScoped<CreateCreditCardUseCase>();
builder.Services.AddScoped<ListCreditCardsUseCase>();
builder.Services.AddScoped<GetCreditCardUseCase>();

// Faturas, pagamentos e dívidas.
builder.Services.AddScoped<ListInvoicesUseCase>();
builder.Services.AddScoped<GetInvoiceUseCase>();
builder.Services.AddScoped<CloseInvoiceUseCase>();
builder.Services.AddScoped<PayInvoiceUseCase>();
builder.Services.AddScoped<DebtUseCases>();
builder.Services.AddScoped<SettleExpenseUseCase>();

// Transferencias.
builder.Services.AddScoped<CreateTransferUseCase>();
builder.Services.AddScoped<GetTransferUseCase>();

// Orcamentos.
builder.Services.AddScoped<CreateBudgetUseCase>();
builder.Services.AddScoped<ListBudgetsUseCase>();
builder.Services.AddScoped<UpdateBudgetUseCase>();

// Dashboard.
builder.Services.AddScoped<GetDashboardUseCase>();

builder.Services.AddScoped<IValidator<CreateAccountRequest>, CreateAccountRequestValidator>();
builder.Services.AddScoped<IValidator<UpdateAccountRequest>, UpdateAccountRequestValidator>();
builder.Services.AddScoped<IValidator<CreateCategoryRequest>, CreateCategoryRequestValidator>();
builder.Services.AddScoped<IValidator<UpdateCategoryRequest>, UpdateCategoryRequestValidator>();
builder.Services.AddScoped<IValidator<RegisterIncomeRequest>, RegisterIncomeRequestValidator>();
builder.Services.AddScoped<IValidator<RegisterExpenseRequest>, RegisterExpenseRequestValidator>();
builder.Services.AddScoped<IValidator<CreateCreditCardRequest>, CreateCreditCardRequestValidator>();
builder.Services.AddScoped<IValidator<CreateTransferRequest>, CreateTransferRequestValidator>();

var app = builder.Build();

if (args.Contains("--migrate") || app.Configuration.GetValue<bool>("Database:ApplyMigrations"))
{
    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<FinanceControlDbContext>();
    await dbContext.Database.MigrateAsync();
    if (args.Contains("--migrate"))
        return;
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Use(async (context, next) =>
{
    context.Response.Headers.XContentTypeOptions = "nosniff";
    context.Response.Headers.XFrameOptions = "DENY";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    if (!app.Environment.IsDevelopment())
        context.Response.Headers.StrictTransportSecurity = "max-age=31536000";
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        context.Response.Headers.CacheControl = "no-store";
        var hasSession = context.Request.Cookies.ContainsKey(SessionCookie.Name(app.Environment.IsDevelopment()));
        var changesState = !HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method) &&
            !HttpMethods.IsOptions(context.Request.Method);
        if (hasSession && changesState && context.Request.Headers[SessionCookie.BrowserHeader] != "browser")
        {
            await Results.Problem(statusCode: 403, detail: "Origem inválida para esta operação.").ExecuteAsync(context);
            return;
        }
    }
    else if (!app.Environment.IsDevelopment())
        context.Response.Headers.ContentSecurityPolicy =
            "default-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; frame-ancestors 'none'; base-uri 'self'; form-action 'self'";
    await next();
});
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("Frontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<FinancialMutationMiddleware>();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    service = "FinanceControl.API"
}));

app.MapFallback("/api/{**path}", () => Results.NotFound());
if (app.Environment.WebRootFileProvider.GetFileInfo("index.html").Exists)
    app.MapFallbackToFile("index.html");

app.Run();
