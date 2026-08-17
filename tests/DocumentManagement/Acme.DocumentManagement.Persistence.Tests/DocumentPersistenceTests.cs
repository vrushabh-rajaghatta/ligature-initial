using FluentAssertions;
using Microsoft.EntityFrameworkCore;

using Acme.DocumentManagement.Domain.Aggregates.Documents;
using Acme.DocumentManagement.Infrastructure.Repositories;
using Acme.SharedKernel.Abstractions;
using Acme.SharedKernel.Primitives;

namespace Acme.DocumentManagement.Persistence.Tests;

// Integration test — exercises the real EF mapping against Postgres.
//
// The database is this assembly's own: created from the current migration
// chain and seeded by the real initializers before the first test runs
// (ADR-064). Nothing here assumes a developer migrated anything by hand.
[Collection(DocumentManagementDatabase.Collection)]
public class DocumentPersistenceTests
{
    private readonly DocumentManagementDatabase _database;

    public DocumentPersistenceTests(DocumentManagementDatabase database)
    {
        _database = database;
    }

    // The first seeded demo tenant. Every context is scoped to it so the
    // global query filter (ADR-031) shows the document created here.
    private static readonly TenantId TestTenant =
        new(Guid.Parse("30000000-0000-0000-0000-000000000001"));

    private sealed class FixedTenantContext : ITenantContext
    {
        public TenantId TenantId => TestTenant;

        public TenantId? TenantIdOrNull => TestTenant;
    }

    private Acme.Persistence.AcmeDbContext NewContext() =>
        _database.NewContext(new FixedTenantContext());

    [Fact]
    public async Task Saves_reloads_and_cascade_deletes_a_document_with_its_version()
    {
        DocumentId documentId;

        // --- Save: new document + initial version, via the repository. ---
        await using (var ctx = NewContext())
        {
            var document = Document.Create(
                TestTenant,
                "Persistence Verify " + Guid.NewGuid());

            document.AddInitialVersion(
                originalFileName: "handbook.pdf",
                storedFileName: "v1.pdf",
                contentType: "application/pdf",
                fileSize: 1024,
                storagePath: "documents/x/v1.pdf",
                checksum: "sha256-v1");

            documentId = document.Id;

            var repository = new DocumentRepository(ctx);
            await repository.AddAsync(document, default);
        }

        // --- Reload from a fresh context and verify the aggregate. ---
        await using (var ctx = NewContext())
        {
            var repository = new DocumentRepository(ctx);
            var reloaded = await repository.GetByIdAsync(documentId, default);

            reloaded.Should().NotBeNull();
            reloaded!.TenantId.Should().Be(TestTenant);
            reloaded.Status.Should().Be(DocumentStatus.Draft);

            reloaded.Versions.Should().ContainSingle();
            var version = reloaded.Versions.Single();
            version.VersionNumber.Should().Be(1);
            version.OriginalFileName.Should().Be("handbook.pdf");
            version.FileSize.Should().Be(1024);
            version.StoragePath.Should().Be("documents/x/v1.pdf");

            reloaded.CurrentVersionId.Should().Be(version.Id);
        }

        // --- Cascade: deleting the document removes its versions. ---
        await using (var ctx = NewContext())
        {
            var document = await ctx.Documents
                .Include(x => x.Versions)
                .FirstAsync(x => x.Id == documentId);

            ctx.Documents.Remove(document);
            await ctx.SaveChangesAsync();
        }

        await using (var ctx = NewContext())
        {
            var documentExists = await ctx.Documents
                .AnyAsync(x => x.Id == documentId);
            documentExists.Should().BeFalse();

            var orphanVersionCount = await ctx.Set<DocumentVersion>()
                .CountAsync(v =>
                    EF.Property<DocumentId>(v, "DocumentId") == documentId);
            orphanVersionCount.Should().Be(0);
        }
    }

    [Fact]
    public async Task A_document_is_invisible_to_another_tenant()
    {
        DocumentId documentId;

        await using (var ctx = NewContext())
        {
            var document = Document.Create(
                TestTenant,
                "Isolation Verify " + Guid.NewGuid());

            var repository = new DocumentRepository(ctx);
            await repository.AddAsync(document, default);
            documentId = document.Id;
        }

        // The same read, as the second seeded tenant: the fail-closed filter
        // (ADR-031) must resolve to no rows, not to an error.
        var otherTenant = new TenantId(
            Guid.Parse("30000000-0000-0000-0000-000000000002"));

        await using (var ctx = _database.NewContext(
            new StubTenantContext(otherTenant)))
        {
            var visible = await ctx.Documents
                .AnyAsync(x => x.Id == documentId);

            visible.Should().BeFalse();
        }
    }

    private sealed class StubTenantContext : ITenantContext
    {
        private readonly TenantId _tenantId;

        public StubTenantContext(TenantId tenantId)
        {
            _tenantId = tenantId;
        }

        public TenantId TenantId => _tenantId;

        public TenantId? TenantIdOrNull => _tenantId;
    }
}
