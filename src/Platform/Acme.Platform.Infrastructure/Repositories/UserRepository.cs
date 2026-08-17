using Microsoft.EntityFrameworkCore;

using Acme.Persistence;
using Acme.Platform.Domain.Aggregates.User;

using UserAggregate = Acme.Platform.Domain.Aggregates.User.User;
using Acme.Platform.Contracts;

namespace Acme.Platform.Infrastructure.Repositories;

/// <summary>
/// Identity-scoped, deliberately outside the tenant query filter (ADR-031).
/// </summary>
/// <remarks>
/// Every caller passes an identity it already owns: an id from a signed token
/// or a consumable grant (login, invitation acceptance, password reset,
/// change-password), or an email at the two doors where no tenant exists yet
/// (sign-in, reset request — ADR-021 made email globally unique precisely for
/// them). Filtering here would break each of those flows for tenant users and
/// all of them for platform users, who match no tenant filter ever.
/// Tenant-scoped access goes through the query handlers and
/// <c>UserRepositoryExtensions.GetRequiredAsync</c>, which checks ownership
/// explicitly. This class is the entire bypass surface for the Users table.
/// </remarks>
public sealed class UserRepository : IUserRepository
{
    private readonly AcmeDbContext _dbContext;

    public UserRepository(AcmeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        UserAggregate user,
        CancellationToken cancellationToken)
    {
        _dbContext.Users.Add(user);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserAggregate?> GetByIdAsync(
        UserId id,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<UserAggregate?> GetByEmailAsync(
        Acme.Platform.Domain.ValueObjects.Email email,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }

    public async Task UpdateAsync(
        UserAggregate user,
        CancellationToken cancellationToken)
    {
        _dbContext.Users.Update(user);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
