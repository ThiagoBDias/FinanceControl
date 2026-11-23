using FinanceControl.Domain.Entities;
using FinanceControl.Domain.Enums;
using FinanceControl.Infrastructure.Persistence;
using FinanceControl.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace FinanceControl.Infrastructure.IntegrationTests;

[Collection("postgres-integration")]
public sealed class PersistenceIntegrationTests
{
    private readonly PostgresIntegrationFixture _fixture;

    public PersistenceIntegrationTests(PostgresIntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task PostgreSqlSchema_CanBeCreated()
    {
        await _fixture.ResetDatabaseAsync();

        await using var context = _fixture.CreateContext();

        Assert.True(await context.Database.CanConnectAsync());

        var accountTableExists = await context.Database.SqlQueryRaw<bool>(
            "SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'Accounts') AS \"Value\"")
            .SingleAsync();

        Assert.True(accountTableExists);
    }

    [Fact]
    public async Task AccountAndTransactions_RoundTripThroughPostgreSql_AndRecalculateBalance()
    {
        await _fixture.ResetDatabaseAsync();

        var ownerId = Guid.NewGuid();
        var expenseCategory = Category.Create(ownerId, "Alimentacao", CategoryType.Expense);
        var account = Account.CreateAsset(ownerId, "Conta Corrente", AccountType.Checking, 1000m);
        account.RegisterIncome(500m, new DateOnly(2026, 9, 24), "Salario");
        account.RegisterExpense(350m, new DateOnly(2026, 9, 24), "Mercado", expenseCategory);

        await using (var context = _fixture.CreateContext())
        {
            context.Categories.Add(expenseCategory);
            context.Accounts.Add(account);
            await context.SaveChangesAsync();
        }

        await using var readContext = _fixture.CreateContext();
        var repository = new AccountRepository(readContext);
        var loaded = await repository.GetByIdAsync(account.Id, ownerId);

        Assert.NotNull(loaded);
        Assert.Equal(1150m, loaded!.CalculatedBalance);
        Assert.Equal(1150m, loaded.CachedBalance);
        Assert.Equal(2, loaded.Transactions.Count);
    }

    [Fact]
    public async Task CreditCardSettings_ArePersistedInOwnedTable()
    {
        await _fixture.ResetDatabaseAsync();

        var ownerId = Guid.NewGuid();
        var settings = FinanceControl.Domain.ValueObjects.CreditCardSettings.Create(3000m, 10, 20);
        var card = Account.CreateCreditCard(ownerId, "Nubank", settings);

        await using (var context = _fixture.CreateContext())
        {
            context.Accounts.Add(card);
            await context.SaveChangesAsync();
        }

        await using var readContext = _fixture.CreateContext();
        var loaded = await readContext.Accounts.SingleAsync(a => a.Id == card.Id);

        Assert.NotNull(loaded.CreditCardSettings);
        Assert.Equal(3000m, loaded.CreditCardSettings!.CreditLimit);
        Assert.Equal(10, loaded.CreditCardSettings.ClosingDay);
        Assert.Equal(20, loaded.CreditCardSettings.DueDay);
    }

    [Fact]
    public async Task AccountRepository_RespectsOwnerScope()
    {
        await _fixture.ResetDatabaseAsync();

        var ownerA = Guid.NewGuid();
        var ownerB = Guid.NewGuid();
        var account = Account.CreateAsset(ownerA, "Conta A", AccountType.Checking, 100m);

        await using (var context = _fixture.CreateContext())
        {
            context.Accounts.Add(account);
            await context.SaveChangesAsync();
        }

        await using var readContext = _fixture.CreateContext();
        var repository = new AccountRepository(readContext);

        var foundWithCorrectOwner = await repository.GetByIdAsync(account.Id, ownerA);
        var hiddenFromOtherOwner = await repository.GetByIdAsync(account.Id, ownerB);

        Assert.NotNull(foundWithCorrectOwner);
        Assert.Null(hiddenFromOtherOwner);
    }

    [Fact]
    public async Task SoftDeletedTransaction_IsHiddenByGlobalQueryFilter()
    {
        await _fixture.ResetDatabaseAsync();

        var ownerId = Guid.NewGuid();
        var account = Account.CreateAsset(ownerId, "Conta", AccountType.Checking, 100m);
        var transaction = account.RegisterExpense(25m, new DateOnly(2026, 9, 24), "Mercado");

        await using (var context = _fixture.CreateContext())
        {
            context.Accounts.Add(account);
            await context.SaveChangesAsync();
        }

        await using (var updateContext = _fixture.CreateContext())
        {
            var persisted = await updateContext.Transactions.SingleAsync(t => t.Id == transaction.Id);
            persisted.Delete();
            await updateContext.SaveChangesAsync();
        }

        await using var readContext = _fixture.CreateContext();
        Assert.Empty(await readContext.Transactions.Where(t => t.Id == transaction.Id).ToListAsync());
        Assert.Single(await readContext.Transactions.IgnoreQueryFilters().Where(t => t.Id == transaction.Id).ToListAsync());
    }

    [Fact]
    public async Task AuditLog_IsWrittenInTheSameSaveOperation()
    {
        await _fixture.ResetDatabaseAsync();

        var ownerId = Guid.NewGuid();
        var account = Account.CreateAsset(ownerId, "Conta Auditada", AccountType.Checking, 250m);

        await using var context = _fixture.CreateContext();
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var auditCount = await context.Database.SqlQueryRaw<long>(
            "SELECT COUNT(*) AS \"Value\" FROM \"AuditLogEntries\" WHERE \"EntityName\" = 'Account' AND \"EntityId\" = {0}",
            account.Id)
            .SingleAsync();

        Assert.Equal(1, auditCount);
    }

    [Fact]
    public async Task CategoryName_IsUniquePerOwnerAtDatabaseLevel()
    {
        await _fixture.ResetDatabaseAsync();

        var ownerId = Guid.NewGuid();
        var first = Category.Create(ownerId, "Mercado", CategoryType.Expense);
        var duplicate = Category.Create(ownerId, "Mercado", CategoryType.Expense);

        await using var context = _fixture.CreateContext();
        context.Categories.Add(first);
        await context.SaveChangesAsync();

        context.Categories.Add(duplicate);
        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }
}
