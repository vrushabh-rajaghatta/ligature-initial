using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Acme.DocumentManagement.Domain.Aggregates.Documents;

namespace Acme.Persistence.Configurations.DocumentManagement;

public sealed class DocumentConfiguration
    : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new DocumentId(value));

        builder.Property(x => x.Name)
            .HasMaxLength(Document.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        // Current-version pointer stored as a plain (converted) column — no
        // FK. Modelling it as a foreign key creates an insert/delete cycle
        // with the ownership relationship below; the aggregate already
        // guarantees the pointer only ever references one of its own
        // versions, so the invariant is enforced in the domain.
        builder.Property(x => x.CurrentVersionId)
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value != null
                    ? new DocumentVersionId(value.Value)
                    : null);

        builder.Property(x => x.CreatedOnUtc)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // Ownership: Document (1) -> DocumentVersions (N). The child has no
        // FK property, so EF uses a shadow "DocumentId".
        builder.HasMany(x => x.Versions)
            .WithOne()
            .HasForeignKey("DocumentId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Document.Versions))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.Status);

        // Document names are unique in this deployment. The handler's explicit
        // check produces the 409; this index is the last line of defence.
        // Was (TenantId, Name) until ADR-066 — see that ADR's sequencing rule:
        // narrowing this index is why the data must be split per customer
        // before this code ships.
        builder.HasIndex(x => x.Name)
            .IsUnique();
    }
}
