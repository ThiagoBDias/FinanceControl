# Migrações PostgreSQL

A migração inicial preserva o esquema legado. A migração de faturas e dívidas adiciona as colunas de pagamento, compra parcelada e agendamento, além das tabelas de dívidas e idempotência. Ela materializa os saldos iniciais de cartão em faturas de abertura sem alterar o saldo histórico do livro. O snapshot representa o modelo a partir dessa migração.

Aplique as migrações com o comando descrito no [guia de implantação](../../../../../DEPLOY.md). O ambiente de desenvolvimento pode configurá-las em `Database__ApplyMigrations=true`. Em produção, aplique a migração previamente usando uma role separada da role de execução da API sempre que possível. Para reverter a migração financeira, use um backup validado e concilie as movimentações criadas desde o backup.
