using Acme.DocumentManagement.Domain.Aggregates.Documents;

namespace Acme.DocumentManagement.Application.Commands.ArchiveDocument;

public sealed record ArchiveDocumentCommand(DocumentId DocumentId);
