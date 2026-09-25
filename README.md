# FinanceControl - Backend MVP

Backend em .NET 8 para controle financeiro pessoal. O foco do MVP e simples: saber quanto existe, quanto entrou, quanto saiu e para onde o dinheiro foi.

## Escopo

- contas e saldos;
- categorias;
- entradas e despesas;
- transferencias entre contas;
- cartoes de credito;
- faturas somente para visualizacao e fechamento;
- orcamento por categoria;
- dashboard financeiro;
- autenticacao JWT;
- auditoria;
- PostgreSQL + EF Core.

Nao existe pagamento de fatura no MVP.

## Requisitos

- .NET 8 SDK
- PostgreSQL 16+
- Docker somente se quiser executar os testes de integracao automaticamente.

## Configuracao

Defina uma connection string e um segredo JWT com pelo menos 32 caracteres. Por exemplo no PowerShell:

```powershell
$env:FINANCECONTROL_CONNECTION_STRING="Host=localhost;Port=5432;Database=financecontrol;Username=postgres;Password=postgres"
$env:FINANCECONTROL_JWT_SECRET="troque-esta-chave-por-uma-chave-secreta-com-32-caracteres"
```

Ou configure `ConnectionStrings:DefaultConnection` e `Jwt:Secret` nos arquivos de configuracao/Secret Manager.

## Banco

A migration inicial fica em `src/FinanceControl.Infrastructure/Persistence/Migrations/`. Em um ambiente com .NET 8 e acesso ao PostgreSQL:

```bash
dotnet restore
dotnet ef database update --project src/FinanceControl.Infrastructure --startup-project src/FinanceControl.API
```

## Executar API

```bash
dotnet restore
dotnet run --project src/FinanceControl.API
```

Em Development, o Swagger fica habilitado. Primeiro use `/api/auth/register`, depois `/api/auth/login` e informe o token Bearer no Swagger.

## Testes

Testes de dominio/application:

```bash
dotnet test tests/FinanceControl.Domain.Tests
dotnet test tests/FinanceControl.Application.Tests
```

Testes de integracao com PostgreSQL real:

```bash
dotnet test tests/FinanceControl.Infrastructure.IntegrationTests
```

Os testes de integracao usam Testcontainers e precisam de Docker.

## Endpoints principais

- `/api/auth`
- `/api/accounts`
- `/api/categories`
- `/api/transactions`
- `/api/transfers`
- `/api/credit-cards`
- `/api/invoices`
- `/api/budgets`
- `/api/dashboard`
- `/health`
