# Implantação do FinanceControl

## Privacidade e custo

Para acesso externo sem endereço público, a opção preferida é executar o contêiner em uma máquina existente e publicar apenas dentro da rede privada Tailscale com Serve. O plano Personal aceita até seis usuários, sem mensalidade para uso pessoal. A máquina precisa permanecer ligada para acesso remoto. [Plano Personal](https://tailscale.com/docs/account/manage-plans/free-plans-discounts) · [Tailscale Serve](https://tailscale.com/kb/1242/tailscale-serve)

Para hospedagem gerenciada, a opção preparada é Render Free para frontend e API no mesmo contêiner, com Neon Free separado para PostgreSQL. O Render oferece 750 horas mensais por workspace e suspende o serviço após 15 minutos ociosos, com reativação de cerca de um minuto. Ao alcançar as franquias, algumas funções suspendem. Mantenha o workspace sem forma de pagamento e use apenas recursos Free. O PostgreSQL gratuito do próprio Render expira em 30 dias. [Render Free](https://render.com/docs/free) · [Contêineres no Render](https://render.com/hosting/docker)

O Neon Free oferece 1 GB de armazenamento e 100 CU-horas por projeto ao mês, suspensão do compute após 5 minutos ociosos e histórico de restauração de até 6 horas ou 1 GB de alterações. Essas cotas exigem acompanhamento e backup fora da plataforma para uso diário. [Limites do Neon](https://neon.com/pricing)

O Vercel Hobby tem proteção de acesso nos previews. O domínio de produção permanece acessível publicamente nessa modalidade. Separar o frontend do backend também exige um proxy de mesma origem para os cookies da aplicação. O contêiner integrado evita essa dependência. [Proteção de deployments na Vercel](https://vercel.com/docs/deployment-protection)

## Rede privada em uma máquina própria

1. Prepare a instalação local descrita no README. Para usar o endereço HTTPS do Tailscale, adicione `FINANCECONTROL_ENVIRONMENT=Production` a `backend/.env`. Se precisar cadastrar sua primeira conta, configure também `FINANCECONTROL_INVITE_EMAIL` e `FINANCECONTROL_INVITE_CODE` conforme o procedimento de convite abaixo.
2. Recrie a aplicação com o comando Compose do README. Instale o Tailscale na máquina e no celular, no mesmo tailnet autorizado.
3. Execute `tailscale serve --bg 8080` na máquina da aplicação. Ajuste a porta se tiver escolhido outra. Abra o endereço HTTPS mostrado pelo comando e verifique o acesso a partir de um dispositivo do tailnet. Proteja a máquina com as permissões de acesso do tailnet.
4. Após o cadastro, remova o e-mail e o código de convite e recrie o serviço. Mantenha as portas do Compose vinculadas ao loopback.

## Configuração de produção gerenciada

1. Autorize o destino e a visibilidade antes de criar recursos. Use um banco e um serviço exclusivos do FinanceControl. Escolha o ambiente de teste com dados sintéticos para a primeira publicação.
2. No Render, configure um Web Service Free com runtime Docker. Deixe o contexto de build na raiz e indique `backend/src/FinanceControl.API/Dockerfile`. Use a rota `/health` para o processo, seguida de verificações autenticadas independentes com dados sintéticos.
3. Configure `ASPNETCORE_ENVIRONMENT=Production`. Armazene `FINANCECONTROL_JWT_SECRET` e `FINANCECONTROL_CONNECTION_STRING` no gerenciador de segredos do serviço. Use pelo menos 32 bytes aleatórios para a chave JWT. No Npgsql, prefira a conexão direta do projeto PostgreSQL para migrações. Use `SSL Mode=VerifyFull` com o hostname do provedor e a cadeia de confiança do sistema.
4. Revise e aplique as migrações a partir de uma máquina autorizada. Para um banco existente, faça backup, valide o restore em ambiente isolado e compare contas, faturas e saldos antes e depois. A migração cria o saldo inicial a pagar como fatura separada. Revise compras legadas classificadas indevidamente como receitas de cartão antes da importação. Evite colocar o aplicativo em serviço até terminar a migração.
5. Para ativar o primeiro cadastro, configure temporariamente `Auth__InviteEmail` e `Auth__InviteCode`, com um código aleatório de pelo menos 32 caracteres. Cadastre a conta em `/register` e remova ambas as variáveis do serviço. Cadastro fechado é o padrão de produção.
6. Valide HTTPS, cookie `Secure` e `HttpOnly`, recusa de acesso anônimo a `/api/accounts`, revogação de acesso em navegador após sair e segregação entre usuários. Faça a validação também em um celular. O serviço gerenciado tem um hostname público e exige autorização específica de visibilidade antes de subir.
7. Rode backups criptografados continuamente e teste a restauração. Planeje rotação de segredos e alertas de consumo dos planos. Mantenha .NET, Node e dependências atualizados.

Para executar somente as migrações a partir da raiz, depois de configurar a conexão e a chave JWT no ambiente.

```sh
dotnet run --project backend/src/FinanceControl.API --no-launch-profile -- --migrate
```

Caso uma configuração antiga tenha sido utilizada em um ambiente, troque a chave JWT e as credenciais de banco antes de reabrir o acesso. O commit atual remove os valores versionados. Uma limpeza de histórico remoto altera commits públicos e deve ser aprovada separadamente.

O modelo atual de dívidas usa parcelas mensais fixas com diferença máxima de um centavo. Os contratos com parcelas personalizadas, estornos de pagamentos e cálculo automático de multa e juros continuam fora desta versão.
