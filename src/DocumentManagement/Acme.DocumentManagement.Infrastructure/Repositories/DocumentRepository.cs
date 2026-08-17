using Microsoft.EntityFrameworkCore;

using Acme.DocumentManagement.Domain.Aggregates.Documents;
using Acme.Persistence;

namespace Acme.DocumentManagement.Infrastructure.Repositories;

public sealed class DocumentRepository : IDocumentRepository
{
    private readonly AcmeDbContext _dbContext;

    public DocumentRepository(AcmeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        Document document,
        CancellationToken cancellationToken)
    {
        _dbContext.Documents.Add(document);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Document?> GetByIdAsync(
        DocumentId id,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Documents
            .Include(x => x.Versions)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task UpdateAsync(
        Document document,
        CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
