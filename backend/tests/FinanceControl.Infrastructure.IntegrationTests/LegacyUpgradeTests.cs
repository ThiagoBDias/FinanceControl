using Microsoft.EntityFrameworkCore;

namespace FinanceControl.Infrastructure.IntegrationTests;

[Collection("postgres-integration")]
public sealed class LegacyUpgradeTests(PostgresIntegrationFixture fixture)
{
    [Fact]
    public async Task UpgradePreservesExistingCardBalanceAndCreatesPayableOpeningInvoice()
    {
        await fixture.ResetDatabaseAsync("20260924180000_InitialCreate");
        await using var db = fixture.CreateContext();
        var accountId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "Accounts"
                ("Id","OwnerScopeId","Name","Type","InitialBalance","CachedBalance","CachedBalanceUpdatedAt","IsActive","CreatedAt","UpdatedAt")
            VALUES ({accountId},{ownerId},'Cartão anterior','CreditCard',125.67,125.67,NOW(),true,NOW(),NOW());
            """);

        await db.Database.MigrateAsync();
        var account = await db.Accounts.SingleAsync(item => item.Id == accountId);
        var invoice = await db.Invoices.SingleAsync(item => item.CreditCardAccountId == accountId);
        Assert.Equal(125.67m, account.CalculatedBalance);
        Assert.Equal(ownerId, invoice.OwnerScopeId);
        Assert.Equal(125.67m, invoice.OpeningAmount);
        Assert.Equal(0m, invoice.PaidAmount);
        Assert.Equal(2, (await db.Database.GetAppliedMigrationsAsync()).Count());
    }
}
