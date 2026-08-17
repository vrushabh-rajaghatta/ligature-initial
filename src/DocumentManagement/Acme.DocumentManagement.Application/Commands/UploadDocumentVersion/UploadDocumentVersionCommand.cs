using Acme.DocumentManagement.Domain.Aggregates.Documents;

namespace Acme.DocumentManagement.Application.Commands.UploadDocumentVersion;

public sealed record UploadDocumentVersionCommand(
    DocumentId DocumentId,
    string OriginalFileName,
    string ContentType,
    Stream Content);
