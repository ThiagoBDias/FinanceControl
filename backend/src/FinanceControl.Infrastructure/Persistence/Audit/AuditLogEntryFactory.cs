using System.Text.Json;
using System.Text.Json.Serialization;
using FinanceControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FinanceControl.Infrastructure.Persistence.Audit;

/// <summary>
/// Constroi AuditLogEntry a partir do ChangeTracker, ANTES do
/// SaveChangesAsync ser chamado (precisa ler OriginalValue/CurrentValue
/// enquanto o EF ainda esta rastreando o estado "sujo"). So audita os
/// aggregates financeiros (Account, Transaction, Invoice, TransferGroup) -
/// User e Category ficam de fora (nao estao na lista de "alteracoes
/// financeiras" aprovada na revisao arquitetural).
/// </summary>
internal static class AuditLogEntryFactory
{
    private static readonly HashSet<Type> AuditedTypes = new()
    {
        typeof(Account),
        typeof(Transaction),
        typeof(Invoice),
        typeof(TransferGroup)
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public static IReadOnlyList<AuditLogEntry> Build(ChangeTracker changeTracker)
    {
        var result = new List<AuditLogEntry>();

        foreach (var entry in changeTracker.Entries())
        {
            if (!AuditedTypes.Contains(entry.Entity.GetType()))
                continue;

            if (entry.State is not (EntityState.Added or EntityState.Modified))
                continue;

            var ownerScopeId = (Guid)entry.Property("OwnerScopeId").CurrentValue!;
            var entityId = (Guid)entry.Property("Id").CurrentValue!;
            var action = entry.State == EntityState.Added ? "Created" : "Modified";
            var changes = BuildChanges(entry);
            var json = JsonSerializer.Serialize(changes, JsonOptions);

            result.Add(new AuditLogEntry(ownerScopeId, entry.Entity.GetType().Name, entityId, action, json));
        }

        return result;
    }

    private static Dictionary<string, object?> BuildChanges(EntityEntry entry)
    {
        var changes = new Dictionary<string, object?>();

        foreach (var property in entry.Properties)
        {
            if (entry.State == EntityState.Added)
            {
                changes[property.Metadata.Name] = property.CurrentValue;
            }
            else if (property.IsModified)
            {
                changes[property.Metadata.Name] = new
                {
                    Old = property.OriginalValue,
                    New = property.CurrentValue
                };
            }
        }

        return changes;
    }
}
