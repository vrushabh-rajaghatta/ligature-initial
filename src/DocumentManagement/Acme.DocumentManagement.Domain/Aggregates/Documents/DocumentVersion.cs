using Acme.SharedKernel.Abstractions;
using Acme.SharedKernel.Exceptions;

namespace Acme.DocumentManagement.Domain.Aggregates.Documents;

/// <summary>
/// One immutable version of a document's content. Created only by the
/// <see cref="Document"/> aggregate — there is no path for application code
/// to instantiate a version independently of its root.
/// </summary>
public sealed class DocumentVersion : Entity<DocumentVersionId>
{
    private DocumentVersion()
    {
    }

    internal DocumentVersion(
        DocumentVersionId id,
        int versionNumber,
        string originalFileName,
        string storedFileName,
        string contentType,
        long fileSize,
        string storagePath,
        string checksum,
        DateTime uploadedOnUtc)
    {
        if (string.IsNullOrWhiteSpace(originalFileName))
            throw new DomainException(DocumentErrors.OriginalFileNameRequired);

        if (string.IsNullOrWhiteSpace(storedFileName))
            throw new DomainException(DocumentErrors.StoredFileNameRequired);

        if (string.IsNullOrWhiteSpace(contentType))
            throw new DomainException(DocumentErrors.ContentTypeRequired);

        if (string.IsNullOrWhiteSpace(storagePath))
            throw new DomainException(DocumentErrors.InvalidStoragePath);

        if (fileSize <= 0)
            throw new DomainException(DocumentErrors.InvalidFileSize);

        Id = id;
        VersionNumber = versionNumber;
        OriginalFileName = originalFileName.Trim();
        StoredFileName = storedFileName.Trim();
        ContentType = contentType.Trim();
        FileSize = fileSize;
        StoragePath = storagePath.Trim();
        Checksum = checksum;
        UploadedOnUtc = uploadedOnUtc;
    }

    public int VersionNumber { get; private set; }

    public string OriginalFileName { get; private set; } = default!;

    public string StoredFileName { get; private set; } = default!;

    public string ContentType { get; private set; } = default!;

    public long FileSize { get; private set; }

    public string StoragePath { get; private set; } = default!;

    public string Checksum { get; private set; } = default!;

    public DateTime UploadedOnUtc { get; private set; }
}
