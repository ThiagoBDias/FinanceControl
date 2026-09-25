# Revisao final do backend

## Resultado

O backend foi revisado para o escopo MVP definido durante o projeto. Nesta revisao foram corrigidos pontos que ainda impediam considerar a API coerente como um conjunto, principalmente endpoints que existiam na Application mas ainda nao estavam expostos pela API, registro de casos de uso no DI, configuracao do segredo JWT por variavel de ambiente, filtros de lancamento por fatura e consulta consolidada do dashboard.

## Funcionalidades fechadas

- autenticacao e isolamento por usuario;
- contas;
- categorias;
- lancamentos;
- transferencias;
- cartoes;
- faturas sem pagamento;
- orcamentos;
- dashboard;
- auditoria;
- migrations;
- testes unitarios/de dominio/application;
- testes de integracao PostgreSQL com Testcontainers.

## Correcoes desta revisao

1. Adicionados controllers para cartoes, faturas, transferencias e dashboard.
2. Todos os UseCases passaram a estar registrados no DI.
3. Parametros de UseCase injetados diretamente nas actions foram marcados com `[FromServices]`.
4. Dashboard passou a existir de fato, com saldo liquido, entradas, despesas, categorias, contas, cartoes, faturas, orcamentos e evolucao de 6 meses.
5. `FINANCECONTROL_JWT_SECRET` passou a sobrescrever o segredo de configuracao quando informado.
6. Transferencias entre cartoes foram bloqueadas no MVP, pois pagamento de fatura foi deliberadamente removido.
7. Lancamentos ganharam filtro por `InvoiceId`.
8. Swagger recebeu esquema Bearer para testar rotas autenticadas.
9. Conflitos de concorrencia e violacoes de unicidade sao convertidos em HTTP 409.
10. Teste de dominio para impedir transferencia envolvendo cartao foi adicionado.
11. Testes de integracao PostgreSQL foram incorporados a solucao.

## Limitacao de validacao

O ambiente usado para esta revisao nao possui o executavel do .NET SDK nem Docker. Portanto, nao foi possivel executar `dotnet build`, `dotnet test` ou os containers PostgreSQL aqui. A estrutura e as referencias foram revisadas estaticamente; a validacao final de compilacao/execucao deve ser feita em uma maquina com .NET 8 SDK e, para integracao, Docker.
