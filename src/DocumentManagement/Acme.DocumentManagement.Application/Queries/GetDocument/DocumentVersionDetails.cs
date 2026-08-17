namespace Acme.DocumentManagement.Application.Queries.GetDocument;

public sealed record DocumentVersionDetails(
    Guid Id,
    int VersionNumber,
    bool IsCurrent,
    string OriginalFileName,
    string ContentType,
    long FileSize,
    string Checksum,
    DateTime UploadedOnUtc);
