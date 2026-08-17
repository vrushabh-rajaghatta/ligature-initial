using Microsoft.EntityFrameworkCore;

using Acme.Persistence;
using Acme.SharedKernel.Exceptions;
using Acme.Platform.Contracts;

namespace Acme.Platform.Application.Queries.GetUserById;

/// <summary>
/// Reads a single user straight from the database: no repository, no aggregate,
/// no tracking. Projects from the flat directory read model so the query stays
/// fully translatable, exactly like the user list.
/// </summary>
public sealed class GetUserByIdHandler
{
    private readonly AcmeDbContext _dbContext;

    public GetUserByIdHandler(AcmeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserDetails> HandleAsync(
        GetUserByIdQuery query,
        CancellationToken cancellationToken)
    {
        var userId = query.UserId.Value;

        // Carried a tenant predicate until ADR-066. Every user in this
        // database belongs to this deployment, so the id alone identifies
        // them.
        var user = await _dbContext.UserDirectory
            .AsNoTracking()
            .Where(x => x.Id == userId)
            .Select(x => new UserDetails(
                x.Id,
                x.FirstName,
                x.LastName,
                x.Email,
                x.Status,
                x.CreatedOn))
            .SingleOrDefaultAsync(cancellationToken);

        return user
            ?? throw new NotFoundException(PlatformErrors.UserNotFound);
    }
}
