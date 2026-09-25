namespace FinanceControl.Infrastructure.IntegrationTests;

[CollectionDefinition("postgres-integration", DisableParallelization = true)]
public sealed class PostgresIntegrationCollection : ICollectionFixture<PostgresIntegrationFixture>
{
}
