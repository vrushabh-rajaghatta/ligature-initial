using Acme.DocumentManagement.Domain.Aggregates.Documents;

namespace Acme.DocumentManagement.Application.Commands.UploadDocumentVersion;

public sealed record UploadDocumentVersionResult(
    DocumentVersionId VersionId,
    int VersionNumber);
