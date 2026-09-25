namespace FinanceControl.Infrastructure.Persistence.Audit;

/// <summary>
/// Registro de auditoria. NAO e uma entidade de Domain - AuditLog nunca
/// fez parte das 7 entidades definidas na etapa do Domain; e um
/// subproduto de persistencia, populado automaticamente pelo
/// SaveChangesAsync do FinanceControlDbContext (ver AuditLogEntryFactory).
///
/// Append-only por construcao: nenhum repository ou caso de uso tem
/// acesso para Update/Delete - o unico jeito de uma linha aqui existir e
/// via Add() dentro do proprio SaveChangesAsync. O bloqueio definitivo em
/// nivel de banco (REVOKE UPDATE, DELETE) sera aplicado na migration
/// inicial, ainda nao criada.
/// </summary>
public sealed class AuditLogEntry
{
    public Guid Id { get; private set; }
    public Guid OwnerScopeId { get; private set; }
    public string EntityName { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string ChangesJson { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private AuditLogEntry()
    {
        // Construtor privado sem parametros para materializacao do EF Core.
    }

    public AuditLogEntry(Guid ownerScopeId, string entityName, Guid entityId, string action, string changesJson)
    {
        Id = Guid.NewGuid();
        OwnerScopeId = ownerScopeId;
        EntityName = entityName;
        EntityId = entityId;
        Action = action;
        ChangesJson = changesJson;
        CreatedAt = DateTime.UtcNow;
    }
}
