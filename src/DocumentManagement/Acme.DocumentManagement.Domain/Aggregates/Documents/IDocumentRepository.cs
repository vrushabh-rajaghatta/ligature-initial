namespace Acme.DocumentManagement.Domain.Aggregates.Documents;

public interface IDocumentRepository
{
    Task AddAsync(
        Document document,
        CancellationToken cancellationToken);

    Task<Document?> GetByIdAsync(
        DocumentId id,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        Document document,
        CancellationToken cancellationToken);
}
