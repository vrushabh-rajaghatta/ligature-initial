namespace Acme.DocumentManagement.Application.Queries.GetDocument;

public sealed record DocumentDetails(
    Guid Id,
    string Name,
    string Status,
    DateTime CreatedOnUtc,
    IReadOnlyList<DocumentVersionDetails> Versions);
