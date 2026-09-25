# FinanceControl — limpeza do backend

## Escopo do MVP

O backend foi simplificado para concentrar o produto em:

- contas e saldos;
- lançamentos;
- categorias;
- cartões de crédito;
- faturas para visualização;
- transferências entre contas;
- dashboard/resumos;
- orçamento por categoria como próxima funcionalidade.

## Alterações desta rodada

1. Removida a operação de pagamento de fatura (`PayInvoiceUseCase`).
2. Removidos da fatura os estados e dados específicos de pagamento (`PaidAmount`, `PaidAt` e `Paid`).
3. `InvoiceResponse` agora apresenta somente os dados necessários para visualização, incluindo o total calculado pelos lançamentos.
4. Removido o vínculo `InvoiceId` de `TransferGroup`, pois ele existia apenas para a antiga operação de pagamento de fatura.
5. Mantido o `InvoiceId` em `Transaction`, pois ele é necessário para identificar os lançamentos que compõem uma fatura de cartão.
6. `CreditCardSettings` passou a ser mapeado em tabela própria, evitando o uso de table splitting com owned type opcional.
7. Categorias inativas não podem mais ser usadas em novos lançamentos.
8. `NuGet.Config` voltou a apontar para `nuget.org`.
9. EF Core foi alinhado para a versão `8.0.10`, compatível com o provider Npgsql já usado pelo projeto.

## Observação

Não há migrations no scaffold original. Portanto, as alterações de modelo ficam prontas para a primeira migration quando o banco for configurado.

A compilação/execução do projeto não foi declarada como validada nesta máquina porque o SDK `dotnet` não está disponível no ambiente de trabalho usado para esta revisão.
