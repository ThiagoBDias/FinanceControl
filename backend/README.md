# Backend FinanceControl

API ASP.NET Core .NET 10 com EF Core, Identity e PostgreSQL. O [README principal](../README.md) documenta o ambiente local, a execução dos testes e as regras de contabilização. O [guia de implantação](../DEPLOY.md) descreve PostgreSQL, segredos, migrações e cadastro restrito em produção.

A API agrupa rotas em `/api/auth`, `/api/accounts`, `/api/categories`, `/api/transactions`, `/api/transfers`, `/api/credit-cards`, `/api/invoices`, `/api/budgets`, `/api/debts` e `/api/dashboard`. O Swagger está disponível em desenvolvimento. No navegador, a sessão é mantida por cookie. Clientes de API podem autenticar com Bearer JWT.
