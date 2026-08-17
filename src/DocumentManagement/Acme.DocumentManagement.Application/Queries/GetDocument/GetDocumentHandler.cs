using Microsoft.EntityFrameworkCore;

using Acme.DocumentManagement.Domain.Aggregates.Documents;
using Acme.Persistence;

namespace Acme.DocumentManagement.Application.Queries.GetDocument;

public sealed class GetDocumentHandler
{
    private readonly AcmeDbContext _dbContext;

    public GetDocumentHandler(AcmeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DocumentDetails?> HandleAsync(
        GetDocumentQuery query,
        CancellationToken cancellationToken)
    {
        var document = await _dbContext.Documents
            .AsNoTracking()
            .Include(d => d.Versions)
            .SingleOrDefaultAsync(
                d => d.Id == query.DocumentId,
                cancellationToken);

        if (document is null)
            return null;

        // Newest first — the version a reader almost always wants is on top.
        // Deterministic: version numbers are unique within a document (the
        // aggregate assigns them and the database enforces it), so this
        // ordering is total without a tie-breaker.
        var versions = document.Versions
            .OrderByDescending(v => v.VersionNumber)
            .Select(v => new DocumentVersionDetails(
                v.Id.Value,
                v.VersionNumber,
                v.Id == document.CurrentVersionId,
                v.OriginalFileName,
                v.ContentType,
                v.FileSize,
                v.Checksum,
                v.UploadedOnUtc))
            .ToList();

        return new DocumentDetails(
            document.Id.Value,
            document.Name,
            document.Status.ToString(),
            document.CreatedOnUtc,
            versions);
    }
}
