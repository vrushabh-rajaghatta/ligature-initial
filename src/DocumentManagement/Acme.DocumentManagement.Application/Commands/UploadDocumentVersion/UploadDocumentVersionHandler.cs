using System.Security.Cryptography;

using Acme.DocumentManagement.Domain.Aggregates.Documents;
using Acme.SharedKernel.Exceptions;
using Acme.Storage;

namespace Acme.DocumentManagement.Application.Commands.UploadDocumentVersion;

/// <summary>
/// Adds the next version of a document that already exists. The aggregate
/// owns the version number; this only supplies the bytes.
/// </summary>
public sealed class UploadDocumentVersionHandler
{
    private readonly IDocumentRepository _repository;
    private readonly IFileStorage _fileStorage;

    public UploadDocumentVersionHandler(
        IDocumentRepository repository,
        IFileStorage fileStorage)
    {
        _repository = repository;
        _fileStorage = fileStorage;
    }

    public async Task<UploadDocumentVersionResult> HandleAsync(
        UploadDocumentVersionCommand command,
        CancellationToken cancellationToken)
    {
        // Buffer once so the bytes hashed and the bytes stored are identical.
        using var buffer = new MemoryStream();
        await command.Content.CopyToAsync(buffer, cancellationToken);
        var bytes = buffer.ToArray();

        if (bytes.LongLength == 0)
            throw new DomainException(DocumentUploadErrors.EmptyFile);

        var document = await _repository.GetByIdAsync(
            command.DocumentId, cancellationToken);

        if (document is null)
            throw new NotFoundException(
                DocumentLifecycleErrors.DocumentDoesNotExist);

        // The next number is the aggregate's to know, but the storage path
        // needs it before AddNewVersion runs. Read from the versions the
        // aggregate already holds rather than duplicating the rule: if they
        // ever disagree, the aggregate's guard is what fails, not the file.
        var nextVersionNumber = document.Versions.Max(v => v.VersionNumber) + 1;

        var extension = Path.GetExtension(command.OriginalFileName);
        var storedFileName = $"v{nextVersionNumber}{extension}";
        var relativePath =
            $"documents/{document.Id.Value}/{storedFileName}";

        var checksum = Convert.ToHexString(SHA256.HashData(bytes))
            .ToLowerInvariant();

        await using (var content = new MemoryStream(bytes))
        {
            await _fileStorage.SaveAsync(relativePath, content, cancellationToken);
        }

        document.AddNewVersion(
            originalFileName: command.OriginalFileName,
            storedFileName: storedFileName,
            contentType: command.ContentType,
            fileSize: bytes.LongLength,
            storagePath: relativePath,
            checksum: checksum);

        try
        {
            await _repository.UpdateAsync(document, cancellationToken);
        }
        catch
        {
            // Persistence failed after the file was written — remove the
            // orphaned file so storage does not drift from the database.
            await _fileStorage.DeleteAsync(relativePath, cancellationToken);
            throw;
        }

        return new UploadDocumentVersionResult(
            document.CurrentVersionId!, nextVersionNumber);
    }
}
