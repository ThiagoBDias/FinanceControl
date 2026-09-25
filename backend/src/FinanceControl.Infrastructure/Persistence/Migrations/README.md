# Migrations

The initial migration is `20260924_InitialCreate`.

It creates the PostgreSQL schema for the current FinanceControl MVP, including:

- ASP.NET Core Identity tables;
- domain users;
- accounts and credit-card settings;
- categories;
- transactions;
- transfers;
- invoices (visualization only);
- budgets;
- audit log.

The migration is SQL-backed because the environment used to prepare this repository does not have the .NET SDK / `dotnet-ef` installed, so an EF-generated C# migration and model snapshot could not be produced here.

## On a development machine with .NET 8 SDK

From `backend/`:

```bash
dotnet restore
dotnet ef database update --project src/FinanceControl.Infrastructure --startup-project src/FinanceControl.API
```

If you later change the EF model, install/use `dotnet-ef` 8.x and generate the next migration normally. Before doing so, review the initial SQL-backed migration and create a proper model snapshot from the current model.

## PostgreSQL role

The audit table is append-only by application design. A deployment migration should additionally revoke `UPDATE` and `DELETE` from the concrete PostgreSQL application role, because the role name is environment-specific.
