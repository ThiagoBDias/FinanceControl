using FinanceControl.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using Testcontainers.PostgreSql;

namespace FinanceControl.Infrastructure.IntegrationTests;

public sealed class PostgresIntegrationFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _container;
    private string? _isolatedDatabase;
    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        var server = Environment.GetEnvironmentVariable("FINANCECONTROL_TEST_POSTGRES");
        if (string.IsNullOrWhiteSpace(server))
        {
            _container = new PostgreSqlBuilder("postgres:16-alpine")
                .WithDatabase("postgres").WithUsername("postgres").WithPassword("postgres").Build();
            await _container.StartAsync();
            server = _container.GetConnectionString();
        }

        ServerConnectionString = server;
    }

    private string ServerConnectionString { get; set; } = string.Empty;

    public async Task DisposeAsync()
    {
        await DropIsolatedDatabaseAsync();
        if (_container is not null)
            await _container.DisposeAsync();
    }

    public FinanceControlDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<FinanceControlDbContext>()
            .UseNpgsql(ConnectionString).EnableDetailedErrors().Options;
        return new FinanceControlDbContext(options);
    }

    public async Task ResetDatabaseAsync(string? targetMigration = null)
    {
        await DropIsolatedDatabaseAsync();
        var databaseName = $"financecontrol_test_{Guid.NewGuid():N}";
        await using var server = new NpgsqlConnection(ServerConnectionString);
        await server.OpenAsync();
        await using var command = new NpgsqlCommand($"CREATE DATABASE {databaseName}", server);
        await command.ExecuteNonQueryAsync();
        _isolatedDatabase = databaseName;
        ConnectionString = new NpgsqlConnectionStringBuilder(ServerConnectionString)
        {
            Database = _isolatedDatabase,
            Pooling = false
        }.ConnectionString;
        await using var context = CreateContext();
        await context.GetService<IMigrator>().MigrateAsync(targetMigration);
    }

    private async Task DropIsolatedDatabaseAsync()
    {
        if (_isolatedDatabase is null) return;
        await using var server = new NpgsqlConnection(ServerConnectionString);
        await server.OpenAsync();
        await using var command = new NpgsqlCommand($"DROP DATABASE IF EXISTS {_isolatedDatabase}", server);
        await command.ExecuteNonQueryAsync();
        _isolatedDatabase = null;
    }
}
