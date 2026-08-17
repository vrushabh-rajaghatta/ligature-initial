namespace Acme.DocumentManagement.Application.Queries.ListDocuments;

public sealed record DocumentSummary(
    Guid Id,
    string Name,
    string Status,
    int? CurrentVersionNumber,
    DateTime CreatedOnUtc);
