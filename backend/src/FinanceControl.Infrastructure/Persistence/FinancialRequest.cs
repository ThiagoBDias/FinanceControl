using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceControl.Infrastructure.Persistence;

public sealed record FinancialRequest(Guid OwnerScopeId, Guid RequestId, string Fingerprint,
    int StatusCode, string? ContentType, string? Location, byte[] Response);

public sealed class FinancialRequestConfiguration : IEntityTypeConfiguration<FinancialRequest>
{
    public void Configure(EntityTypeBuilder<FinancialRequest> builder)
    {
        builder.HasKey(r => new { r.OwnerScopeId, r.RequestId });
        builder.Property(r => r.Fingerprint).HasMaxLength(64);
    }
}
