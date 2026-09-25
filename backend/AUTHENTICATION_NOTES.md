# Autenticação

A API agora usa ASP.NET Core Identity para credenciais e JWT Bearer para autenticação das rotas financeiras.

## Endpoints públicos

- `POST /api/auth/register`
- `POST /api/auth/login`

## Endpoint autenticado

- `GET /api/auth/me`

As rotas financeiras exigem `Authorization: Bearer <token>` e usam o `NameIdentifier` do token como `OwnerScopeId`. O cliente não pode escolher o `OwnerScopeId` de outro usuário.

## Configuração

Defina `FINANCECONTROL_JWT_SECRET` com pelo menos 32 caracteres no ambiente. O valor `CHANGE_ME_IN_ENVIRONMENT` dos arquivos `appsettings*.json` é deliberadamente inválido para impedir que uma chave padrão seja usada em produção.

O mesmo Guid do usuário é usado pelo Identity e pelo usuário de domínio financeiro, permitindo que `OwnerScopeId` continue representando o dono dos dados.
