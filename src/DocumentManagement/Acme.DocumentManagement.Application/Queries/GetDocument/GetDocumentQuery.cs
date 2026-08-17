using Acme.DocumentManagement.Domain.Aggregates.Documents;

namespace Acme.DocumentManagement.Application.Queries.GetDocument;

public sealed record GetDocumentQuery(DocumentId DocumentId);
