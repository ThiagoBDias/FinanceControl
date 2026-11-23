# FinanceControl

Aplicação financeira pessoal em português, com a SPA React/TypeScript, API ASP.NET Core .NET 10 e PostgreSQL. O contêiner da aplicação serve a interface e a API na mesma origem.

## Executar localmente no Windows

Requer Docker Desktop com Docker Compose. Execute os comandos no PowerShell a partir da raiz do projeto.

```powershell
function New-FinanceControlSecret {
  $bytes = New-Object 'byte[]' 32
  $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
  try { $rng.GetBytes($bytes) } finally { $rng.Dispose() }
  return [System.BitConverter]::ToString($bytes).Replace('-', '').ToLowerInvariant()
}

if (!(Test-Path backend/.env)) {
  @(
    "FINANCECONTROL_DB_PASSWORD=$(New-FinanceControlSecret)"
    "FINANCECONTROL_JWT_SECRET=$(New-FinanceControlSecret)"
    "FINANCECONTROL_DB_PORT=55432"
    "FINANCECONTROL_API_PORT=8080"
  ) | Set-Content backend/.env -Encoding ascii
}
docker compose --project-name financecontrol --env-file backend/.env -f backend/docker-compose.yml up --build -d
```

Abra [http://localhost:8080](http://localhost:8080), cadastre uma conta e entre. O modo local libera cadastro e vincula as portas do banco e da aplicação ao loopback. Gere o arquivo `backend/.env` uma única vez por instalação. Preserve as senhas ao reiniciar ou reconstruir o contêiner, pois o volume do PostgreSQL retém suas credenciais e seus dados.

O comando abaixo para os serviços e preserva o volume.

```powershell
docker compose --project-name financecontrol --env-file backend/.env -f backend/docker-compose.yml down
```

## Desenvolvimento

Com SDK .NET 10, Node 24 e a pilha local em execução, rode `npm ci` e `npm run dev` em `frontend`. O Vite encaminha `/api` ao backend local. Altere `FINANCECONTROL_DEV_API` no ambiente do Vite se a API usar outra porta.

Para rodar o backend fora do Docker, inicie apenas o serviço `postgres` do Compose e configure as variáveis abaixo no ambiente do processo.

- `FINANCECONTROL_CONNECTION_STRING` no formato Npgsql, com host `127.0.0.1`, porta `55432`, banco `financecontrol` e as credenciais do ambiente isolado.
- `FINANCECONTROL_JWT_SECRET` com 32 bytes aleatórios ou mais.
- `ASPNETCORE_ENVIRONMENT=Development`, `Auth__AllowLocalRegistration=true` e `Database__ApplyMigrations=true` em desenvolvimento.
- `ASPNETCORE_URLS=http://127.0.0.1:8080` para manter o proxy padrão do Vite.

```sh
dotnet run --project backend/src/FinanceControl.API --no-launch-profile
```

## Validação

```sh
dotnet build backend/FinanceControl.sln -m:1
dotnet test backend/FinanceControl.sln -m:1
npm --prefix frontend ci
npm --prefix frontend run build
npm --prefix frontend audit
```

Os testes de integração iniciam PostgreSQL 16 via Testcontainers. Configure `FINANCECONTROL_TEST_POSTGRES` com uma instância exclusivamente de testes para rodar sem Docker. A role precisa de `CREATEDB`. A suíte cria bancos com nomes aleatórios prefixados com `financecontrol_test_` e remove apenas os bancos criados por ela.

Instale o navegador do Playwright com `npx playwright install chromium` em `frontend`. Rode a suíte ponta a ponta contra uma instalação local com cadastro de desenvolvimento e banco descartável. Configure `FINANCECONTROL_E2E_URL` caso a aplicação esteja em outra porta. A suíte cria usuários e registros sintéticos no banco indicado.

```sh
npm --prefix frontend run test:e2e -- --grep-invert "survives|production"
```

Para o cenário de persistência, defina `FINANCECONTROL_PERSISTENCE_FILE` com um caminho temporário antes desse comando. Depois de reiniciar a API e o PostgreSQL preservando o volume, execute `npm --prefix frontend run test:e2e -- --grep survives` com a mesma variável.

## Critérios financeiros

- As contas exibem o saldo do livro de movimentações. A conta a pagar entra no saldo e no relatório de competência após a baixa. Lançamentos anteriores sem data de pagamento conservam a interpretação original.
- Transferências geram duas movimentações atômicas e ficam fora das receitas e despesas. Pagamentos de fatura registram a saída da conta, a baixa do cartão e o vínculo com a fatura na mesma transação.
- Compras parceladas preservam todos os centavos. As primeiras parcelas recebem os centavos residuais. A competência avança da data original e cada parcela vai para o ciclo seguinte do cartão.
- O limite usado inclui parcelas futuras. O saldo em contas e o saldo após compromissos de cartão aparecem separadamente. Faturas com pagamento parcial mantêm o saldo remanescente.
- Dívidas usam parcelas mensais calculadas a partir do total a pagar informado, incluindo juros já conhecidos. A baixa grava a saída e mantém a data de competência contratual. A projeção usa o saldo restante de faturas, contas programadas e parcelas de dívidas.
- O fechamento de faturas é manual. Fechar um ciclo antes de registrar todas as compras impede lançamentos retroativos naquele ciclo. Compras parceladas e pagamentos vinculados ficam protegidos contra exclusão individual.
- A API aceita decimais e valores monetários como strings decimais. `Idempotency-Key` deve ser um UUID para POST em lançamentos, transferências, faturas e dívidas. A mesma chave com o mesmo corpo e rota reproduz a resposta original. Uma chave usada com conteúdo diferente retorna 409.

O procedimento para infraestrutura e migrações de produção está no [guia de implantação](DEPLOY.md).
