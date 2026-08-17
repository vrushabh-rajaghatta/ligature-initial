using Acme.DocumentManagement.Domain.Aggregates.Documents;

namespace Acme.DocumentManagement.Application.Commands.UploadDocument;

public sealed record UploadDocumentResult(DocumentId Id);
