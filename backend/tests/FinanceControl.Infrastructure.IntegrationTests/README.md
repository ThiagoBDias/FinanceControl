# Integration tests - PostgreSQL

These tests exercise the real EF Core/PostgreSQL persistence layer using a disposable PostgreSQL 16 container through Testcontainers.

## Requirements

- .NET 8 SDK
- Docker Desktop (or a Docker-compatible daemon)

## Run

From `backend/`:

```bash
dotnet test tests/FinanceControl.Infrastructure.IntegrationTests/FinanceControl.Infrastructure.IntegrationTests.csproj
```

The suite creates a temporary PostgreSQL container, recreates the schema for each test, runs the assertions against the real database, and removes the container when the suite finishes.

## What is covered

- PostgreSQL connectivity and schema creation
- Account + transaction persistence and balance recalculation
- Credit card owned settings persistence
- OwnerScope isolation at repository level
- Transaction soft-delete query filter
- Audit log persistence in the same SaveChanges operation
- Database uniqueness constraint for category names per owner

These tests intentionally use PostgreSQL rather than the EF Core InMemory provider, because the goal is to catch provider-specific mapping, constraints, SQL and persistence behavior.
