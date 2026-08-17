using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Acme.DocumentManagement.Domain.Aggregates.Documents;

namespace Acme.Persistence.Configurations.DocumentManagement;

public sealed class DocumentVersionConfiguration
    : IEntityTypeConfiguration<DocumentVersion>
{
    public void Configure(EntityTypeBuilder<DocumentVersion> builder)
    {
        builder.ToTable("DocumentVersions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new DocumentVersionId(value));

        builder.Property(x => x.VersionNumber)
            .IsRequired();

        builder.Property(x => x.OriginalFileName)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.StoredFileName)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.ContentType)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.FileSize)
            .IsRequired();

        builder.Property(x => x.StoragePath)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.Checksum)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.UploadedOnUtc)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // Shadow FK to the owning document. Declared explicitly with the
        // aggregate's strongly-typed id (and its converter) so it is
        // compatible with Document's primary key, and required so the FK
        // deletes with its parent rather than severing (the optional-FK trap
        // IdentityConventionTests documents). The ownership relationship
        // binds to it in DocumentConfiguration.
        builder.Property<DocumentId>("DocumentId")
            .HasConversion(
                id => id.Value,
                value => new DocumentId(value))
            .IsRequired();

        builder.HasIndex("DocumentId");

        // Enforces the aggregate invariant at the database level: version
        // numbers are unique within a document.
        builder.HasIndex("DocumentId", nameof(DocumentVersion.VersionNumber))
            .IsUnique();
    }
}
