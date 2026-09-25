# FinanceControl API - MVP

## Authentication
- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/auth/me`

Todas as rotas financeiras exigem `Authorization: Bearer <token>`. O `OwnerScopeId` vem do token; o cliente nao escolhe o usuario.

## Recursos
- Contas: `POST/GET /api/accounts`, `GET/PUT /api/accounts/{id}`, `POST /api/accounts/{id}/deactivate`
- Categorias: `POST/GET /api/categories`, `PUT /api/categories/{id}`, `POST /api/categories/{id}/deactivate`
- Lancamentos: `POST /api/transactions/income`, `POST /api/transactions/expense`, `GET /api/transactions`, `GET /api/transactions/{id}`, `DELETE /api/transactions/{id}`
- Cartoes: `POST/GET /api/credit-cards`, `GET /api/credit-cards/{id}`
- Faturas: `GET /api/invoices?creditCardAccountId={id}`, `GET /api/invoices/{id}`, `POST /api/invoices/{id}/close`
- Transferencias: `POST /api/transfers`, `GET /api/transfers/{id}`
- Orcamentos: `POST/GET /api/budgets`, `PUT /api/budgets/{id}`
- Dashboard: `GET /api/dashboard?year=2026&month=9`
- Health: `GET /health`

Faturas sao somente para visualizacao/ciclo de fechamento. Nao existe pagamento de fatura no MVP.

## Regras
- Transferencias entre contas nao entram como receita/despesa.
- Cartao gera despesa e vincula automaticamente a fatura do periodo.
- Uma fatura calcula seu total pelos lancamentos vinculados.
- Orcamento calcula o gasto a partir dos lancamentos, sem duplicar esse valor no banco.
