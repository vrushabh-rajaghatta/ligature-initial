using Microsoft.EntityFrameworkCore;

using Acme.Persistence;
using Acme.Platform.Domain.Aggregates.Tenant;
using Acme.SharedKernel.Primitives;

namespace Acme.Platform.Infrastructure.Repositories;

public sealed class TenantRepository : ITenantRepository
{
    private readonly AcmeDbContext _dbContext;

    public TenantRepository(AcmeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Tenant?> GetByIdAsync(
        TenantId id,
        CancellationToken cancellationToken)
    {
        // Tenants carry no query filter — the directory is global by
        // definition (ADR-031) — so no bypass is needed here.
        return await _dbContext.Tenants
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task UpdateAsync(
        Tenant tenant,
        CancellationToken cancellationToken)
    {
        _dbContext.Tenants.Update(tenant);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
