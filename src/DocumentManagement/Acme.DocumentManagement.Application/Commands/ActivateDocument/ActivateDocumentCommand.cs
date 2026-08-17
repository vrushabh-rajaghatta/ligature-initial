using Acme.DocumentManagement.Domain.Aggregates.Documents;

namespace Acme.DocumentManagement.Application.Commands.ActivateDocument;

public sealed record ActivateDocumentCommand(DocumentId DocumentId);
