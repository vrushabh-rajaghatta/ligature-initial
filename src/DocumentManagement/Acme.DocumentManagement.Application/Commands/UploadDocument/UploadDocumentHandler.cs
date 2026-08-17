using System.Security.Cryptography;

using Microsoft.EntityFrameworkCore;

using Acme.DocumentManagement.Domain.Aggregates.Documents;
using Acme.Persistence;
using Acme.SharedKernel.Abstractions;
using Acme.SharedKernel.Exceptions;
using Acme.Storage;

namespace Acme.DocumentManagement.Application.Commands.UploadDocument;

public sealed class UploadDocumentHandler
{
    private readonly AcmeDbContext _dbContext;
    private readonly IDocumentRepository _repository;
    private readonly IFileStorage _fileStorage;
    private readonly ITenantContext _tenantContext;

    public UploadDocumentHandler(
        AcmeDbContext dbContext,
        IDocumentRepository repository,
        IFileStorage fileStorage,
        ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _repository = repository;
        _fileStorage = fileStorage;
        _tenantContext = tenantContext;
    }

    public async Task<UploadDocumentResult> HandleAsync(
        UploadDocumentCommand command,
        CancellationToken cancellationToken)
    {
        // Buffer once so we can both hash and store the exact same bytes.
        using var buffer = new MemoryStream();
        await command.Content.CopyToAsync(buffer, cancellationToken);
        var bytes = buffer.ToArray();

        if (bytes.LongLength == 0)
            throw new DomainException(DocumentUploadErrors.EmptyFile);

        // The document belongs to the caller's tenant (ADR-031, ADR-024) —
        // resolved from identity, never accepted from the request body.
        var tenantId = _tenantContext.TenantId;

        // Names are unique within the tenant. The unique index is the last
        // line of defence; this check is the one that produces a 409 the
        // caller can act on.
        var trimmedName = command.Name?.Trim() ?? string.Empty;

        var nameTaken = await _dbContext.Documents
            .AsNoTracking()
            .AnyAsync(d => d.Name == trimmedName, cancellationToken);

        if (nameTaken)
            throw new BusinessRuleViolationException(
                DocumentUploadErrors.DuplicateDocumentName);

        // Create the aggregate first so we have its id for the storage path.
        var document = Document.Create(tenantId, command.Name!);

        // Deterministic stored filename + relative path mirroring ownership.
        // Original filename is preserved separately on the version.
        var extension = Path.GetExtension(command.OriginalFileName);
        var storedFileName = $"v1{extension}";
        var relativePath =
            $"documents/{document.Id.Value}/{storedFileName}";

        var checksum = Convert.ToHexString(SHA256.HashData(bytes))
            .ToLowerInvariant();

        await using (var content = new MemoryStream(bytes))
        {
            await _fileStorage.SaveAsync(relativePath, content, cancellationToken);
        }

        document.AddInitialVersion(
            originalFileName: command.OriginalFileName,
            storedFileName: storedFileName,
            contentType: command.ContentType,
            fileSize: bytes.LongLength,
            storagePath: relativePath,
            checksum: checksum);

        try
        {
            await _repository.AddAsync(document, cancellationToken);
        }
        catch
        {
            // Persistence failed after the file was written — remove the
            // orphaned file so storage does not drift from the database.
            await _fileStorage.DeleteAsync(relativePath, cancellationToken);
            throw;
        }

        return new UploadDocumentResult(document.Id);
    }
}
