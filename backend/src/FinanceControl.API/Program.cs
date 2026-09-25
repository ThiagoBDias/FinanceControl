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
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
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

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("FINANCECONTROL_CONNECTION_STRING")
    ?? throw new InvalidOperationException(
        "Configure ConnectionStrings:DefaultConnection ou FINANCECONTROL_CONNECTION_STRING.");

builder.Services.AddFinanceControlInfrastructure(connectionString, builder.Configuration);

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

// Faturas (somente visualizacao/ciclo de fechamento).
builder.Services.AddScoped<ListInvoicesUseCase>();
builder.Services.AddScoped<GetInvoiceUseCase>();
builder.Services.AddScoped<CloseInvoiceUseCase>();

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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    service = "FinanceControl.API"
}));

app.Run();
