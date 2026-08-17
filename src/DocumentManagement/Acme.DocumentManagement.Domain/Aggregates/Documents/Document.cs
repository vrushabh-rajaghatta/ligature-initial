using Acme.SharedKernel.Abstractions;
using Acme.SharedKernel.Exceptions;

namespace Acme.DocumentManagement.Domain.Aggregates.Documents;

/// <summary>
/// A managed document: named content with an immutable version history.
/// </summary>
/// <remarks>
/// The aggregate owns version numbering — a version number is never accepted
/// from the outside. The stored bytes live behind <c>IFileStorage</c>; the
/// aggregate records only the facts about them (name, size, checksum, path).
/// Carried a <c>TenantId</c> until ADR-066; the database a document is stored
/// in now says whose it is.
/// </remarks>
public sealed class Document : AggregateRoot<DocumentId>
{
    public const int NameMaxLength = 200;

    private readonly List<DocumentVersion> _versions = [];

    private Document()
    {
    }

    public string Name { get; private set; } = default!;

    public DocumentStatus Status { get; private set; }

    public DocumentVersionId? CurrentVersionId { get; private set; }

    public DateTime CreatedOnUtc { get; private set; }

    // Never expose a mutable collection — version management stays inside
    // the aggregate.
    public IReadOnlyCollection<DocumentVersion> Versions
        => _versions.AsReadOnly();

    public static Document Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(DocumentErrors.DocumentNameRequired);

        var trimmedName = name.Trim();

        if (trimmedName.Length > NameMaxLength)
            throw new DomainException(DocumentErrors.DocumentNameTooLong);

        return new Document
        {
            Id = DocumentId.New(),
            Name = trimmedName,
            Status = DocumentStatus.Draft,
            CurrentVersionId = null,
            CreatedOnUtc = DateTime.UtcNow,
        };
    }

    /// <summary>Creates Version 1. Valid only when no versions exist.</summary>
    public void AddInitialVersion(
        string originalFileName,
        string storedFileName,
        string contentType,
        long fileSize,
        string storagePath,
        string checksum)
    {
        if (_versions.Count > 0)
            throw new BusinessRuleViolationException(
                DocumentErrors.DocumentAlreadyHasInitialVersion);

        AppendVersion(
            1,
            originalFileName,
            storedFileName,
            contentType,
            fileSize,
            storagePath,
            checksum);
    }

    /// <summary>
    /// Appends the next version (N+1). The aggregate owns numbering — a
    /// version number is never accepted from the outside.
    /// </summary>
    public void AddNewVersion(
        string originalFileName,
        string storedFileName,
        string contentType,
        long fileSize,
        string storagePath,
        string checksum)
    {
        if (_versions.Count == 0)
            throw new BusinessRuleViolationException(
                DocumentErrors.DocumentHasNoInitialVersion);

        var nextVersionNumber = _versions.Max(v => v.VersionNumber) + 1;

        AppendVersion(
            nextVersionNumber,
            originalFileName,
            storedFileName,
            contentType,
            fileSize,
            storagePath,
            checksum);
    }

    /// <summary>Draft -> Active. Requires a current version.</summary>
    public void Activate()
    {
        if (Status == DocumentStatus.Archived)
            throw new BusinessRuleViolationException(
                DocumentErrors.DocumentArchived);

        if (Status == DocumentStatus.Active)
            throw new BusinessRuleViolationException(
                DocumentErrors.DocumentAlreadyActive);

        // A document with no content is not something the business can
        // approve — the aggregate enforces this rather than trusting the
        // upload workflow to always have run first.
        if (CurrentVersionId is null)
            throw new BusinessRuleViolationException(
                DocumentErrors.CannotActivateWithoutVersion);

        Status = DocumentStatus.Active;
    }

    /// <summary>Active -> Archived. Archived is terminal.</summary>
    public void Archive()
    {
        if (Status == DocumentStatus.Archived)
            throw new BusinessRuleViolationException(
                DocumentErrors.DocumentArchived);

        if (Status == DocumentStatus.Draft)
            throw new BusinessRuleViolationException(
                DocumentErrors.CannotArchiveDraft);

        Status = DocumentStatus.Archived;
    }

    private void AppendVersion(
        int versionNumber,
        string originalFileName,
        string storedFileName,
        string contentType,
        long fileSize,
        string storagePath,
        string checksum)
    {
        var version = new DocumentVersion(
            DocumentVersionId.New(),
            versionNumber,
            originalFileName,
            storedFileName,
            contentType,
            fileSize,
            storagePath,
            checksum,
            DateTime.UtcNow);

        _versions.Add(version);
        CurrentVersionId = version.Id;
    }
}
