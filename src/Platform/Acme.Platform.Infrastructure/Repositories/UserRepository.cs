using Microsoft.EntityFrameworkCore;

using Acme.Persistence;
using Acme.Platform.Domain.Aggregates.User;

using UserAggregate = Acme.Platform.Domain.Aggregates.User.User;
using Acme.Platform.Contracts;

namespace Acme.Platform.Infrastructure.Repositories;

/// <summary>
/// Identity-scoped: every caller passes an identity it already owns.
/// </summary>
/// <remarks>
/// An id from a signed token or a consumable grant (login, invitation
/// acceptance, password reset, change-password), or an email at the two doors
/// where no identity exists yet (sign-in, reset request — ADR-021 made email
/// unique precisely for them).
/// <para>
/// This was described as "the bypass surface for the Users table", because it
/// sat deliberately outside the tenant query filter. ADR-066 removed the
/// filter, so there is no longer anything to be outside of.
/// </para>
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
