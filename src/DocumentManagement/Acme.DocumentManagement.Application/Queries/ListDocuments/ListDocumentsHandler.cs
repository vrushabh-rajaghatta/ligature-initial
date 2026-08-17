using Microsoft.EntityFrameworkCore;

using Acme.DocumentManagement.Domain.Aggregates.Documents;
using Acme.Persistence;

namespace Acme.DocumentManagement.Application.Queries.ListDocuments;

public sealed class ListDocumentsHandler
{
    private readonly AcmeDbContext _dbContext;

    public ListDocumentsHandler(AcmeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<DocumentSummary>> HandleAsync(
        ListDocumentsQuery query,
        CancellationToken cancellationToken)
    {
        // Lightweight summaries for the list — the current version's number
        // joined without loading the whole version collection. The id
        // tie-breaker keeps the order deterministic when two documents share
        // a creation instant.
        var rows = await (
            from document in _dbContext.Documents.AsNoTracking()
            orderby document.CreatedOnUtc descending, document.Id
            select new
            {
                document.Id,
                document.Name,
                document.Status,
                CurrentVersionNumber = _dbContext.Set<DocumentVersion>()
                    .Where(v => v.Id == document.CurrentVersionId)
                    .Select(v => (int?)v.VersionNumber)
                    .FirstOrDefault(),
                document.CreatedOnUtc,
            }).ToListAsync(cancellationToken);

        return rows
            .Select(row => new DocumentSummary(
                row.Id.Value,
                row.Name,
                row.Status.ToString(),
                row.CurrentVersionNumber,
                row.CreatedOnUtc))
            .ToList();
    }
}
